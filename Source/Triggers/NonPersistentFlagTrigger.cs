namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/NonPersistentFlagTrigger")]
public class NonPersistentFlagTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    [Tracked]
    private class NonPersistentFlagManager : Entity
    {
        private readonly HashSet<string> flags = [];

        public NonPersistentFlagManager()
        {
            Tag = Tags.Global;
        }

        public void SetFlag(string flag, bool value = true)
        {
            if (flag is null)
                return;

            if (value)
                flags.Add(flag);
            else
                flags.Remove(flag);
        }

        public bool GetFlag(string flag)
            => flag is not null && flags.Contains(flag);
    }

    private readonly string flag = data.String("flag");

    private readonly bool value = data.Bool("value", true);
    private readonly bool oneUse = data.Bool("oneUse");

    public override void OnEnter(Player player)
    {
        if (Scene.Tracker.GetEntity<NonPersistentFlagManager>() is not { } manager || flag is null)
            return;

        manager.SetFlag(flag, value);
        if (oneUse)
            RemoveSelf();
    }

    #region Hooks

    internal static void Load()
    {
        Everest.Events.LevelLoader.OnLoadingThread += Event_LevelLoader_OnLoadingThread;

        On.Celeste.Session.GetFlag += On_Session_GetFlag;
    }

    internal static void Unload()
    {
        Everest.Events.LevelLoader.OnLoadingThread -= Event_LevelLoader_OnLoadingThread;

        On.Celeste.Session.GetFlag -= On_Session_GetFlag;
    }

    private static void Event_LevelLoader_OnLoadingThread(Level level)
        => level.Add(new NonPersistentFlagManager());

    private static bool On_Session_GetFlag(On.Celeste.Session.orig_GetFlag orig, Session self, string flag)
        => orig(self, flag) || ((Engine.Scene as Level)?.Tracker.GetEntity<NonPersistentFlagManager>()?.GetFlag(flag) ?? false);

    #endregion
}
