using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/ReboundModifyController")]
[Tracked]
public class ReboundModifyController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    private struct ReboundData
    {
        public enum Mode
        {
            Multiplier,
            Constant,
        }
        public Mode xMode, yMode;

        public float xModifier, yModifier;
    }

    private readonly ReboundData leftRightData = new()
    {
        xMode = (ReboundData.Mode)data.Int("leftRightXMode", 0),
        yMode = (ReboundData.Mode)data.Int("leftRightYMode", 1),
        xModifier = data.Float("leftRightXModifier", data.Bool("reflectSpeed") ? -data.Float("reflectSpeedMultiplier", 0.5f) : 1f),
        yModifier = data.Float("leftRightYModifier", -120f),
    };
    private readonly ReboundData topData = new()
    {
        xMode = (ReboundData.Mode)data.Int("topXMode", 1),
        yMode = (ReboundData.Mode)data.Int("topYMode", 0),
        xModifier = data.Float("topXModifier", 0f),
        yModifier = data.Float("topYModifier", 1f),
    };

    private readonly bool refillDash = data.Bool("refillDash");

    private readonly string flag = data.Attr("flag");
    
    #region Hooks

    internal static void Load()
    {
        IL.Celeste.Player.Rebound += Player_Rebound;
    }

    internal static void Unload()
    {
        IL.Celeste.Player.Rebound -= Player_Rebound;
    }

    private static void Player_Rebound(ILContext il)
    {
        ILCursor cursor = new(il);

        ILLabel afterSpeedSet = cursor.DefineLabel();
        
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate(CustomRebound);
        cursor.EmitBrtrue(afterSpeedSet);
        
        /*
         * IL_0013: ldarg.0
         * IL_0014: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.Player::Speed
         * IL_0019: ldc.r4 -120
         * IL_001e: stfld float32 [FNA]Microsoft.Xna.Framework.Vector2::Y
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdflda<Player>("Speed"),
            instr => instr.MatchLdcR4(Player.ReboundSpeedY),
            instr => instr.MatchStfld<Vector2>("Y")))
            throw new HookHelper.HookException(il, "Unable to find assignment to `Player.Speed.Y`.");

        cursor.MarkLabel(afterSpeedSet);

        return;

        static bool CustomRebound(Player player, int direction)
        {
            Level level = player.SceneAs<Level>();

            ReboundModifyController controller = level.Tracker.GetEntity<ReboundModifyController>();
            if (controller is null || !level.Session.GetFlag(controller.flag) && controller.flag != "")
                return false;
            
            player.Speed = direction switch
            {
                0 => UpdateSpeed(player.Speed, controller.topData),
                1 or -1 => UpdateSpeed(player.Speed, controller.leftRightData),
                _ => player.Speed
            };
            if (controller.refillDash)
                player.RefillDash();

            return true;
        }
    }

    private static Vector2 UpdateSpeed(Vector2 input, ReboundData data)
        => new(data.xMode switch
            {
                ReboundData.Mode.Multiplier => input.X * data.xModifier,
                ReboundData.Mode.Constant => data.xModifier,
                _ => throw new ArgumentOutOfRangeException()
            },
            data.yMode switch
            {
                ReboundData.Mode.Multiplier => input.Y * data.yModifier,
                ReboundData.Mode.Constant => data.yModifier,
                _ => throw new ArgumentOutOfRangeException()
            });
    
    #endregion
}