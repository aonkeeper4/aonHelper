namespace Celeste.Mod.aonHelper;

public class aonHelperSession : EverestModuleSession
{
    public class GlassLockBlockState
    {
        public Color BgColor { get; set; }
        public Color LineColor { get; set; }
        public Color RayColor { get; set; }
        public Color[] StarColors { get; set; }
        public bool Wavy { get; set; }
        public bool VanillaEdgeBehavior { get; set; }
    }
    public GlassLockBlockState GlassLockBlockCurrentSettings { get; set; }

    public Dictionary<EntityID, bool> DreamBlockDummyStates { get; set; } = new();
    public HashSet<EntityID> UnlockedDreamLockBlocks { get; set; } = [];
}
