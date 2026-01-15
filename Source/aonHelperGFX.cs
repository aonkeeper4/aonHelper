using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.aonHelper;

public static class aonHelperGFX
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperGFX)}";
    
    public static int GameplayBufferWidth => GameplayBuffers.Gameplay?.Width ?? 320;
    public static int GameplayBufferHeight => GameplayBuffers.Gameplay?.Height ?? 180;
    
    public static SpriteBank SpriteBank { get; private set; }
    
    private static List<Effect> effects;
    public static Effect FxQuantizedColorGrade { get; private set; }

    internal static void LoadContent()
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/aonHelper/Sprites.xml");

        effects = [];
        FxQuantizedColorGrade = LoadEffect("quantized_colorgrade");
    }

    internal static void UnloadContent()
    {
        effects.ForEach(e => e?.Dispose());
        effects = null;
    }
    
    private static Effect LoadEffect(string id)
    {
        string path = $"Effects/aonHelper/{id}.cso";
        Logger.Info(LogID, $"Loading effect from {path}...");

        if (!Everest.Content.TryGet(path, out ModAsset effect))
            Logger.Error(LogID, $"Failed to find effect at {path}!");

        effects.Add(new Effect(Engine.Graphics.GraphicsDevice, effect.Data));
        return effects.Last();
    }
    
#if DEBUG

    [Command("aonHelper_reloadGFX", "Reload GFX resources for aonHelper")]
    public static void ReloadContentCommand()
    {
        Logger.Info(LogID, "Reloading GFX...");

        UnloadContent();
        LoadContent();
    }

#endif
    
}
