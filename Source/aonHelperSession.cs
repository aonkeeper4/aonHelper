namespace Celeste.Mod.aonHelper;

public class aonHelperSession : EverestModuleSession
{
    public Dictionary<EntityID, bool> DreamBlockDummyStates { get; set; } = new();
    public HashSet<EntityID> UnlockedDreamLockBlocks { get; set; } = [];
}
