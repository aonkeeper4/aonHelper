using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FeatherBounceScamController")]
[Tracked]
public class FeatherBounceScamController : Entity
{
    private float featherBounceScamThresholdMultiplier = 1f;

    private readonly string flag;

    public FeatherBounceScamController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        featherBounceScamThresholdMultiplier = data.Float("featherBounceScamThreshold") / 0.2f;
        flag = data.Attr("flag");
    }

    public static void Load()
    {
        IL.Celeste.Player.OnCollideH += mod_PlayerOnCollideH;
        IL.Celeste.Player.OnCollideV += mod_PlayerOnCollideV;
    }

    public static void Unload()
    {
        IL.Celeste.Player.OnCollideH -= mod_PlayerOnCollideH;
        IL.Celeste.Player.OnCollideV -= mod_PlayerOnCollideV;
    }

    private static void mod_PlayerOnCollideH(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Player), "starFlyTimer"), instr => instr.MatchLdcR4(0.2f));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(determineFeatherBounceScamThreshold);
        cursor.Emit(OpCodes.Mul);
    }

    private static void mod_PlayerOnCollideV(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Player), "starFlyTimer"), instr => instr.MatchLdcR4(0.2f));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(determineFeatherBounceScamThreshold);
        cursor.Emit(OpCodes.Mul);
    }

    private static float determineFeatherBounceScamThreshold(Player player)
    {
        Level level = player.SceneAs<Level>();

        FeatherBounceScamController controller = level.Tracker.GetEntity<FeatherBounceScamController>();
        if (controller is null)
        {
            return 1f;
        }
        else if (level.Session.GetFlag(controller.flag) || controller.flag == "")
        {
            return controller.featherBounceScamThresholdMultiplier;
        }
        else
        {
            return 1f;
        }
    }
}