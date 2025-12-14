using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FeatherDashSwitch")]
public class FeatherDashSwitch : DashSwitch
{
    private new readonly ParticleType P_PressA;
    private new readonly ParticleType P_PressB;

    private readonly bool allowDashPress;
    private readonly bool allowHoldablePress;

    public FeatherDashSwitch(EntityID id,
        Vector2 position, Sides side,
        bool persistent, bool allGates,
        bool allowDashPress, bool allowHoldablePress,
        string spriteDir, Color particleColor1, Color particleColor2)
        : base(position, side, persistent, allGates, id, "default")
    {
        this.allowDashPress = allowDashPress;
        this.allowHoldablePress = allowHoldablePress;
        
        Vector2 spritePos = sprite.Position;
        float spriteRot = sprite.Rotation;
        sprite.Stop();
        Remove(sprite);
        
        sprite = string.IsNullOrEmpty(spriteDir) ? aonHelperModule.SpriteBank.Create("aonHelper_featherDashSwitch") : BuildSprite(spriteDir);
        sprite.Position = spritePos;
        sprite.Rotation = spriteRot;
        Add(sprite);
        sprite.Play("idle");
        
        OnDashCollide = OnDashed;
        
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
        : this(id,
            data.Position + offset, data.Enum("side", Sides.Up),
            data.Bool("persistent"), data.Bool("allGates"),
            data.Bool("allowDashPress"), data.Bool("allowHoldablePress", true),
            data.Attr("spriteDir"), data.HexColor("particleColor1"), data.HexColor("particleColor2"))
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

    private void Press(Vector2 direction)
    {
        if (pressed || direction != pressDirection)
            return;
        
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
            
        sprite.Play("push");
        pressed = true;
            
        MoveTo(pressedTarget);
        Collidable = false;
        Position -= pressDirection * 2f;

        Vector2 particlePos = Position + sprite.Position;
        Vector2 particleSpread = direction.Perpendicular() * 6f;
        float particleRot = sprite.Rotation - (float) Math.PI;
        SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, particlePos, particleSpread, particleRot);
        SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, particlePos, particleSpread, particleRot);
        
        if (allGates)
            foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>()
                                                       .Cast<TempleGate>()
                                                       .Where(entity => entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level))
                entity.SwitchOpen();
        else
            GetGate()?.SwitchOpen();
        
        if (persistent)
            SceneAs<Level>().Session.SetFlag(FlagName);
    }

    private new DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (allowDashPress)
            Press(direction);
        
        return DashCollisionResults.NormalCollision;
    }
    
    #region Hooks
    
    internal static void Load()
    {
        On.Celeste.DashSwitch.OnDashed += DashSwitch_OnDashed;
        IL.Celeste.DashSwitch.Update += DashSwitch_Update;

        IL.Celeste.Glider.OnCollideH += FeatherDashSwitchHoldableCheck;
        IL.Celeste.TheoCrystal.OnCollideH += FeatherDashSwitchHoldableCheck;
        IL.Celeste.TheoCrystal.OnCollideV += FeatherDashSwitchHoldableCheck;
        
        IL.Celeste.Player.OnCollideH += FeatherDashSwitchPlayerCheck;
        IL.Celeste.Player.OnCollideV += FeatherDashSwitchPlayerCheck;
    }

    internal static void Unload()
    {
        On.Celeste.DashSwitch.OnDashed -= DashSwitch_OnDashed;
        IL.Celeste.DashSwitch.Update -= DashSwitch_Update;
        
        IL.Celeste.Glider.OnCollideH -= FeatherDashSwitchHoldableCheck;
        IL.Celeste.TheoCrystal.OnCollideH -= FeatherDashSwitchHoldableCheck;
        IL.Celeste.TheoCrystal.OnCollideV -= FeatherDashSwitchHoldableCheck;
        
        IL.Celeste.Player.OnCollideH -= FeatherDashSwitchPlayerCheck;
        IL.Celeste.Player.OnCollideV -= FeatherDashSwitchPlayerCheck;
    }
    
    private static DashCollisionResults DashSwitch_OnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Vector2 direction)
        => self is not FeatherDashSwitch ? orig(self, player, direction) : DashCollisionResults.NormalCollision;

    private static void DashSwitch_Update(ILContext il)
    {
        ILCursor cursor = new(il);

        /*
         * IL_0032: ldarg.0
         * IL_0033: ldloc.0
         * IL_0034: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::get_UnitY()
         * IL_0039: callvirt instance valuetype Celeste.DashCollisionResults Celeste.DashSwitch::OnDashed(class Celeste.Player, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_003e: pop
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdloc0(),
            instr => instr.MatchCall<Vector2>("get_UnitY"),
            instr => instr.MatchCallvirt<DashSwitch>("OnDashed"),
            instr => instr.MatchPop()))
            throw new HookHelper.HookException(il, "Unable to find call to `DashSwitch.OnDashed` to modify.");

        ILLabel afterOnDashed = cursor.DefineLabel();

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ShouldSkipOnDashed);
        cursor.EmitBrtrue(afterOnDashed);
        cursor.GotoNext(MoveType.After, instr => instr.MatchPop());
        cursor.MarkLabel(afterOnDashed);

        return;

        static bool ShouldSkipOnDashed(DashSwitch dashSwitch)
        {
            if (dashSwitch is not FeatherDashSwitch { allowHoldablePress: true } featherDashSwitch)
                return false;

            featherDashSwitch.Press(Vector2.UnitY);
            return true;
        }
    }

    private static void FeatherDashSwitchHoldableCheck(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchCallvirt<DashCollision>("Invoke"),
            instr => instr.MatchPop()))
            throw new HookHelper.HookException(il, "Unable to find call to `DashCollision.Invoke` to modify.");

        ILLabel afterInvokeDashCollision = cursor.DefineLabel();

        cursor.EmitDup();
        cursor.EmitLdarg1();
        cursor.EmitDelegate(ShouldSkipInvokeDashCollision);
        cursor.EmitBrtrue(afterInvokeDashCollision);
        cursor.GotoNext(MoveType.Before, instr => instr.MatchPop());
        cursor.MarkLabel(afterInvokeDashCollision);
        
        Logger.Info(nameof(aonHelper) + "/featehr dahs switch", il.ToString());

        return;

        static bool ShouldSkipInvokeDashCollision(Vector2 collisionDir, CollisionData data)
        {
            if (data.Hit is not FeatherDashSwitch { allowHoldablePress: true } featherDashSwitch)
                return false;
            
            featherDashSwitch.Press(collisionDir);
            return true;
        }
    }

    private static void FeatherDashSwitchPlayerCheck(ILContext il)
    {
        ILCursor cursor = new(il);
        
        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("starFlyTimer")))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.starFlyTimer`.");
        
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(CheckHit);

        return;
        
        static void CheckHit(CollisionData data)
        {
            if (data.Hit is not FeatherDashSwitch dashSwitch)
                return;
            
            dashSwitch.Press(data.Direction);
        }
    }
    
    #endregion
}