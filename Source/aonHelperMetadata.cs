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
        metadata = null;
        
        if (CachedMetadata.TryGetValue(areaKey.SID, out metadata)) return metadata is not null;

        string filename = AreaData.Get(areaKey).Mode[(int) areaKey.Mode].Path;
        if (!Everest.Content.TryGet<AssetTypeYaml>($"Maps/{filename}.meta", out ModAsset asset)) goto fail;

        if (asset is null) goto fail;
        if (!asset.PathVirtual.StartsWith("Maps")) goto fail;
        if (!asset.TryValidatingDeserialize(out aonHelperYaml meta)) goto fail;
        if (meta?.aonHelperMetadata is not { } deserialized) goto fail;
        
        metadata = CachedMetadata[areaKey.SID] = deserialized;
        return true;
        
    fail:
        CachedMetadata[areaKey.SID] = null;
        return false;
    }
    
    #region Hooks

    internal static void Load()
    {
        Everest.Content.OnUpdate += OnUpdate;
    }
    
    internal static void Unload()
    {
        Everest.Content.OnUpdate -= OnUpdate;
    }
    
    private static void OnUpdate(ModAsset old, ModAsset _)
    {
        // maybe a bit overkill
        if (old.Type == typeof(AssetTypeYaml)
            && old.PathVirtual.StartsWith("Maps")
            && old.PathVirtual.EndsWith(".meta"))
            CachedMetadata.Clear();
    }
    
    #endregion
}