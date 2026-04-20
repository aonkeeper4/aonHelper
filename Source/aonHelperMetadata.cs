namespace Celeste.Mod.aonHelper;

public class aonHelperMetadata
{
    private static readonly Dictionary<string, aonHelperMetadata> CachedMetadata = new();
    
    #region Metadata Properties
    
    private class aonHelperYaml
    {
        public aonHelperMetadata aonHelperMetadata { get; set; } = new();
    }

    public class PostcardInfo
    {
        public string DialogID { get; set; }
        
        public enum Sides
        {
            Front,
            Back
        }
        public Sides StartingSide { get; set; } = Sides.Front;
        public bool CanFlip { get; set; } = true;

        public string FrontTexture { get; set; } = "postcard";
        public string BackTexture { get; set; }
    }
    [Helpers.YamlHelper.NoNullItems]
    public PostcardInfo[] Postcards { get; set; } = [];
    
    #endregion

    public static bool TryGetMetadata(AreaKey areaKey, out aonHelperMetadata metadata)
    {
        if (CachedMetadata.TryGetValue(areaKey.SID, out metadata)) return true;

        string filename = AreaData.Get(areaKey).Mode[(int) areaKey.Mode].Path;
        if (!Everest.Content.TryGet<AssetTypeYaml>($"Maps/{filename}.meta", out ModAsset asset)) return false;

        if (asset is null) return false;
        if (!asset.PathVirtual.StartsWith("Maps")) return false;
        if (!asset.TryValidatingDeserialize(out aonHelperYaml meta)) return false;

        metadata = meta?.aonHelperMetadata;
        CachedMetadata[areaKey.SID] = metadata;
        return true;
    }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        CachedMetadata.Clear();
        
        Everest.Content.OnUpdate += OnUpdate;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        Everest.Content.OnUpdate -= OnUpdate;
    }
    
    private static void OnUpdate(ModAsset old, ModAsset _)
    {
        // maybe a bit overkill
        if (old.Type == typeof(AssetTypeYaml))
            CachedMetadata.Clear();
    }
    
    #endregion
}