using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FeatherBounceScamController")]
[Tracked]
public class FeatherBounceScamController(Vector2 position, float featherBounceScamThreshold, string flag)
    : FlagAffectedController<FeatherBounceScamController>(position, flag)
{
    private readonly float featherBounceScamThreshold = featherBounceScamThreshold;

    public FeatherBounceScamController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("featherBounceScamThreshold"), data.Attr("flag"))
    { }

    #region Hooks
    
    internal static void Load()
    {
        IL.Celeste.Player.OnCollideH += ControlFeatherBounceScam;
        IL.Celeste.Player.OnCollideV += ControlFeatherBounceScam;
    }

    internal static void Unload()
    {
        IL.Celeste.Player.OnCollideH -= ControlFeatherBounceScam;
        IL.Celeste.Player.OnCollideV -= ControlFeatherBounceScam;
    }

    private static void ControlFeatherBounceScam(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("starFlyTimer"),
            instr => instr.MatchLdcR4(Player.StarFlyEndNoBounceTime)))
            throw new HookHelper.HookException(il, "Unable to find check on `Player.starFlyTimer` to modify.");
        
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(FeatherBounceScamThresholdMultiplier);
        cursor.Emit(OpCodes.Mul);

        return;
        
        static float FeatherBounceScamThresholdMultiplier(Player player)
            => ControllerActive(player.SceneAs<Level>(), out FeatherBounceScamController controller)
                ? controller.featherBounceScamThreshold / Player.StarFlyEndNoBounceTime
                : 1f;
    }
    
    #endregion
}