namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/IntroFacingController")]
[Tracked]
public class IntroFacingController(Vector2 position, Facings facing, string condition)
    : ConditionalController<IntroFacingController>(position, condition)
{
    private readonly Facings facing = facing;
    
    public IntroFacingController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum("facing", Facings.Right), data.Attr("flag"))
    { }
    
    #region Hooks

    private static ILHook ilHook_Player_orig_Added;

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.TempleFallUpdate += Player_TempleFallUpdate;
        
        ilHook_Player_orig_Added = new ILHook(typeof(Player).GetMethod("orig_Added", HookHelper.Bind.PublicInstance)!, Player_orig_Added);
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.TempleFallUpdate -= Player_TempleFallUpdate;
        
        HookHelper.DisposeAndSetNull(ref ilHook_Player_orig_Added);
    }

    private static int Player_TempleFallUpdate(On.Celeste.Player.orig_TempleFallUpdate orig, Player self)
    {
        int result = orig(self);

        if (TryGetActiveController(self.SceneAs<Level>(), out IntroFacingController controller))
            self.Facing = controller.facing;

        return result;
    }

    private static void Player_orig_Added(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_015a: ldarg.0
         * IL_015b: ldc.i4.0
         * IL_015c: stfld valuetype Celeste.Player/IntroTypes Celeste.Player::IntroType
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdcI4(0),
            instr => instr.MatchStfld<Player>("IntroType")))
            throw new HookHelper.HookException(il, "Unable to find assignment to `Player.IntroType`.");
        
        cursor.EmitLdarg0();
        cursor.EmitDelegate(SetPlayerFacing);

        return;

        static void SetPlayerFacing(Player player)
        {
            if (TryGetActiveController(player.SceneAs<Level>(), out IntroFacingController controller, true)
                && player.IntroType is not (Player.IntroTypes.Transition or Player.IntroTypes.Respawn or Player.IntroTypes.None)) // not sure if these are the only ones used in gameplay?
                player.Facing = controller.facing;
        }
    }

    #endregion
}
