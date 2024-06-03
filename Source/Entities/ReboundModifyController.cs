using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

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
            public Mode xMode;
            public Mode yMode;

            public float xModifier, yModifier;
        }

        private ReboundData leftRightData;
        private ReboundData topData;

        private bool refillDash;

        private readonly string flag;

        public ReboundModifyController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            leftRightData = new ReboundData
            {
                xMode = (ReboundData.Mode)data.Int("leftRightXMode"),
                yMode = (ReboundData.Mode)data.Int("leftRightYMode"),
                xModifier = data.Float("leftRightXModifier"),
                yModifier = data.Float("leftRightYModifier"),
            };
            topData = new ReboundData
            {
                xMode = (ReboundData.Mode)data.Int("topXMode"),
                yMode = (ReboundData.Mode)data.Int("topYMode"),
                xModifier = data.Float("topXModifier"),
                yModifier = data.Float("topYModifier"),
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

            // logic here

            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
            self.wallSlideTimer = 1.2f;
            self.wallBoostTimer = 0f;
            self.launched = false;
            self.lowFrictionStopTimer = 0.15f;
            self.forceMoveXTimer = 0f;
            self.StateMachine.State = 0;
        }
    }
}