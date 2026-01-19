using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Celeste.Mod.aonHelper;

public static class aonHelperImports
{
    // do all modinterop stuff here
    // if we need to do anything more than modinterop, that goes in `aonHelperDependencies`
    
    internal static void Initialize()
    {
        typeof(ReverseHelper).ModInterop();
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    
    [ModImportName("ReverseHelper.DreamBlock")]
    public static class ReverseHelper
    {
        public static Action<Type, Action<Entity>, Action<Entity>> RegisterDreamBlockLike;
        public static Func<Entity, bool> PlayerHasDreamDash;
        public static Func<Entity, long, bool?> ConfigureGetFromEnum;
        public static Action<Entity, long, bool?> ConfigureSetFromEnum;
        public static Func<string, long> ConfigureGetEnum;
        public static Func<Scene, ImmutableArray<List<Entity>>> GetDreamBlockTrackers;
    }
    
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
