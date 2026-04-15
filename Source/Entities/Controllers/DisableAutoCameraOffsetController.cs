using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/DisableAutoCameraOffsetController")]
[Tracked]
public class DisableAutoCameraOffsetController(
    Vector2 position, string condition,
    bool disableAutoCameraOffset, bool disableCameraUpdate)
    : ConditionalController<DisableAutoCameraOffsetController>(position, condition)
{
    private readonly bool disableAutoCameraOffset = disableAutoCameraOffset, disableCameraUpdate = disableCameraUpdate;
    
    public DisableAutoCameraOffsetController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("flag"),
            data.Bool("disableAutoCameraOffset", true), data.Bool("disableCameraUpdate"))
    { }

    #region Hooks
    
    private static ILHook il_Player_get_CameraTarget;
    private static ILHook il_Player_orig_Update;

    [OnLoad]
    internal static void Load()
    {
        il_Player_get_CameraTarget = new ILHook(typeof(Player).GetMethod("get_CameraTarget", BindingFlags.Public | BindingFlags.Instance)!, IL_Player_get_CameraTarget);
        il_Player_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance)!, IL_Player_orig_Update);
    }

    [OnUnload]
    internal static void Unload()
    {
        HookHelper.DisposeAndSetNull(ref il_Player_get_CameraTarget);
        HookHelper.DisposeAndSetNull(ref il_Player_orig_Update);
    }

    private static void IL_Player_get_CameraTarget(ILContext il)
    {
        ILCursor cursor = new(il);
        
        // add check for controller being active to the check for StReflectionFall
        /*
         * IL_0062: ldarg.0
         * IL_0063: ldfld class Monocle.StateMachine Celeste.Player::StateMachine
         * IL_0068: callvirt instance int32 Monocle.StateMachine::get_State()
         * IL_0032: ldc.i4.s 18
         * IL_0034: beq.s IL_0062
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(Player.StReflectionFall),
            instr => instr.MatchBeq(out _)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.StateMachine.State` to modify.");
        
        ILCursor setCameraOffsetLabelCursor = cursor.Clone();
        ILLabel setCameraOffset = setCameraOffsetLabelCursor.DefineLabel();
        
        /*
         * IL_0036: ldloc.1
         * IL_0037: ldarg.0
         * IL_0038: ldfld class Celeste.Level Celeste.Player::level
         * IL_003d: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.Level::CameraOffset
         */
        if (!setCameraOffsetLabelCursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdloc(1),
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<Player>("level"),
            instr => instr.MatchLdflda<Level>("CameraOffset")))
            throw new HookHelper.HookException(il, "Unable to find assignment to local `vector` to branch to.");
        
        setCameraOffsetLabelCursor.MarkLabel(setCameraOffset);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(ShouldDisableAutoCameraOffset);
        cursor.Emit(OpCodes.Brtrue, setCameraOffset);
        
        // skip all the state-specific camera offsets if the controller is active
        /*
         * IL_0062: ldarg.0
         * IL_0063: ldfld class Monocle.StateMachine Celeste.Player::StateMachine
         * IL_0068: callvirt instance int32 Monocle.StateMachine::get_State()
         * IL_006d: ldc.i4.s 19
         * IL_006f: bne.un.s IL_00ae
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(Player.StStarFly),
            instr => instr.MatchBneUn(out _)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.StateMachine.State` to modify.");
        
        ILCursor skipStateOffsetLabelCursor = cursor.Clone();
        ILLabel skipStateOffset = skipStateOffsetLabelCursor.DefineLabel();

        /*
         * IL_013c: ldarg.0
         * IL_013d: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.Player::CameraAnchorLerp
         * IL_0142: call instance float32 [FNA]Microsoft.Xna.Framework.Vector2::Length()
         * IL_0147: ldc.r4 0.0
         * IL_014c: ble.un IL_024d
         */
        if (!skipStateOffsetLabelCursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdflda<Player>("CameraAnchorLerp"),
            instr => instr.MatchCall<Vector2>("Length"),
            instr => instr.MatchLdcR4(0f),
            instr => instr.MatchBleUn(out _)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.CameraAnchorLerp.Length()` to branch to.");
        
        skipStateOffsetLabelCursor.MarkLabel(skipStateOffset);
        
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(ShouldDisableAutoCameraOffset);
        cursor.Emit(OpCodes.Brtrue, skipStateOffset);

        return;
        
        static bool ShouldDisableAutoCameraOffset(Player player)
            => TryGetActiveController(player.SceneAs<Level>(), out DisableAutoCameraOffsetController controller)
                && controller.disableAutoCameraOffset;
    }

    private static void IL_Player_orig_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_105f: ldarg.0
         * IL_1060: callvirt instance bool Celeste.Player::get_InControl()
         * IL_1065: brtrue.s IL_1072
         * IL_1067: ldarg.0
         * IL_1068: ldfld bool Celeste.Player::ForceCameraUpdate
         * IL_106d: brfalse IL_1110
         */
        ILLabel failedCheck = null;
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Player>("get_InControl"),
            instr => instr.MatchBrtrue(out ILLabel _),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("ForceCameraUpdate"),
            instr => instr.MatchBrfalse(out failedCheck)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.ForceCameraUpdate` to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ShouldSkipCameraUpdate);
        cursor.EmitBrtrue(failedCheck);

        return;

        static bool ShouldSkipCameraUpdate(Player player)
            => TryGetActiveController(player.SceneAs<Level>(), out DisableAutoCameraOffsetController controller)
                && controller.disableCameraUpdate;
    }

    #endregion
}