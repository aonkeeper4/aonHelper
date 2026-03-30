namespace Celeste.Mod.aonHelper.Entities;

// threshold by sungazer reference
[CustomEntity("aonHelper/SpringSpeedThresholdController")]
[Tracked]
public class SpringSpeedThresholdController(Vector2 position, float thresholdX, float thresholdY, string condition)
    : ConditionalController<SpringSpeedThresholdController>(position, condition)
{
    private readonly Vector2 threshold = new(thresholdX, thresholdY);
    
    public SpringSpeedThresholdController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("thresholdX", data.Float("threshold", 240f)), data.Float("thresholdY"), data.Attr("flag"))
    { }
    
    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.Player.SideBounce += Player_SideBounce;
        IL.Celeste.Spring.OnCollide += Spring_OnCollide;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.Player.SideBounce -= Player_SideBounce;
        IL.Celeste.Spring.OnCollide -= Spring_OnCollide;
    }

    private static void Player_SideBounce(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0000: ldarg.0
         * IL_0001: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.Player::Speed
         * IL_0006: ldfld float32 [FNA]Microsoft.Xna.Framework.Vector2::X
         * IL_000b: call float32 [mscorlib]System.Math::Abs(float32)
         * IL_0010: ldc.r4 240
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdflda<Player>("Speed"),
            instr => instr.MatchLdfld<Vector2>("X"),
            instr => instr.MatchCall(typeof(System.Math), "Abs"),
            instr => instr.MatchLdcR4(240f)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.Speed` to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(DetermineXSpeedThreshold);

        return;

        static float DetermineXSpeedThreshold(float orig, Player player)
            => ControllerActive(player.SceneAs<Level>(), out SpringSpeedThresholdController controller)
                ? controller.threshold.X
                : orig;
    }

    private static void Spring_OnCollide(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0020: ldarg.1
         * IL_0021: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.Player::Speed
         * IL_0026: ldfld float32 [FNA]Microsoft.Xna.Framework.Vector2::Y
         * IL_002b: ldc.r4 0.0
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg1(),
            instr => instr.MatchLdflda<Player>("Speed"),
            instr => instr.MatchLdfld<Vector2>("Y"),
            instr => instr.MatchLdcR4(0f)))
            throw new HookHelper.HookException(il, "Unable to find check for `Player.Speed` to modify.");
        
        cursor.EmitLdarg0();
        cursor.EmitDelegate(DetermineYSpeedThreshold);

        return;

        static float DetermineYSpeedThreshold(float orig, Player player)
            => ControllerActive(player.SceneAs<Level>(), out SpringSpeedThresholdController controller)
                ? controller.threshold.Y
                : orig;
    }

    #endregion
}
