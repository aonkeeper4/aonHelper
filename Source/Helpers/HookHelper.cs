using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Helpers;

public class HookHelper
{
    public const string DetourConfigID = "aonHelper";
    public const string StyleMaskHelperDetourConfigID = "StyleMaskHelper";

    public static readonly DetourConfig BeforeStyleMaskHelper
        = new DetourConfig(DetourConfigID)
            .WithBefore(StyleMaskHelperDetourConfigID)
            .WithPriority(0);
    
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

    // see CommunalHelper DreamTunnelDash source for a properly explained version of this
    public static void ModifyStateCheck(ILCursor cursor, int originalCheckedState, bool equal, bool canShortCircuit, int newCheckedState, Func<Player, bool> extraCheck = null)
    {
        if (!cursor.TryGotoNextFirstFitReversed(MoveType.AfterLabel, 0x10,
            instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(originalCheckedState)))
            return;

        ILLabel failedCheck = null;

        // todo: add support for `ceq`-based checks
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

    public static class HookLazyLoadingManager
    {
        private const string LogID = $"{nameof(aonHelper)}/{nameof(HookLazyLoadingManager)}";

        public delegate bool ShouldLazyLoadHandler(MapData mapData);
        public delegate void LazyLoadHandler();
        public delegate void LazyUnloadHandler();
        
        private class HookState(ShouldLazyLoadHandler shouldLazyLoad, LazyLoadHandler lazyLoad, LazyUnloadHandler lazyUnload)
        {
            public bool Loaded;

            public readonly ShouldLazyLoadHandler ShouldLazyLoad = shouldLazyLoad;
            public readonly LazyLoadHandler LazyLoad = lazyLoad;
            public readonly LazyUnloadHandler LazyUnload = lazyUnload;
        }
        private static readonly Dictionary<string, HookState> Hooks = new();
        
        public static void Register(string tag, ShouldLazyLoadHandler shouldLazyLoad, LazyLoadHandler load, LazyUnloadHandler unload)
            => Hooks.TryAdd(tag, new HookState(shouldLazyLoad, load, unload));
        
        private static void UpdateHooks(Session session)
        {
            foreach ((string tag, HookState state) in Hooks)
            {
                bool shouldLoad = session?.MapData is { } mapData && state.ShouldLazyLoad(mapData);
                if (shouldLoad == state.Loaded)
                    continue;

                if (shouldLoad)
                {
                    state.LazyLoad();
                    state.Loaded = true;
                    Logger.Info(LogID, $"Lazily loaded hooks for {tag}.");
                }
                else
                {
                    state.LazyUnload();
                    state.Loaded = false;
                    Logger.Info(LogID, $"Lazily unloaded hooks for {tag}.");
                }
            }
        }

        #region Hooks
    
        // we need to make sure these are loaded *before* any other hooks are loaded and *after* all other hooks are unloaded, so we don't use the mod lifecycle attributes
        internal static void Load()
        {
            Hooks.Clear();
            
            // i'm not entirely sure whether we should be using the everest events? but gravityhelper doesn't so
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor += OverworldLoader_ctor;
        }

        internal static void Unload()
        {
            UpdateHooks(null);
            
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor -= OverworldLoader_ctor;
        }
        
        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startposition)
        {
            orig(self, session, startposition);

            UpdateHooks(session);
        }
        
        private static void OverworldLoader_ctor(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startmode, HiresSnow snow)
        {
            orig(self, startmode, snow);

            // i assume this is for collabutils2 support
            if (startmode is (Overworld.StartMode) (-1))
                return;
            
            UpdateHooks(null);
        }
        
        #endregion
    }
}
