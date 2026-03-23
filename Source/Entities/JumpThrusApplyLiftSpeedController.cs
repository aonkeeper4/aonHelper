using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/JumpThrusApplyLiftSpeedController")]
[Tracked]
public class JumpThrusApplyLiftSpeedController(Vector2 position, string flag) : Entity(position)
{
    private readonly string flag = string.IsNullOrEmpty(flag) ? null : flag;
    
    public JumpThrusApplyLiftSpeedController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("flag"))
    { }
    
    #region Hooks

    internal static void Load()
    {
        IL.Celeste.JumpThru.MoveHExact += JumpThru_MoveHExact;
    }

    internal static void Unload()
    {
        IL.Celeste.JumpThru.MoveHExact -= JumpThru_MoveHExact;
    }

    private static void JumpThru_MoveHExact(ILContext il)
    {
        ILCursor cursor = new(il);

        // due to where we hook, the `actor` local is uninitialised the first time we access it, which is technically UB
        // let's initialise it so we don't like idk corrupt memory or whatever
        cursor.EmitLdnull();
        cursor.EmitStloc1();
        
        /*
         * IL_0020: ldloca.s 0
         * IL_0022: call instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Monocle.Entity>::get_Current()
         * IL_0027: castclass Celeste.Actor
         * IL_002c: stloc.1
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloca(0),
            instr => instr.MatchCall<List<Entity>.Enumerator>("get_Current"),
            instr => instr.MatchCastclass<Actor>(),
            instr => instr.MatchStloc1()))
            throw new HookHelper.HookException(il, "Unable to find beginning of loop to modify.");
        
        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(PreApplyLiftSpeed);
        
        /*
         * IL_005c: ldloca.s 0
         * IL_005e: call instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Monocle.Entity>::MoveNext()
         * IL_0063: brtrue.s IL_0020
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdloca(0),
            instr => instr.MatchCall<List<Entity>.Enumerator>("MoveNext"),
            instr => instr.MatchBrtrue(out _)))
            throw new HookHelper.HookException(il, "Unable to find end of loop to modify.");

        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(PostApplyLiftSpeed);

        return;

        static bool ShouldApplyLiftSpeed(JumpThru jumpThru, Actor actor) {
            Level level = jumpThru.SceneAs<Level>();
            if (level.Tracker.GetEntity<JumpThrusApplyLiftSpeedController>() is not { } controller
                || !level.Session.GetFlag(controller.flag) && controller.flag is not null)
                return false;
            
            if (actor is null || !actor.IsRiding(jumpThru))
                return false;

            return true;
        }

        static void PreApplyLiftSpeed(JumpThru jumpThru, Actor actor)
        {
            if (!ShouldApplyLiftSpeed(jumpThru, actor))
                return;

            jumpThru.Collidable = false;
        }
        
        static void PostApplyLiftSpeed(JumpThru jumpThru, Actor actor)
        {
            if (!ShouldApplyLiftSpeed(jumpThru, actor))
                return;
            
            actor.LiftSpeed = jumpThru.LiftSpeed;
            jumpThru.Collidable = true;
        }
    }

    #endregion
}
