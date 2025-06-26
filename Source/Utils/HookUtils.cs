using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Utils;

public class HookUtils
{
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
    
    /// <summary>
    ///   Contains commonly used <see cref="BindingFlags"/>.
    /// </summary>
    public static class Bind
    {
        /// <summary>
        ///   Shorthand for <see cref="BindingFlags.Public"/> and <see cref="BindingFlags.Static"/>.
        /// </summary>
        public static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        ///   Shorthand for <see cref="BindingFlags.NonPublic"/> and <see cref="BindingFlags.Static"/>.
        /// </summary>
        public static readonly BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        ///   Shorthand for <see cref="BindingFlags.Public"/> and <see cref="BindingFlags.Instance"/>.
        /// </summary>
        public static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        ///   Shorthand for <see cref="BindingFlags.NonPublic"/> and <see cref="BindingFlags.Instance"/>.
        /// </summary>
        public static readonly BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    }
    
    public class HookException(string message, Exception inner = null) : Exception($"Hook application failed: {message}", inner)
    {
        public HookException(ILContext il, string message, Exception inner = null) : this($"ILHook application on method {il.Method.FullName} failed: {message}", inner) { }
    }
}
