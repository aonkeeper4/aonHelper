using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Celeste.Mod.aonHelper;

public static class aonHelperImports
{
    internal static void Initialize()
    {
        typeof(ReverseHelper).ModInterop();
    }
    
    public static class ReverseHelperCallHelper
    {
        public static void RegisterDreamBlockLike(Type targetType, Action<Entity> ActivateNoRoutine, Action<Entity> DeactivateNoRoutine)
            => ReverseHelper.RegisterDreamBlockLike?.Invoke(targetType, ActivateNoRoutine, DeactivateNoRoutine);

        public static bool PlayerHasDreamDash(Entity e, Func<bool> fallback = null)
        {
            if (ReverseHelper.PlayerHasDreamDash is null)
                return fallback?.Invoke() ?? (Engine.Scene as Level)?.Session?.Inventory.DreamDash ?? false;
            
            return ReverseHelper.PlayerHasDreamDash(e);
        }
        
        public static bool? ConfigureGetFromEnum(Entity e, long i)
            => ReverseHelper.ConfigureGetFromEnum?.Invoke(e, i);

        public static void ConfigureSetFromEnum(Entity e, long i, bool? value)
            => ReverseHelper.ConfigureSetFromEnum?.Invoke(e, i, value);
        
        public static long ConfigureGetEnum(string s)
            => ReverseHelper.ConfigureGetEnum?.Invoke(s) ?? 0;

        public static ImmutableArray<List<Entity>>? GetDreamBlockTrackers(Scene scene)
            => ReverseHelper.GetDreamBlockTrackers?.Invoke(scene);
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    
    [ModImportName("ReverseHelper.DreamBlock")]
    private static class ReverseHelper
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
