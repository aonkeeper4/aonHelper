using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/IntroTypeFacingController")]
[Tracked]
public class IntroTypeFacingController(Vector2 position, Facings facing) : Entity(position)
{
    private readonly Facings facing = facing;
    
    public IntroTypeFacingController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum("facing", Facings.Right))
    { }
    
    #region Hooks

    private static ILHook ilHook_Player_orig_Added;

    internal static void Load()
    {
        ilHook_Player_orig_Added = new ILHook(typeof(Player).GetMethod("orig_Added", HookHelper.Bind.PublicInstance)!, Player_orig_Added);
    }

    internal static void Unload()
    {
        HookHelper.DisposeAndSetNull(ref ilHook_Player_orig_Added);
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
            if (player.Scene.Tracker.GetEntities<IntroTypeFacingController>()
                                    .Concat(player.Scene.Entities.ToAdd)
                                    .FirstOrDefault(e => e is IntroTypeFacingController)
                is IntroTypeFacingController controller)
                player.Facing = controller.facing;
        }
    }

    #endregion
}
