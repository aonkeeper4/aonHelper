namespace Celeste.Mod.aonHelper;

public class aonHelperMetadata
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperMetadata)}";
    
    private static readonly Dictionary<string, aonHelperMetadata> CachedMetadata = new();
    
    #region Metadata Properties
    
    private class aonHelperYaml
    {
        public aonHelperMetadata aonHelperMetadata { get; set; } = new();
    }
    
    #endregion

    public static bool TryGetMetadata(AreaKey areaKey, out aonHelperMetadata metadata)
    {
        metadata = null;
        
        if (CachedMetadata.TryGetValue(areaKey.SID, out metadata))
            return metadata is not null;

        string filename = AreaData.Get(areaKey).Mode[(int) areaKey.Mode].Path;
        if (Everest.Content.TryGet<AssetTypeYaml>($"Maps/{filename}.meta", out ModAsset asset)
            && asset is not null
            && asset.PathVirtual.StartsWith("Maps")
            && asset.TryValidatingDeserialize(out aonHelperYaml meta)
            && meta?.aonHelperMetadata is { } deserialized)
        {
            Logger.Info(LogID, $"Cached aon helper metadata for '{areaKey.SID}' from 'Maps/{filename}.meta.yaml'.");
            metadata = CachedMetadata[areaKey.SID] = deserialized;
            return true;
        }
        
        Logger.Info(LogID, $"No aon helper metadata found for '{areaKey.SID}' in 'Maps/{filename}.meta.yaml'.");
        CachedMetadata[areaKey.SID] = null;
        return false;
    }
    
    #region Hooks

    internal static void Load()
    {
        Everest.Content.OnUpdate += OnUpdateContent;
    }
    
    internal static void Unload()
    {
        Everest.Content.OnUpdate -= OnUpdateContent;
    }
    
    private static void OnUpdateContent(ModAsset old, ModAsset _)
    {
        // maybe a bit overkill
        if (old is not null
            && old.Type == typeof(AssetTypeYaml)
            && old.PathVirtual.StartsWith("Maps")
            && old.PathVirtual.EndsWith(".meta"))
            CachedMetadata.Clear();
    }
    
    #endregion
}