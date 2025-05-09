using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
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

        private Vector2 a;
        private Vector2 b;

        public Edge(DarkerMatter parent, EdgeType type, Vector2 a, Vector2 b)
        {
            this.parent = parent;
            this.type = type;
            this.a = a;
            this.b = b;
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
            seed += (uint)(a.GetHashCode() + b.GetHashCode());
        
            float length = (b - a).Length();
            Vector2 dir = (b - a) / length;
            Vector2 offsetDir = dir.TurnRight();
            Vector2 offsetA = a + offsetDir;
            Vector2 offsetB = b + offsetDir;
        
            Vector2 currentLineStart = offsetA;
            int offsetSign = PseudoRand(ref seed) % 2u != 0 ? 1 : -1;
            float drawnEdgeLength = 0f;

            do
            {
                float currentLineEndOffset = PseudoRandRange(ref seed, 0f, 4f);
                drawnEdgeLength += 4f + currentLineEndOffset;
                Vector2 currentLineEnd = offsetA + dir * drawnEdgeLength;
            
                if (drawnEdgeLength < length)
                {
                    currentLineEnd += offsetSign * offsetDir * currentLineEndOffset - offsetDir;
                }
                else
                {
                    currentLineEnd = offsetB;
                }
            
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
        {
            return min + (PseudoRand(ref seed) & 0x3FFu) / 1024f * (max - min);
        }
    }

    private ParticleType P_DarkerMatter;
    
    private readonly bool warpHorizontal, warpVertical;
    private readonly bool refillDash;
    
    private readonly float speedThreshold;
    private readonly float speedLimit;

    private readonly Color[] colors;
    private readonly Color[] warpColors;
    
    private readonly Edge[] edges;
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
        this.speedThreshold = speedThreshold;
        this.speedLimit = speedLimit;
        this.colors = colors;
        this.warpColors = warpColors;

        Tag = Tags.TransitionUpdate;
        Depth = -8000;
        Collider = new Hitbox(width, height);
        
        edges = [
            new Edge(this, this.warpHorizontal ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, new Vector2(Left, Bottom), new Vector2(Left, Top)),
            new Edge(this, this.warpHorizontal ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, new Vector2(Right, Top), new Vector2(Right, Bottom)),
            new Edge(this, this.warpVertical ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, new Vector2(Left, Top), new Vector2(Right, Top)),
            new Edge(this, this.warpVertical ? Edge.EdgeType.Warp : Edge.EdgeType.Normal, new Vector2(Right, Bottom), new Vector2(Left, Bottom)),
        ];

        P_DarkerMatter = new(Glider.P_Glow)
        {
            Color = Calc.Random.Choose(colors),
            Color2 = Calc.Random.Choose(colors) * 0.6f,
        };
        
        Add(new PlayerCollider(OnPlayer));
        Add(new CustomBloom(OnRenderBloom));
    }

    public DarkerMatter(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height,
            data.Bool("warpHorizontal"), data.Bool("warpVertical"), data.Bool("refillDash", true),
            data.Float("speedThreshold"), data.Float("speedLimit"),
            data.Attr("colors").Split(",").Select(Calc.HexToColor).ToArray(), data.Attr("warpColors").Split(",").Select(Calc.HexToColor).ToArray())
    { }

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
    
    public static int StDarkerMatter { get; private set; }  = -1;

    public class DarkerMatterComponent() : Component(false, false)
    {
        public DarkerMatter LastDarkerMatter;
        
        public const float StopGraceTime = 0.05f;
        public float StopGraceTimer;
        
        public Sprite WarpSprite;
        public static readonly Vector2 WarpSpriteOffset = new(16f, 24f);
    }

    private static class DarkerMatterState
    {

        public static void DarkerMatterBegin(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return;

            darkerMatterComponent.LastDarkerMatter = null;
            darkerMatterComponent.StopGraceTimer = DarkerMatterComponent.StopGraceTime;
            darkerMatterComponent.WarpSprite.Visible = true;
        }

        public static void DarkerMatterEnd(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return;
            
            if (darkerMatterComponent.LastDarkerMatter.refillDash)
                player.RefillDash();
            
            darkerMatterComponent.WarpSprite.Visible = false;
        }

        public static int DarkerMatterUpdate(Player player)
        {
            if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
                return Player.StNormal;

            darkerMatterComponent.WarpSprite.Play("boost");

            bool shouldEnterDarkerMatterState = false;

            if (player.CollideFirst<DarkerMatter>() is { } darkerMatter)
            {
                darkerMatterComponent.LastDarkerMatter = darkerMatter;
                shouldEnterDarkerMatterState = true;
            }

            // wrap check
            DarkerMatter last = darkerMatterComponent.LastDarkerMatter;
            if (last is { warpHorizontal: true })
            {
                if (player.Center.X <= last.Left && player.Speed.X < 0)
                {
                    player.NaiveMove(last.Width * Vector2.UnitX);
                    shouldEnterDarkerMatterState = true;
                }
                else if (player.Center.X >= last.Right && player.Speed.X > 0)
                {
                    player.NaiveMove(-last.Width * Vector2.UnitX);
                    shouldEnterDarkerMatterState = true;
                }
            }
            if (last is { warpVertical: true })
            {
                if (player.Center.Y <= last.Top && player.Speed.Y < 0)
                {
                    player.NaiveMove(last.Height * Vector2.UnitY);
                    shouldEnterDarkerMatterState = true;
                }
                else if (player.Center.Y >= last.Bottom && player.Speed.Y > 0)
                {
                    player.NaiveMove(-last.Height * Vector2.UnitY);
                    shouldEnterDarkerMatterState = true;
                }
            }

            if (shouldEnterDarkerMatterState)
            {
                if (darkerMatterComponent.StopGraceTimer <= 0f)
                    player.Die(Vector2.Zero, true);

                if (player.Speed == Vector2.Zero)
                    darkerMatterComponent.StopGraceTimer -= Engine.DeltaTime;
                else
                    darkerMatterComponent.StopGraceTimer = DarkerMatterComponent.StopGraceTime;

                float speedLimit = darkerMatterComponent.LastDarkerMatter.speedLimit;
                float amplitude = speedLimit >= 0 ? Math.Clamp(player.Speed.Length(), 0f, speedLimit) : player.Speed.Length();
                Vector2 unitMovement = player.Speed.SafeNormalize();
                player.Speed = unitMovement * amplitude;

                return StDarkerMatter;
            }

            return Player.StNormal;
        }
    }

    #endregion
    
    #region Hooks

    public static void Load()
    {
        Everest.Events.Player.OnRegisterStates += OnRegisterStates;
        Everest.Events.Player.OnSpawn += OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;
    }

    public static void Unload()
    {
        Everest.Events.Player.OnRegisterStates -= OnRegisterStates;
        Everest.Events.Player.OnSpawn -= OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload -= OnBeforeReload;
    }

    private static void OnRegisterStates(Player player)
    {
        StDarkerMatter = player.AddState("DarkerMatter", DarkerMatterState.DarkerMatterUpdate, null, DarkerMatterState.DarkerMatterBegin, DarkerMatterState.DarkerMatterEnd);
    }

    private static void OnSpawn(Player player)
    {
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
}