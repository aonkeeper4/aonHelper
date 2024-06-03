using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Celeste.Mod.aonHelper.Entities
{

    [CustomEntity("aonHelper/ReboundModifyController")]
    [Tracked]
    public class ReboundModifyController : Entity
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

        private readonly ReboundData leftRightData;
        private readonly ReboundData topData;

        private readonly bool refillDash;

        private readonly string flag;

        public ReboundModifyController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            leftRightData = new ReboundData
            {
                xMode = (ReboundData.Mode)data.Int("leftRightXMode", 0),
                yMode = (ReboundData.Mode)data.Int("leftRightYMode", 1),
                xModifier = data.Float("leftRightXModifier", data.Bool("reflectSpeed") ? -data.Float("reflectSpeedMultiplier", 0.5f) : 1f),
                yModifier = data.Float("leftRightYModifier", -120f),
            };
            topData = new ReboundData
            {
                xMode = (ReboundData.Mode)data.Int("topXMode", 1),
                yMode = (ReboundData.Mode)data.Int("topYMode", 0),
                xModifier = data.Float("topXModifier", 0f),
                yModifier = data.Float("topYModifier", 1f),
            };
            refillDash = data.Bool("refillDash");
            flag = data.Attr("flag");
        }

        public static void Load()
        {
            On.Celeste.Player.Rebound += mod_PlayerRebound;
        }

        public static void Unload()
        {
            On.Celeste.Player.Rebound -= mod_PlayerRebound;
        }

        private static void mod_PlayerRebound(On.Celeste.Player.orig_Rebound orig, Player self, int direction)
        {
            ReboundModifyController controller = self.SceneAs<Level>().Tracker.GetEntity<ReboundModifyController>();
            if (controller is null)
            {
                orig(self, direction);
                return;
            }
            if (!self.SceneAs<Level>().Session.GetFlag(controller.flag) && controller.flag != "")
            {
                orig(self, direction);
                return;
            }

            switch (direction)
            {
                case 0:
                    self.Speed = UpdateSpeed(self.Speed, controller.topData);
                    break;
                case 1:
                case -1:
                    self.Speed = UpdateSpeed(self.Speed, controller.leftRightData);
                    break;
            }
            if (controller.refillDash)
                self.RefillDash();

            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
            self.wallSlideTimer = 1.2f;
            self.wallBoostTimer = 0f;
            self.launched = false;
            self.lowFrictionStopTimer = 0.15f;
            self.forceMoveXTimer = 0f;
            self.StateMachine.State = 0;
        }

        private static Vector2 UpdateSpeed(Vector2 input, ReboundData data)
        {
            return new(
                data.xMode switch
                {
                    ReboundData.Mode.Multiplier => input.X * data.xModifier,
                    ReboundData.Mode.Constant => data.xModifier,
                    _ => throw new UnreachableException(),
                },
                data.yMode switch
                {
                    ReboundData.Mode.Multiplier => input.Y * data.yModifier,
                    ReboundData.Mode.Constant => data.yModifier,
                    _ => throw new UnreachableException(),
                }
            );
        }
    }
}