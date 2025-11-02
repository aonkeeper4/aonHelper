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
public class FeatherBounceScamController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    private readonly float featherBounceScamThresholdMultiplier = data.Float("featherBounceScamThreshold") / Player.StarFlyEndNoBounceTime;

    private readonly string flag = data.Attr("flag");

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
        cursor.EmitDelegate(FeatherBounceScamThreshold);
        cursor.Emit(OpCodes.Mul);

        return;
        
        static float FeatherBounceScamThreshold(Player player)
        {
            Level level = player.SceneAs<Level>();

            FeatherBounceScamController controller = level.Tracker.GetEntity<FeatherBounceScamController>();
            if (controller is null)
                return 1f;
            
            if (level.Session.GetFlag(controller.flag) || controller.flag == "")
                return controller.featherBounceScamThresholdMultiplier;
            
            return 1f;
        }
    }
    
    #endregion
}