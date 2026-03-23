using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
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

        // local to store whether we should apply liftspeed to this actor
        VariableDefinition shouldApplyLiftSpeed = new(il.Import(typeof(bool)));
        il.Body.Variables.Add(shouldApplyLiftSpeed);
        
        // label to mark the end of liftspeed application so we can modify the labels in the method to do what we need
        // annoyingly there are 2 different labels that point to the same place, and we want to insert instructions after one but not the other
        ILLabel postApplyLiftSpeed = cursor.DefineLabel();
        
        /*
         * IL_002d: ldloc.1
         * IL_002e: ldarg.0
         * IL_002f: callvirt instance bool Celeste.Actor::IsRiding(class Celeste.JumpThru)
         * IL_0034: brfalse.s IL_005c
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloc1(),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Actor>("IsRiding"),
            instr => instr.MatchBrfalse(out _)))
            throw new HookHelper.HookException(il, "Unable to find beginning of `entity.IsRiding` block to modify.");
        
        // initialise local to store whether we should apply liftspeed or not, to ensure there are never any cases where the liftspeed application is half-finished
        cursor.EmitLdarg0();
        cursor.EmitDelegate(ShouldApplyLiftSpeed);
        cursor.EmitStloc(shouldApplyLiftSpeed);
        
        // set `Collidable` to `false`
        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitLdloc(shouldApplyLiftSpeed);
        cursor.EmitDelegate(PreApplyLiftSpeed);
        
        /*
         * IL_005c: ldloca.s 0
         * IL_005e: call instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Monocle.Entity>::MoveNext()
         * IL_0063: brtrue.s IL_0020
         */
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdloca(0),
            instr => instr.MatchCall<List<Entity>.Enumerator>("MoveNext"),
            instr => instr.MatchBrtrue(out _)))
            throw new HookHelper.HookException(il, "Unable to find end of `entity.IsRiding` block to modify.");

        // actually apply the liftspeed and set `Collidable` back to `true`
        cursor.MarkLabel(postApplyLiftSpeed);
        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitLdloc(shouldApplyLiftSpeed);
        cursor.EmitDelegate(PostApplyLiftSpeed);
        
        /*
         * IL_0050: br.s IL_005c
         * IL_0052: ldloc.1
         * IL_0053: ldarg.1
         * IL_0054: ldnull
         * IL_0055: ldnull
         * IL_0056: callvirt instance bool Celeste.Actor::MoveHExact(int32, class Celeste.Collision, class Celeste.Solid)
         * IL_005b: pop
         */
        if (!cursor.TryGotoPrevBestFit(MoveType.Before,
            instr => instr.MatchBr(out _),
            instr => instr.MatchLdloc1(),
            instr => instr.MatchLdarg1(),
            instr => instr.MatchLdnull(),
            instr => instr.MatchLdnull(),
            instr => instr.MatchCallvirt<Actor>("MoveHExact"),
            instr => instr.MatchPop()))
            throw new HookHelper.HookException(il, "Unable to find `entity.TreatNaive` if-else to modify.");
        
        // make the `br.s` point to our `PostApplyLiftSpeed` call. this is technically destructive but  ehh if it becomes a problem i'll fix it
        cursor.Next!.OpCode = OpCodes.Br; // just in case a `br.s` can't be used somehow
        cursor.Next!.Operand = postApplyLiftSpeed.Target!;

        return;

        static bool ShouldApplyLiftSpeed(JumpThru jumpThru) {
            Level level = jumpThru.SceneAs<Level>();
            return level.Tracker.GetEntity<JumpThrusApplyLiftSpeedController>() is { } controller
                && (level.Session.GetFlag(controller.flag) || controller.flag is null);
        }

        static void PreApplyLiftSpeed(JumpThru jumpThru, Actor actor, bool shouldApplyLiftSpeed)
        {
            if (!shouldApplyLiftSpeed)
                return;

            jumpThru.Collidable = false;
        }
        
        static void PostApplyLiftSpeed(JumpThru jumpThru, Actor actor, bool shouldApplyLiftSpeed)
        {
            if (!shouldApplyLiftSpeed)
                return;
            
            actor.LiftSpeed = jumpThru.LiftSpeed;
            jumpThru.Collidable = true;
        }
    }

    #endregion
}
