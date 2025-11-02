using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Helpers;

public class HookHelper
{
    private const string DetourConfigName = "aonHelper";
    
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

    public static DetourConfig GetDetourConfig(List<string> before = null, List<string> after = null)
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
