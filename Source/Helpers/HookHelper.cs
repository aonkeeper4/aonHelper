namespace Celeste.Mod.aonHelper.Helpers;

public static class HookHelper
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
    public delegate bool NewStateCheck(Player player);
    public static void ModifyStateCheck(ILCursor cursor, int originalCheckedState, bool equal, bool canShortCircuit, NewStateCheck newStateCheck, bool moveBackwards = false)
    {
        if (newStateCheck.GetInvocationList().Length != 1 || newStateCheck.Target is not null)
            throw new ArgumentException("New state check must be of the form `static bool NewStateCheck(Player player)`.", nameof(newStateCheck));

        Func<Instruction, bool>[] predicates =
            [instr => instr.MatchLdfld<Player>("StateMachine"),
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(originalCheckedState)];
        if (!(moveBackwards 
            ? cursor.TryGotoPrevFirstFitReversed(MoveType.AfterLabel, 0x10, predicates)
            : cursor.TryGotoNextFirstFitReversed(MoveType.AfterLabel, 0x10, predicates)))
            throw new HookException(cursor.Context, $"Unable to find state check for player state {originalCheckedState} to modify.");

        ILLabel failedCheck = null;

        // todo: add support for `ceq`-based checks
        ILCursor cloned = cursor.Clone();
        if (!cloned.TryGotoNext(MoveType.After, instr => equal ^ canShortCircuit ? instr.MatchBneUn(out failedCheck) : instr.MatchBeq(out failedCheck)))
            throw new HookException(cursor.Context, $"Unable to find check failure label in check for player state {originalCheckedState}.");
        Instruction afterMatch = cloned.Next!;

        ILLabel cleanUpPlayer = cursor.DefineLabel(), pastCleanUpPlayer = cursor.DefineLabel();
        
        cursor.EmitDup();
        cursor.EmitCall(newStateCheck.Method);
        cursor.EmitBrtrue(cleanUpPlayer);

        cursor.Goto(equal ^ canShortCircuit ? afterMatch : failedCheck.Target);
        cursor.EmitBr(pastCleanUpPlayer);
        cursor.EmitPop();
        cursor.MarkLabel(pastCleanUpPlayer);
        cursor.Index--;
        cursor.MarkLabel(cleanUpPlayer);
        
        cursor.Goto(afterMatch, MoveType.After);
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

    // heavily referenced from gravityhelper
    public static class HookLazyLoadingManager
    {
        private const string LogID = $"{nameof(aonHelper)}/{nameof(HookLazyLoadingManager)}";

        public delegate bool ShouldLazyLoadHandler(MapData mapData);
        public delegate void LazyLoadHandler();
        public delegate void LazyUnloadHandler();
        
        private class HookHandler(ShouldLazyLoadHandler shouldLazyLoad, LazyLoadHandler lazyLoad, LazyUnloadHandler lazyUnload)
        {
            public bool Loaded;

            public readonly ShouldLazyLoadHandler ShouldLazyLoad = shouldLazyLoad;
            public readonly LazyLoadHandler LazyLoad = lazyLoad;
            public readonly LazyUnloadHandler LazyUnload = lazyUnload;
        }
        private static readonly Dictionary<string, HookHandler> Hooks = new();
        
        public static void Register(string tag, ShouldLazyLoadHandler shouldLazyLoad, LazyLoadHandler load, LazyUnloadHandler unload)
            => Hooks.Add(tag, new HookHandler(shouldLazyLoad, load, unload));
        
        private static void UpdateHooks(Session session)
        {
            foreach ((string tag, HookHandler handler) in Hooks)
            {
                bool shouldLoad = session?.MapData is { } mapData && handler.ShouldLazyLoad(mapData);
                if (shouldLoad == handler.Loaded)
                    continue;

                if (shouldLoad)
                {
                    handler.LazyLoad();
                    handler.Loaded = true;
                    Logger.Info(LogID, $"Lazily loaded hooks for {tag}.");
                }
                else
                {
                    handler.LazyUnload();
                    handler.Loaded = false;
                    Logger.Info(LogID, $"Lazily unloaded hooks for {tag}.");
                }
            }
        }

        #region Hooks
    
        // we need to make sure these are loaded *before* any other hooks are loaded and *after* all other hooks are unloaded, so we don't use the mod lifecycle attributes
        internal static void Load()
        {
            Hooks.Clear();
            
            // i'm not sure whether we could be using everest events but applying hooks on a different thread (in `LevelLoader`'s case) doesn't sound like a good idea
            On.Celeste.LevelLoader.ctor += On_LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor += On_OverworldLoader_ctor;
        }

        internal static void Unload()
        {
            UpdateHooks(null);
            
            On.Celeste.LevelLoader.ctor -= On_LevelLoader_ctor;
            On.Celeste.OverworldLoader.ctor -= On_OverworldLoader_ctor;
        }
        
        private static void On_LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startposition)
        {
            orig(self, session, startposition);

            UpdateHooks(session);
        }
        
        private static void On_OverworldLoader_ctor(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startmode, HiresSnow snow)
        {
            orig(self, startmode, snow);

            // i assume this is for collabutils2 support?
            if (startmode is (Overworld.StartMode) (-1))
                return;
            
            UpdateHooks(null);
        }
        
        #endregion
    }
}
