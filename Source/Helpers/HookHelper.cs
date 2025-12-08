using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Helpers;

public class HookHelper
{
    public const string DetourConfigName = "aonHelper";
    public const string StyleMaskHelperDetourConfigName = "StyleMaskHelper";

    public static readonly DetourConfig BeforeStyleMaskHelper = CreateDetourConfig(before: [StyleMaskHelperDetourConfigName]);
    
    public static void DisposeAndSetNull(ref Hook hook)
    {
        hook.Dispose();
        hook = null;
    }

    public static void DisposeAndSetNull(ref ILHook ilHook)
    {
        ilHook.Dispose();
        ilHook = null;
    }

    public static DetourConfig CreateDetourConfig(List<string> before = null, List<string> after = null)
    {
        bool beforeAll = HasWildcard(before);
        bool afterAll = HasWildcard(after);
        int priority = beforeAll
            ? int.MaxValue
            : afterAll
                ? int.MinValue
                : 0;

        return new DetourConfig(DetourConfigName, priority, before, after);

        static bool HasWildcard(List<string> list)
            => (list?.RemoveAll(s => s.Equals("*")) ?? 0) != 0;
    }
    
    // see CommunalHelper DreamTunnelDash source for a properly explained version of this
    public static void ModifyStateCheck(ILCursor cursor, int originalCheckedState, bool equal, bool canShortCircuit, int newCheckedState, Func<Player, bool> extraCheck = null)
    {
        if (!cursor.TryGotoNextFirstFitReversed(MoveType.AfterLabel, 0x10,
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(originalCheckedState)))
            return;

        ILLabel failedCheck = null;

        // todo: add support for ceq-based checks
        ILCursor cloned = cursor.Clone();
        if (!cloned.TryGotoNext(MoveType.After, instr => equal ^ canShortCircuit ? instr.MatchBneUn(out failedCheck) : instr.MatchBeq(out failedCheck)))
            return;
        Instruction afterMatch = cloned.Next!;

        ILLabel cleanUpPlayer = cursor.DefineLabel(), pastCleanUpPlayer = cursor.DefineLabel();

        cursor.EmitDup();
        cursor.EmitLdcI4(newCheckedState);
        cursor.EmitNewReference(extraCheck, out _); // we coulddd cache this?
        cursor.EmitDelegate(StateCheck);
        cursor.EmitBrtrue(cleanUpPlayer);

        cursor.Goto(equal ^ canShortCircuit ? afterMatch : failedCheck.Target);
        cursor.EmitBr(pastCleanUpPlayer);
        cursor.EmitPop();
        cursor.MarkLabel(pastCleanUpPlayer);
        cursor.Index--;
        cursor.MarkLabel(cleanUpPlayer);
        
        cursor.Goto(afterMatch, MoveType.After);
        return;
        
        static bool StateCheck(Player player, int newCheckedState, Func<Player, bool> extraCheck)
            => player.StateMachine.State == newCheckedState && (extraCheck?.Invoke(player) ?? true);
    }
    
    public static class Bind
    {
        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    }
    
    public class HookException(string message, Exception inner = null)
        : Exception($"Hook application failed: {message}", inner)
    {
        public HookException(ILContext il, string message, Exception inner = null)
            : this($"ILHook application on method {il.Method.FullName} failed: {message}", inner)
        { }
    }
}
