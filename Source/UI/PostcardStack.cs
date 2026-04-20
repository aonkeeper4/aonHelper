namespace Celeste.Mod.aonHelper.UI;

public class PostcardStack : Postcard
{
    private new class Postcard
    {
        
    }
    
    public PostcardStack(aonHelperMetadata.PostcardInfo[] postcards, string sfxEventIn, string sfxEventOut)
        : base(null, sfxEventIn, sfxEventOut)
    {
        
    }
    
    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        
    }

    [OnUnload]
    internal static void Unload()
    {
        
    }
    
    #endregion
}
