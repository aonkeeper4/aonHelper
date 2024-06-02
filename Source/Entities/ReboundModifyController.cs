using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.aonHelper.Entities
{

    [CustomEntity("aonHelper/ReboundModifyController")]
    [Tracked]
    public class ReboundModifyController : Entity
    {
        private bool reflectSpeed;

        private float reflectSpeedMultiplier;

        private bool refillDash;

        private readonly string flag;

        public ReboundModifyController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            reflectSpeed = data.Bool("reflectSpeed");
            reflectSpeedMultiplier = !reflectSpeed ? 0f : data.Float("reflectSpeedMultiplier", 0.5f);
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

            DynamicData selfData = DynamicData.For(self);
            if (controller.reflectSpeed)
            {
                self.Speed.X *= (direction != 0) ? -controller.reflectSpeedMultiplier : 0f;
            }
            if (controller.refillDash)
            {
                self.RefillDash();
            }
            selfData.Set("dashAttackTimer", 0f);
            selfData.Set("gliderBoostTimer", 0f);
            selfData.Set("wallSlideTimer", 1.2f);
            selfData.Set("wallBoostTimer", 0f);
            selfData.Set("launched", false);
            selfData.Set("lowFrictionStopTimer", 0.15f);
            selfData.Set("forceMoveXTimer", 0f);
            self.StateMachine.State = 0;
        }
    }
}