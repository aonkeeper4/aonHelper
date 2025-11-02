using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/DarkerMatter")]
[Tracked]
public class DarkerMatter : Entity
{
    private class Edge
    {
        public enum EdgeType
        {
            Normal,
            Warp
        }

        private readonly DarkerMatter parent;

        private readonly EdgeType type;

        public Vector2 Start;
        public Vector2 End;

        public Edge(DarkerMatter parent, EdgeType type, Vector2 start, Vector2 end)
        {
            this.parent = parent;
            this.type = type;
            Start = start;
            End = end;
        }
        
        public Color Color(Level level, int cycleOffset)
        {
            return type switch
            {
                EdgeType.Normal => parent.ColorCycle(level, cycleOffset),
                EdgeType.Warp => parent.WarpColorCycle(level, cycleOffset),
                _ => throw new Exception($"invalid edge type: {type}")
            };
        }
        
        public void Draw(uint seed, Color color)
        {
            seed += (uint)(Start.GetHashCode() + End.GetHashCode());
        
            float length = (End - Start).Length();
            Vector2 dir = (End - Start) / length;
            Vector2 offsetDir = dir.TurnRight();
            Vector2 offsetA = parent.Position + Start + offsetDir;
            Vector2 offsetB = parent.Position + End + offsetDir;
        
            Vector2 currentLineStart = offsetA;
            int offsetSign = PseudoRand(ref seed) % 2u != 0 ? 1 : -1;
            float drawnEdgeLength = 0f;

            do
            {
                float currentLineEndOffset = PseudoRandRange(ref seed, 0f, 4f);
                drawnEdgeLength += 4f + currentLineEndOffset;
                Vector2 currentLineEnd = offsetA + dir * drawnEdgeLength;
            
                if (drawnEdgeLength < length)
                    currentLineEnd += offsetSign * offsetDir * currentLineEndOffset - offsetDir;
                else
                    currentLineEnd = offsetB;
            
                Monocle.Draw.Line(currentLineStart, currentLineEnd, color, 1f);
                currentLineStart = currentLineEnd;
                offsetSign = -offsetSign;
            }
            while (drawnEdgeLength < length);
        }
        
        private static uint PseudoRand(ref uint seed)
        {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            return seed;
        }

        private static float PseudoRandRange(ref uint seed, float min, float max)
            => min + (PseudoRand(ref seed) & 0x3FFu) / 1024f * (max - min);
    }

    private ParticleType P_DarkerMatter;
    
    private readonly bool warpHorizontal, warpVertical;
    private readonly bool refillDash;
    
    private readonly float speedThreshold;
    private readonly float speedLimit;

    private readonly Color[] colors;
    private readonly Color[] warpColors;
    
    private readonly bool lonely;
    private List<Edge> edges;
    private uint edgeSeed;
    
    private float totalTime;

    public DarkerMatter(Vector2 position, int width, int height,
        bool warpHorizontal, bool warpVertical, bool refillDash,
        float speedThreshold, float speedLimit,
        Color[] colors, Color[] warpColors) : base(position)
    {
        this.warpHorizontal = warpHorizontal;
        this.warpVertical = warpVertical;
        this.refillDash = refillDash;
        lonely = this.warpHorizontal || this.warpVertical;
        
        this.speedThreshold = speedThreshold;
        this.speedLimit = speedLimit;
        
        this.colors = colors;
        this.warpColors = warpColors;

        Tag = Tags.TransitionUpdate;
        Depth = -8000;
        Collider = new Hitbox(width, height);
        
        Add(new PlayerCollider(OnPlayer));
        Add(new CustomBloom(OnRenderBloom));
        
        P_DarkerMatter = new ParticleType(Glider.P_Glow)
        {
            Color = Calc.Random.Choose(colors),
            Color2 = Calc.Random.Choose(colors) * 0.6f,
        };
    }

    public DarkerMatter(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height,
            data.Bool("warpHorizontal"), data.Bool("warpVertical"), data.Bool("refillDash", true),
            data.Float("speedThreshold"), data.Float("speedLimit"),
            data.Attr("colors").Split(",").Select(Calc.HexToColor).ToArray(), data.Attr("warpColors").Split(",").Select(Calc.HexToColor).ToArray())
    { }

    public override void Awake(Scene scene)
    {
        edges = [];
        edges.AddRange(WalkEdge(warpHorizontal ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, Vector2.Zero, Vector2.UnitY, -Vector2.UnitX * 8, Height));
        edges.AddRange(WalkEdge(warpHorizontal ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, Vector2.UnitX * Width, Vector2.UnitY, Vector2.Zero, Height));
        edges.AddRange(WalkEdge(warpVertical ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, Vector2.Zero, Vector2.UnitX, -Vector2.UnitY * 8, Width));
        edges.AddRange(WalkEdge(warpVertical ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, Vector2.UnitY * Height, Vector2.UnitX, Vector2.Zero, Width));
    }

    private List<Edge> WalkEdge(Edge.EdgeType type, Vector2 start, Vector2 walkDir, Vector2 checkOffset, float distance)
    {
        List<Edge> builtEdges = [];
        Edge currentEdge = null;

        for (int i = 0; i < distance; i += 8)
        {
            Vector2 segmentStart = start + i * walkDir;
            Vector2 segmentEnd = start + (i + 8) * walkDir;

            if (!CheckForDarkerMatter(Position + segmentStart + checkOffset)) {
                if (currentEdge is null)
                    currentEdge = new Edge(this, type, segmentStart, segmentEnd);
                else
                    currentEdge.End = segmentEnd;
            }
            else if (currentEdge is not null)
            {
                builtEdges.Add(currentEdge);
                currentEdge = null;
            }
        }
        
        if (currentEdge is not null)
            builtEdges.Add(currentEdge);
        return builtEdges;
    }
    
    private bool CheckForDarkerMatter(Vector2 pos)
        => !lonely && Scene.Tracker.GetEntities<DarkerMatter>().Cast<DarkerMatter>().Any(entity =>
            !entity.lonely && entity.Collider.Collide(new Rectangle((int) pos.X, (int) pos.Y, 8, 8)));

    public override void Update()
    {
        base.Update();
        
        totalTime += Engine.DeltaTime;
        if (Scene.OnInterval(0.1f))
        {
            edgeSeed = (uint)Calc.Random.Next();
            
            int numParticles = (int)MathF.Ceiling(Width * Height * 4 / (32 * 32));
            SceneAs<Level>().ParticlesFG.Emit(P_DarkerMatter, numParticles, Center, new Vector2(Width / 2f, Height / 2f));
        }
    }

    public override void Render()
    {
        base.Render();

        Level level = SceneAs<Level>();
        Camera camera = level.Camera;
        
        Rectangle view = new((int)camera.Left - 4, (int)camera.Top - 4, (int)(camera.Right - camera.Left) + 8, (int)(camera.Bottom - camera.Top) + 8);
        if (!Collider.Bounds.Intersects(view))
            return;
        
        Draw.Rect(Collider, ColorCycle(level, 0) * 0.4f);
        foreach (Edge edge in edges)
        {
            edge.Draw(edgeSeed, edge.Color(level, 0));
            edge.Draw(edgeSeed + 1, edge.Color(level, 5));
        }
    }
    
    private Color ColorCycle(Level level, int offset)
    {
        float time = (totalTime + offset) % 10;
        int timeInt = (int)time;
        return level is not null
            ? Color.Lerp(colors[timeInt % colors.Length], colors[(timeInt + 1) % colors.Length], time % 1f)
            : default;
    }

    private Color WarpColorCycle(Level level, int offset)
    {
        float time = (totalTime + offset) % 10;
        int timeInt = (int)time;
        return level is not null
            ? Color.Lerp(warpColors[timeInt % warpColors.Length], warpColors[(timeInt + 1) % warpColors.Length], time % 1f)
            : default;
    }
    
    private void OnPlayer(Player player)
    {
        if (player.Speed.Length() >= speedThreshold && player.StateMachine.State != StDarkerMatter)
            player.StateMachine.State = StDarkerMatter;
    }

    private void OnRenderBloom()
    { 
        Draw.Rect(Collider, Color.White * 0.1f);
    }
    
    #region States
    
    public static int StDarkerMatter { get; private set; } = -1;

    private class DarkerMatterComponent() : Component(false, false)
    {
        public DarkerMatter LastDarkerMatter;

        public const float StopGraceThreshold = 0.01f;
        public const float StopGraceTime = 0.05f;
        public float StopGraceTimer;

        public Vector2 PreviousExactPosition;
        public Vector2 LastNonZeroSpeed;
        
        public Sprite WarpSprite;
        public static readonly Vector2 WarpSpriteOffset = new(16f, 24f);
    }

    private static class DarkerMatterState
    {
        public static void DarkerMatterBegin(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return;
            
            Vector2 playerVelocity = (player.ExactPosition - darkerMatterComponent.PreviousExactPosition) / Engine.DeltaTime;

            darkerMatterComponent.LastDarkerMatter = player.CollideFirst<DarkerMatter>();
            darkerMatterComponent.StopGraceTimer = DarkerMatterComponent.StopGraceTime;
            darkerMatterComponent.LastNonZeroSpeed = player.Speed != Vector2.Zero ? player.Speed : playerVelocity;
            darkerMatterComponent.WarpSprite.Visible = true;
        }

        public static void DarkerMatterEnd(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return;
            
            if (darkerMatterComponent.LastDarkerMatter?.refillDash ?? false)
                player.RefillDash();

            darkerMatterComponent.LastDarkerMatter = null;
            darkerMatterComponent.WarpSprite.Visible = false;
        }

        public static int DarkerMatterUpdate(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return Player.StNormal;

            darkerMatterComponent.WarpSprite.Play("boost");

            DarkerMatter last = player.CollideFirst<DarkerMatter>();
            if (last is null)
                return Player.StNormal;

            // wrap check
            if (last is { warpHorizontal: true })
            {
                if (player.Center.X <= last.Left && player.Speed.X < 0)
                    player.NaiveMove(last.Width * Vector2.UnitX);
                else if (player.Center.X >= last.Right && player.Speed.X > 0)
                    player.NaiveMove(-last.Width * Vector2.UnitX);
            }
            if (last is { warpVertical: true })
            {
                if (player.Center.Y <= last.Top && player.Speed.Y < 0)
                    player.NaiveMove(last.Height * Vector2.UnitY);
                else if (player.Center.Y >= last.Bottom && player.Speed.Y > 0)
                    player.NaiveMove(-last.Height * Vector2.UnitY);
            }

            if (darkerMatterComponent.StopGraceTimer <= 0f)
            {
                if (SaveData.Instance.Assists.Invincible)
                    DarkerMatterAssistBounce(player);
                else
                    player.Die(Vector2.Zero);
            }
            
            if (player.Speed.Length() < DarkerMatterComponent.StopGraceThreshold)
                darkerMatterComponent.StopGraceTimer -= Engine.DeltaTime;
            else
            {
                darkerMatterComponent.StopGraceTimer = DarkerMatterComponent.StopGraceTime;
                darkerMatterComponent.LastNonZeroSpeed = player.Speed;
            }

            Vector2 speed = darkerMatterComponent.LastNonZeroSpeed;
            float magnitude = last.speedLimit >= 0 ? Math.Clamp(speed.Length(), 0f, last.speedLimit) : speed.Length();
            Vector2 direction = speed.SafeNormalize();
            player.Speed = direction * magnitude;
            
            darkerMatterComponent.LastDarkerMatter = last;
            
            return StDarkerMatter;
        }
        
        private static void DarkerMatterAssistBounce(Player player) {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return;
            
            player.Speed = darkerMatterComponent.LastNonZeroSpeed * -1f;
            player.Play(SFX.game_assist_dreamblockbounce);
        }
    }

    #endregion
    
    #region Hooks

    private static ILHook ilHook_Player_orig_Update;
    private static ILHook ilHook_Player_orig_UpdateSprite;
    
    internal static void Load()
    {
        // everest events
        Everest.Events.Player.OnRegisterStates += OnRegisterStates;
        Everest.Events.Player.OnSpawn += OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;

        // player hooks
        On.Celeste.Player.UnderwaterMusicCheck += Player_UnderwaterMusicCheck;
        On.Celeste.Player.Update += Player_Update;
        
        ilHook_Player_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update", HookHelper.Bind.PublicInstance)!, Player_orig_Update);
        ilHook_Player_orig_UpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", HookHelper.Bind.NonPublicInstance)!, Player_orig_UpdateSprite);
    }

    internal static void Unload()
    {
        Everest.Events.Player.OnRegisterStates -= OnRegisterStates;
        Everest.Events.Player.OnSpawn -= OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload -= OnBeforeReload;
        
        On.Celeste.Player.UnderwaterMusicCheck -= Player_UnderwaterMusicCheck;
        On.Celeste.Player.Update -= Player_Update;
        
        HookHelper.DisposeAndSetNull(ref ilHook_Player_orig_Update);
        HookHelper.DisposeAndSetNull(ref ilHook_Player_orig_UpdateSprite);
    }

    #region Events
    
    private static void OnRegisterStates(Player player)
    {
        StDarkerMatter = player.AddState("DarkerMatter", DarkerMatterState.DarkerMatterUpdate, null, DarkerMatterState.DarkerMatterBegin, DarkerMatterState.DarkerMatterEnd);
    }

    private static void OnSpawn(Player player)
    {
        if (player.Get<DarkerMatterComponent>() is not null)
            return;
        
        DarkerMatterComponent darkerMatterComponent = new()
        {
            WarpSprite = aonHelperModule.SpriteBank.Create("aonHelper_darkerMatterWarp")
        };
        darkerMatterComponent.WarpSprite.Visible = false;
        darkerMatterComponent.WarpSprite.Origin = DarkerMatterComponent.WarpSpriteOffset;
        
        player.Add(darkerMatterComponent);
        player.Add(darkerMatterComponent.WarpSprite);
    }

    private static void OnBeforeReload(bool silent)
    {
        if (Engine.Scene?.Tracker?.GetEntity<Player>() is { } player)
            player.Remove(player.Get<DarkerMatterComponent>());
    }
    
    #endregion
    
    #region Player

    private static bool Player_UnderwaterMusicCheck(On.Celeste.Player.orig_UnderwaterMusicCheck orig, Player self)
        => orig(self) || self.StateMachine.State == StDarkerMatter;

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        if (self.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
        {
            orig(self);
            return;
        }
        
        orig(self);
        
        darkerMatterComponent.PreviousExactPosition = self.ExactPosition;
    }

    private static void Player_orig_Update(ILContext il) => CheckState(new ILCursor(il), Player.StHitSquash, false, false);
    private static void Player_orig_UpdateSprite(ILContext il) => CheckState(new ILCursor(il), Player.StCassetteFly, false, false);
    
    private static void CheckState(ILCursor cursor, int state, bool equal, bool canShortCircuit)
    {
        if (!cursor.TryGotoNextFirstFitReversed(MoveType.AfterLabel, 0x10,
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(state)))
            return;

        ILLabel failedCheck = null;

        ILCursor cloned = cursor.Clone();
        if (!cloned.TryGotoNext(MoveType.After, instr => equal ^ canShortCircuit ? instr.MatchBneUn(out failedCheck) : instr.MatchBeq(out failedCheck)))
            return;
        Instruction afterMatch = cloned.Next!;

        ILLabel cleanUpPlayer = cursor.DefineLabel(), pastCleanUpPlayer = cursor.DefineLabel();

        cursor.Emit(OpCodes.Dup);
        cursor.EmitDelegate(StateCheck);
        cursor.Emit(OpCodes.Brtrue, cleanUpPlayer);

        cursor.Goto(equal ^ canShortCircuit ? afterMatch : failedCheck.Target);
        cursor.Emit(OpCodes.Br, pastCleanUpPlayer);
        cursor.Emit(OpCodes.Pop);
        cursor.MarkLabel(pastCleanUpPlayer);
        cursor.Index--;
        cursor.MarkLabel(cleanUpPlayer);
        
        cursor.Goto(afterMatch, MoveType.After);
        return;
        
        static bool StateCheck(Player player) => player.StateMachine.State == StDarkerMatter;
    }

    #endregion

    #endregion
}