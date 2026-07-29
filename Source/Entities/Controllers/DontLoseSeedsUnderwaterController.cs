namespace Celeste.Mod.aonHelper.Entities.Controllers;

[GlobalHelper.GlobalEntity("aonHelper/DontLoseSeedsUnderwaterController", "global")]
[Tracked]
public class DontLoseSeedsUnderwaterController(string condition)
    : ConditionalController<DontLoseSeedsUnderwaterController>(condition)
{
    public DontLoseSeedsUnderwaterController(EntityData data, Vector2 offset)
        : this(data.Attr("flag"))
    { }

    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.StrawberrySeed.Update += IL_StrawberrySeed_Update;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.StrawberrySeed.Update -= IL_StrawberrySeed_Update;
    }

    private static void IL_StrawberrySeed_Update(ILContext il) {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_LoseShards")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(PreventLosingSeeds);
        }

        return;

        static bool PreventLosingSeeds(bool orig, StrawberrySeed seed)
            => orig && (!TryGetController(seed.SceneAs<Level>(), out _) || !seed.player.CollideCheck<Water>());
    }

    #endregion
}
