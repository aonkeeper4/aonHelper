using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.aonHelper.Entities
{
    [CustomEntity("aonHelper/FlingBirdNoSkipController")]
    [Tracked]
    public class FlingBirdNoSkipController : Entity
    {
        private readonly string flag;

        public FlingBirdNoSkipController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            flag = data.Attr("flag");
        }

        public static void Load()
        {
            IL.Celeste.FlingBird.Update += mod_FlingBirdUpdate;
        }

        public static void Unload()
        {
            IL.Celeste.FlingBird.Update -= mod_FlingBirdUpdate;
        }

        private static void mod_FlingBirdUpdate(ILContext il)
        {
            ILCursor cursor = new(il);
            cursor.GotoNext(MoveType.After, instr => instr.MatchSwitch(out _)); // inside the switch statement
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchCallOrCallvirt<FlingBird>("Skip")); // go to first place where Skip is called
            ILLabel ret = cursor.DefineLabel(); // define label to break to return early
            cursor.Emit(OpCodes.Ldarg_0); // get bird instance
            cursor.EmitDelegate(determineIfBirdSkipControllerActivated); // is there a controller?
            cursor.Emit(OpCodes.Brtrue, ret); // if controller, jump to ret so we don't call Skip
            cursor.GotoNext(instr => instr.MatchRet()); // go to the ret after call to Skip
            cursor.MarkLabel(ret); // mark the label
        }

        private static bool determineIfBirdSkipControllerActivated(FlingBird bird)
        {
            Level level = bird.SceneAs<Level>();
            if (level.Tracker.GetEntity<FlingBirdNoSkipController>() is not FlingBirdNoSkipController controller)
                return false;
            return level.Session.GetFlag(controller.flag) || controller.flag == "";
        }
    }
}