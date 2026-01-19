using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.IO;
using System.Xml;

namespace Celeste.Mod.aonHelper.Helpers;

public static class EffectHelper
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(EffectHelper)}";
    
    public static Effect LoadEffect(string id)
    {
        string path = $"aonHelper:Effects/aonHelper/{id}.cso";

        if (Everest.Content.TryGet(path, out ModAsset effect))
            return new Effect(Engine.Graphics.GraphicsDevice, effect.Data);
        
        Logger.Error(LogID, $"Failed to find effect at {path}!");
        return null;
    }

    public static void DisposeAndSetNull(ref Effect effect)
    {
        effect?.Dispose();
        effect = null;
    }

    public static Atlas LoadAtlasFromMod(string path)
    {
        Atlas atlas = new() { Sources = [] };

        if (Everest.Content.TryGet<AssetTypeXml>(path, out ModAsset asset))
        {
            string directory = Path.GetDirectoryName(path);

            XmlDocument xml = new();
            xml.Load(asset.Stream);
            atlas.LoadXmlData(xml, directory);
            
            return atlas;
        }
        
        Logger.Error(LogID, $"Failed to find atlas data file at {path}!");
        return null;
    }

    private static void LoadXmlData(this Atlas atlas, XmlDocument xml, string directory)
    {
        foreach (XmlElement tex in xml["atlas"])
        {
            string sourcePath = Path.Combine(directory, tex.GetAttribute("n")).Replace('\\', '/');

            if (!Everest.Content.TryGet(sourcePath, out ModAsset asset))
            {
                Logger.Error(LogID, $"Failed to find atlas source image at {sourcePath}, skipping!");
                continue;
            }
            
            VirtualTexture virtualTexture = VirtualContent.CreateTexture(asset);
            MTexture source = new(virtualTexture) { Atlas = atlas };
            atlas.Sources.Add(virtualTexture);

            foreach (XmlElement img in tex)
            {
                string name = img.Attr("n");
                
                int x = img.AttrInt("x");
                int y = img.AttrInt("y");
                int w = img.AttrInt("w");
                int h = img.AttrInt("h");
                Rectangle rect = new(x, y, w, h);

                if (img.HasAttr("fx"))
                {
                    int fx = img.AttrInt("fx");
                    int fy = img.AttrInt("fy");
                    int fw = img.AttrInt("fw");
                    int fh = img.AttrInt("fh");
                    atlas.Textures[name] = new MTexture(source, name, rect, new Vector2(-fx, -fy), fw, fh);
                }
                else
                    atlas.Textures[name] = new MTexture(source, name, rect);
            }
        }
    }
}
