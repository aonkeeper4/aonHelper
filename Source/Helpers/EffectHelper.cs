using System.IO;
using System.Xml;

namespace Celeste.Mod.aonHelper.Helpers;

public static class EffectHelper
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(EffectHelper)}";
    
    public static Effect LoadEffect(string id)
    {
        string path = $"aonHelper:/Effects/aonHelper/{id}.cso";

        if (Everest.Content.TryGet(path, out ModAsset effect))
        {
            Logger.Info(LogID, $"Loaded effect from {path}.");
            return new Effect(Engine.Graphics.GraphicsDevice, effect.Data);
        }

        Logger.Error(LogID, $"Failed to find effect at {path}!");
        return null;
    }

    public static void DisposeAndSetNull(ref Effect effect)
    {
        effect?.Dispose();
        effect = null;
    }

    public static Atlas LoadAtlas(string path)
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

    public class ResetGraphicsDeviceOnDispose : IDisposable
    {
        public class SlotRegistrar(ResetGraphicsDeviceOnDispose disposable, int start)
        {
            private readonly HashSet<int> registered = [];
            public int[] Registered => registered.ToArray();

            public int Current { get; private set; } = start;

            public int Add(int slot)
            {
                if (!registered.Add(Current = slot))
                    throw new InvalidOperationException("Cannot register the same slot multiple times!");

                disposable.previousTextures.Add(disposable.graphicsDevice.Textures[Current]);
                disposable.previousSamplerStates.Add(disposable.graphicsDevice.SamplerStates[Current]);

                return Current;
            }

            public int Next()
                => Add(Current + 1);
        }

        private readonly GraphicsDevice graphicsDevice;
        private readonly SlotRegistrar slotRegistrar;

        private List<Texture> previousTextures = [];
        private List<SamplerState> previousSamplerStates = [];

        private BlendState previousBlendState;
        private DepthStencilState previousDepthStencilState;
        private RasterizerState previousRasterizerState;

        public ResetGraphicsDeviceOnDispose(GraphicsDevice graphicsDevice, out SlotRegistrar slotRegistrar,
            int startingSlot = 0)
        {
            this.graphicsDevice = graphicsDevice;
            this.slotRegistrar = slotRegistrar = new SlotRegistrar(this, startingSlot);

            previousBlendState = this.graphicsDevice.BlendState;
            previousDepthStencilState = this.graphicsDevice.DepthStencilState;
            previousRasterizerState = this.graphicsDevice.RasterizerState;
        }

        public void Dispose()
        {
            // no idea if this is in any way correct
            ObjectDisposedException.ThrowIf(previousTextures is null, this);
            ObjectDisposedException.ThrowIf(previousSamplerStates is null, this);
            ObjectDisposedException.ThrowIf(previousBlendState is null, this);
            ObjectDisposedException.ThrowIf(previousDepthStencilState is null, this);
            ObjectDisposedException.ThrowIf(previousRasterizerState is null, this);

            foreach ((int i, Texture texture, SamplerState samplerState) in slotRegistrar.Registered.Zip(previousTextures, previousSamplerStates))
            {
                graphicsDevice.Textures[i] = texture;
                graphicsDevice.SamplerStates[i] = samplerState;
            }

            graphicsDevice.BlendState = previousBlendState;
            graphicsDevice.DepthStencilState = previousDepthStencilState;
            graphicsDevice.RasterizerState = previousRasterizerState;

            previousTextures = null;
            previousSamplerStates = null;
            previousBlendState = null;
            previousDepthStencilState = null;
            previousRasterizerState = null;

            GC.SuppressFinalize(this);
        }
    }
}
