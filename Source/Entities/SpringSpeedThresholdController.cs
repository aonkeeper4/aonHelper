using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.aonHelper.Entities;

// threshold by sungazer reference
[CustomEntity("aonHelper/SpringSpeedThresholdController")]
[Tracked]
public class SpringSpeedThresholdController(Vector2 position, float threshold, string flag)
    : FlagAffectedController<SpringSpeedThresholdController>(position, flag)
{
    private readonly float threshold = threshold;
    
    public SpringSpeedThresholdController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("threshold"), data.Attr("flag"))
    { }
    
    #region Hooks
    
    internal static void Load()
    {
        IL.Celeste.Player.SideBounce += Player_SideBounce;
    }

    internal static void Unload()
    {
        IL.Celeste.Player.SideBounce -= Player_SideBounce;
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
        cursor.EmitDelegate(DetermineSpeedThreshold);

        return;

        static float DetermineSpeedThreshold(float orig, Player player)
            => ControllerActive(player.SceneAs<Level>(), out SpringSpeedThresholdController controller)
                ? controller.threshold
                : orig;
    }

    #endregion
}
