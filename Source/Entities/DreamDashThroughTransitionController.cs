using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/DreamDashThroughTransitionController")]
[Tracked]
public class DreamDashThroughTransitionController(Vector2 position, string flag) : Entity(position)
{
    private readonly string flag = string.IsNullOrEmpty(flag) ? null : flag;
    
    public DreamDashThroughTransitionController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("flag"))
    { }
    
    #region Hooks

    private static ILHook ilHook_Player_orig_Update;
    
    internal static void Load()
    {
        On.Celeste.Player.OnBoundsH += Player_OnBoundsH;
        On.Celeste.Player.OnBoundsV += Player_OnBoundsV;
        
        IL.Celeste.Player.BeforeUpTransition += Player_BeforeUpTransition;
        IL.Celeste.Player.BeforeDownTransition += Player_BeforeDownTransition;
        IL.Celeste.Player.TransitionTo += Player_TransitionTo;
        ilHook_Player_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update", HookHelper.Bind.PublicInstance)!, Player_orig_Update);
        
        IL.Celeste.Level.EnforceBounds += Level_EnforceBounds;
    }

    internal static void Unload()
    {
        On.Celeste.Player.OnBoundsH -= Player_OnBoundsH;
        On.Celeste.Player.OnBoundsV -= Player_OnBoundsV;
        
        IL.Celeste.Player.BeforeUpTransition -= Player_BeforeUpTransition;
        IL.Celeste.Player.BeforeDownTransition -= Player_BeforeDownTransition;
        IL.Celeste.Player.TransitionTo -= Player_TransitionTo;
        HookHelper.DisposeAndSetNull(ref ilHook_Player_orig_Update);
        
        IL.Celeste.Level.EnforceBounds -= Level_EnforceBounds;
    }

    private static bool ShouldAffectStateCheck(Player player)
    {
        Level level = player.SceneAs<Level>();
        return level.Tracker.GetEntity<DreamDashThroughTransitionController>() is { } controller
            && (level.Session.GetFlag(controller.flag) || controller.flag is null);
    }
    
    private static bool IsAffectedAndDreamDashing(Player player)
        => ShouldAffectStateCheck(player) && player.StateMachine.State == Player.StDreamDash;
    
    private static void Player_OnBoundsH(On.Celeste.Player.orig_OnBoundsH orig, Player self)
    {
        if (IsAffectedAndDreamDashing(self))
        {
            DreamDashDie(self, self.Position);
            return;
        }

        orig(self);
    }

    private static void Player_OnBoundsV(On.Celeste.Player.orig_OnBoundsV orig, Player self)
    {
        if (IsAffectedAndDreamDashing(self))
        {
            DreamDashDie(self, self.Position);
            return;
        }

        orig(self);
    }
    
    private static void DreamDashDie(Player player, Vector2 previousPos, bool evenIfInvincible = false)
    {
        if (!evenIfInvincible && SaveData.Instance.Assists.Invincible)
        {
            player.Position = previousPos;
            player.Speed *= -1f;
            player.Play(SFX.game_assist_dreamblockbounce);
        }

        player.Die(Vector2.Zero, evenIfInvincible);
    }
    
    private static void Player_BeforeUpTransition(ILContext il)
    {
        ILCursor cursor = new(il);

        HookHelper.ModifyStateCheck(cursor, Player.StRedDash, false, false, Player.StDreamDash, ShouldAffectStateCheck);
        HookHelper.ModifyStateCheck(cursor, Player.StRedDash, false, false, Player.StDreamDash, ShouldAffectStateCheck);
    }

    private static void Player_BeforeDownTransition(ILContext il)
    {
        ILCursor cursor = new(il);

        HookHelper.ModifyStateCheck(cursor, Player.StRedDash, false, false, Player.StDreamDash, ShouldAffectStateCheck);
    }

    private static void Player_TransitionTo(ILContext il)
    {
        ILCursor cursor = new(il);

        // IL_0013: call instance void Celeste.Actor::MoveTowardsX(float32, float32, class Celeste.Collision)
        if (!cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCall<Actor>("MoveTowardsX")))
            throw new HookHelper.HookException(il, "Unable to find call to `Actor.MoveTowardsX`.");
        UseInsteadIfDreamDashing(cursor, NaiveMoveTowardsX);
        
        // IL_002b: call instance void Celeste.Actor::MoveTowardsY(float32, float32, class Celeste.Collision)
        if (!cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCall<Actor>("MoveTowardsY")))
            throw new HookHelper.HookException(il, "Unable to find call to `Actor.MoveTowardsY`.");
        UseInsteadIfDreamDashing(cursor, NaiveMoveTowardsY);
        
        return;

        static void UseInsteadIfDreamDashing<T>(ILCursor cursor, T cb) where T : Delegate
        {
            ILLabel normalCall = cursor.DefineLabel();
            ILLabel afterNormalCall = cursor.DefineLabel();

            cursor.EmitLdarg0();
            cursor.EmitDelegate(IsAffectedAndDreamDashing);
            cursor.EmitBrfalse(normalCall);
            cursor.EmitDelegate(cb);
            cursor.EmitBr(afterNormalCall);
            cursor.MarkLabel(normalCall);
            cursor.Index++; // normal method call would be here
            cursor.MarkLabel(afterNormalCall);
        }
    }

    private static void NaiveMoveTowardsX(Player player, float targetX, float maxAmount, Collision _)
    {
        float toX = Calc.Approach(player.ExactPosition.X, targetX, maxAmount);
        float moveX = (float) ((double) toX - player.Position.X - player.movementCounter.X);
        player.NaiveMove(Vector2.UnitX * moveX);
    }

    private static void NaiveMoveTowardsY(Player player, float targetY, float maxAmount, Collision _)
    {
        float toY = Calc.Approach(player.ExactPosition.Y, targetY, maxAmount);
        float moveY = (float) ((double) toY - player.Position.Y - player.movementCounter.Y);
        player.NaiveMove(Vector2.UnitY * moveY);
    }

    private static void Player_orig_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        
        // could probably also do this with a HookHelper.ModifyStateCheck
        /*
         * IL_11c6: ldarg.0
         * IL_11c7: ldfld class Monocle.StateMachine Celeste.Player::StateMachine
         * IL_11cc: callvirt instance int32 Monocle.StateMachine::get_State()
         * IL_11d1: ldc.i4.s 9
         * IL_11d3: beq.s IL_11e9
         * IL_11d5: ldarg.0
         * IL_11d6: ldfld bool Celeste.Player::EnforceLevelBounds
         * IL_11db: brfalse.s IL_11e9
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(Player.StDreamDash),
            instr => instr.MatchBeq(out _),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("EnforceLevelBounds"),
            instr => instr.MatchBrfalse(out _)))
            throw new HookHelper.HookException(il, "Unable to find state check for `Player.StDreamDash`.");
        
        ILLabel nextCondition = cursor.DefineLabel();

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ShouldAffectStateCheck);
        cursor.EmitBrtrue(nextCondition);

        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchBeq(out _)))
            throw new HookHelper.HookException("Unable to find check for `Player.EnforceLevelBounds`.");

        cursor.MarkLabel(nextCondition);
    }

    private static void Level_EnforceBounds(ILContext il)
    {
        ILCursor cursor = new(il);

        /*
         * IL_03c8: ldarg.1
         * IL_03c9: ldarg.1
         * IL_03ca: ldfld valuetype [FNA]Microsoft.Xna.Framework.Vector2 Monocle.Entity::Position
         * IL_03cf: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::get_UnitY()
         * IL_03d4: ldc.r4 4
         * IL_03d9: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Multiply(valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32)
         * IL_03de: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_03e3: callvirt instance bool Monocle.Entity::CollideCheck<class Celeste.Solid>(valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_03e8: brtrue IL_047b
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg1(),
            instr => instr.MatchLdarg1(),
            instr => instr.MatchLdfld<Entity>("Position"),
            instr => instr.MatchCall<Vector2>("get_UnitY"),
            instr => instr.MatchLdcR4(4f),
            instr => instr.MatchCall<Vector2>("op_Multiply"),
            instr => instr.MatchCall<Vector2>("op_Addition"),
            instr => instr.MatchCallvirt<Entity>("CollideCheck")))
            throw new HookHelper.HookException(il, "Unable to find collision check for solids on down transition to modify.");

        cursor.EmitLdarg1();
        cursor.EmitDelegate(IsAffectedAndDreamDashing);
        cursor.EmitNot();
        cursor.EmitAnd();
    }

    #endregion
}
