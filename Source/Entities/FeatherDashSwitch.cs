using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using MonoMod.Cil;
using MonoMod;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

// i really hate to do this but i don't know how to preserve the legacy behaviour + it's been used in too many maps to just  fix the bugs
[CustomEntity("aonHelper/FeatherDashSwitchV2")]
public class FeatherDashSwitch : DashSwitch
{
    private new readonly ParticleType P_PressA, P_PressB;

    private readonly bool dashActivated, holdableActivated, featherActivated;

    public enum RefillBehavior
    {
        None,
        Refill,
        TwoDashRefill
    }
    private readonly RefillBehavior refillBehavior;
    
    private readonly string flagOnPress;

    public FeatherDashSwitch(EntityID id, Vector2 position, Sides side,
        bool dashActivated, bool holdableActivated, bool featherActivated,
        RefillBehavior refillBehavior, string flagOnPress,
        bool persistent, bool allGates,
        string spriteDir, Color particleColor1, Color particleColor2)
        : base(position, side, persistent, allGates, id, "default")
    {
        OnDashCollide = OnDashed;
        
        this.dashActivated = dashActivated;
        this.holdableActivated = holdableActivated;
        this.featherActivated = featherActivated;

        this.refillBehavior = refillBehavior;
        
        this.flagOnPress = string.IsNullOrEmpty(flagOnPress) ? null : flagOnPress;
        
        Vector2 spritePos = sprite.Position;
        float spriteRot = sprite.Rotation;
        sprite.Stop();
        Remove(sprite);
        
        sprite = string.IsNullOrEmpty(spriteDir)
            ? aonHelperGFX.SpriteBank.Create("aonHelper_featherDashSwitch")
            : BuildSprite(spriteDir);
        sprite.Position = spritePos;
        sprite.Rotation = spriteRot;
        sprite.Play("idle");
        Add(sprite);
        
        P_PressA = new ParticleType
        {
            Color = particleColor1,
            Color2 = particleColor2,
            ColorMode = ParticleType.ColorModes.Blink,
            Size = 1f,
            SizeRange = 0f,
            SpeedMin = 60f,
            SpeedMax = 80f,
            LifeMin = 0.8f,
            LifeMax = 1.2f,
            DirectionRange = 0.7f,
            SpeedMultiplier = 0.2f
        };
        P_PressB = new ParticleType(P_PressA)
        {
            SpeedMin = 100f,
            SpeedMax = 110f,
            DirectionRange = 0.35f
        };
    }

    public FeatherDashSwitch(EntityData data, Vector2 offset, EntityID id)
        : this(id, data.Position + offset, data.Enum("side", Sides.Up),
            data.Bool("dashActivated"), data.Bool("holdableActivated"), data.Bool("featherActivated", true),
            data.Enum("refillBehavior", RefillBehavior.None), data.Attr("flagOnPress"),
            data.Bool("persistent"), data.Bool("allGates"),
            data.Attr("spriteDir"), data.HexColor("particleColor1", Calc.HexToColor("ff8000")), data.HexColor("particleColor2", Calc.HexToColor("ffd65c")))
    { }

    private static Sprite BuildSprite(string spriteDir)
    {
        Sprite sprite = new(GFX.Game, spriteDir);
            
        // <Loop id="idle" path="" delay="0.08" frames="0-20"/>
        sprite.AddLoop("idle", "", 0.08f, Enumerable.Range(0, 21).ToArray());
        // <Loop id="pushed" path="" delay="0.08" frames="27"/>
        sprite.AddLoop("pushed", "", 0.08f, 27);
        // <Anim id="push" path="" delay="0.07" frames="21-27" goto="pushed"/>
        sprite.Add("push", "", 0.07f, "pushed", Enumerable.Range(21, 7).ToArray());
        
        return sprite;
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Awake(Monocle.Scene)")]
    private extern void base_Awake(Scene scene);
    
    public override void Awake(Scene scene)
    {
        base_Awake(scene);

        if (!SceneAs<Level>().Session.GetFlag(flagOnPress ?? FlagName))
            return;
        
        if (persistent)
        {
            sprite.Play("pushed");
            Position = pressedTarget - pressDirection * 2f;
            Collidable = false;
            pressed = true;
            
            if (allGates)
                foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>()
                                                   .Cast<TempleGate>()
                                                   .Where(entity => entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level))
                    entity.StartOpen();
            else
                GetGate()?.StartOpen();
        }
        else
            SceneAs<Level>().Session.SetFlag(flagOnPress ?? FlagName, false);
    }

    [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
    private extern void base_Update();

    public override void Update()
    {
        base_Update();
        
        if (pressed
            || side is not Sides.Down
            || !dashActivated && !holdableActivated)
            return;
        
        if (GetPlayerOnTop() is { } player)
        {
            if (holdableActivated && player.Holding is not null)
                Press(player, Vector2.UnitY);
            else
            {
                if (speedY < 0f)
                    speedY = 0f;
                speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
                
                if (!playerWasOn)
                    Audio.Play(SFX.game_05_gatebutton_depress, Position);
            }

            playerWasOn = true;
        }
        else
        {
            if (speedY > 0f)
                speedY = 0f;
            speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
            MoveTowardsY(startY, (0f - speedY) * Engine.DeltaTime);
            
            if (playerWasOn)
                Audio.Play(SFX.game_05_gatebutton_return, Position);

            playerWasOn = false;
        }
    }

    private void Press(Player player, Vector2 direction)
    {
        if (pressed || direction != pressDirection) 
            return;
        
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Audio.Play(SFX.game_05_gatebutton_activate, Position);
            
        sprite.Play("push");
        MoveTo(pressedTarget);
        Position -= pressDirection * 2f;
        Collidable = false;
        pressed = true;

        Vector2 particlePos = Position + sprite.Position;
        Vector2 particleSpread = direction.Perpendicular() * 6f;
        float particleRot = sprite.Rotation - MathF.PI;
        SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, particlePos, particleSpread, particleRot);
        SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, particlePos, particleSpread, particleRot);

        switch (refillBehavior)
        {
            case RefillBehavior.None:
                break;
            
            case RefillBehavior.Refill when player?.UseRefill(false) ?? false:
                Audio.Play(SFX.game_gen_diamond_touch, Position);
                break;
            
            case RefillBehavior.TwoDashRefill when player?.UseRefill(true) ?? false:
                Audio.Play(SFX.game_10_pinkdiamond_touch, Position);
                break;
        }
        
        if (allGates)
            foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>()
                                                       .Cast<TempleGate>()
                                                       .Where(entity => entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level))
                entity.SwitchOpen();
        else
            GetGate()?.SwitchOpen();
        
        if (flagOnPress is not null || persistent)
            SceneAs<Level>().Session.SetFlag(flagOnPress ?? FlagName);

        // make it work with crystalline all dash switch temple gates
        OnDashCollide(player, direction);
    }

    // ensure only our code can activate feather dash switches
    private static new DashCollisionResults OnDashed(Player player, Vector2 dir)
        => DashCollisionResults.NormalCollision;
    
    #region Hooks
    
    internal static void Load()
    {
        On.Celeste.DashSwitch.OnDashed += DashSwitch_OnDashed;

        On.Celeste.Glider.OnCollideH += Glider_OnCollideH;

        On.Celeste.TheoCrystal.OnCollideH += TheoCrystal_OnCollideH;
        On.Celeste.TheoCrystal.OnCollideV += TheoCrystal_OnCollideV;

        On.Celeste.Seeker.SlammedIntoWall += Seeker_SlammedIntoWall;
        
        IL.Celeste.Player.OnCollideH += Player_OnCollideHV;
        IL.Celeste.Player.OnCollideV += Player_OnCollideHV;
    }

    internal static void Unload()
    {
        On.Celeste.DashSwitch.OnDashed -= DashSwitch_OnDashed;
        
        On.Celeste.Glider.OnCollideH -= Glider_OnCollideH;

        On.Celeste.TheoCrystal.OnCollideH -= TheoCrystal_OnCollideH;
        On.Celeste.TheoCrystal.OnCollideV -= TheoCrystal_OnCollideV;

        On.Celeste.Seeker.SlammedIntoWall -= Seeker_SlammedIntoWall;
        
        IL.Celeste.Player.OnCollideH -= Player_OnCollideHV;
        IL.Celeste.Player.OnCollideV -= Player_OnCollideHV;
    }

    // ensure only our code can activate feather dash switches
    // not sure if anyone actually calls this? but better to be safe than sorry
    private static DashCollisionResults DashSwitch_OnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Vector2 direction)
        => self is FeatherDashSwitch ? DashCollisionResults.NormalCollision : orig(self, player, direction);
    
    private static void Glider_OnCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
        => PressFeatherDashSwitch(() => orig(self, data), data, () => self.Speed, Vector2.UnitX, featherDashSwitch => featherDashSwitch.holdableActivated);
    
    private static void TheoCrystal_OnCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        => PressFeatherDashSwitch(() => orig(self, data), data, () => self.Speed, Vector2.UnitX, featherDashSwitch => featherDashSwitch.holdableActivated);
    private static void TheoCrystal_OnCollideV(On.Celeste.TheoCrystal.orig_OnCollideV orig, TheoCrystal self, CollisionData data)
        => PressFeatherDashSwitch(() => orig(self, data), data, () => self.Speed, Vector2.UnitY, featherDashSwitch => featherDashSwitch.holdableActivated);
    
    private static void Seeker_SlammedIntoWall(On.Celeste.Seeker.orig_SlammedIntoWall orig, Seeker self, CollisionData data)
        => PressFeatherDashSwitch(() => orig(self, data), data, () => self.Speed, Vector2.UnitX, featherDashSwitch => featherDashSwitch.dashActivated);

    private static void PressFeatherDashSwitch(Action callOrig, CollisionData data, Func<Vector2> speedGetter, Vector2 direction, Func<FeatherDashSwitch, bool> condition)
    {
        if (data.Hit is FeatherDashSwitch featherDashSwitch && condition(featherDashSwitch))
            featherDashSwitch.Press(null, direction * MathF.Sign(Vector2.Dot(speedGetter(), direction)));

        callOrig();
    }
    
    private static void Player_OnCollideHV(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0007: ldarg.0
         * IL_0008: ldfld class Monocle.StateMachine Celeste.Player::StateMachine
         * IL_000d: callvirt instance int32 Monocle.StateMachine::get_State()
         * IL_0012: ldc.i4.s 19
         * IL_0014: bne.un.s IL_0062
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(Player.StStarFly),
            instr => instr.MatchBneUn(out _)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.StateMachine.State` to insert feather dash switch press after.");

        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate(PressFeatherDashSwitchOnFeather);
        
        /*
         * IL_00c5: ldarg.1
         * IL_00c6: ldfld valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.CollisionData::Direction
         * IL_00cb: callvirt instance valuetype Celeste.DashCollisionResults Celeste.DashCollision::Invoke(class Celeste.Player, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_00d0: stloc.0
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg1(),
            instr => instr.MatchLdfld<CollisionData>("Direction"),
            instr => instr.MatchCallvirt<DashCollision>("Invoke"),
            instr => instr.MatchStloc0()))
            throw new HookHelper.HookException(il, "Unable to find call to `DashCollision.Invoke` to insert feather dash switch press after.");

        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate(PressFeatherDashSwitchOnDash);

        return;
        
        static void PressFeatherDashSwitchOnFeather(Player player, CollisionData data)
        {
            if (data.Hit is FeatherDashSwitch { featherActivated: true } featherDashSwitch)
                featherDashSwitch.Press(player, data.Direction);
        }
        
        static void PressFeatherDashSwitchOnDash(Player player, CollisionData data)
        {
            if (data.Hit is FeatherDashSwitch { dashActivated: true } featherDashSwitch)
                featherDashSwitch.Press(player, data.Direction);
        }
    }
    
    #endregion
}