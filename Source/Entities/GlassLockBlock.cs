using Celeste.Mod.aonHelper.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[Tracked]
[CustomEntity("aonHelper/GlassLockBlock", "MoreLockBlocks/GlassLockBlock")]
public class GlassLockBlock : BaseLockBlock
{
    private const string SpriteID = "aonHelper_lockBlockLock";

    private List<Rectangle?> frameMetadata;
    
    public Rectangle? RenderBounds
    {
        get
        {
            if (Sprite.Texture is null)
                return new Rectangle(-16, -16, 32, 32);
                
            if (TryGetFrameIndexFromPath(Sprite.Texture.AtlasPath, frameMetadata.Count, out int index))
                return frameMetadata[index];
                    
            throw new KeyNotFoundException("Could not find metadata associated with current frame index, does your sprite match vanilla's format?");
            
            static bool TryGetFrameIndexFromPath(string path, int max, out int index)
            {
                int lastDigitFromEndIndex = path.Length - 1 - path.Reverse().LastIndexWhere(char.IsDigit);
                bool result = int.TryParse(path.AsSpan(lastDigitFromEndIndex), out int i) && i < max;
            
                index = result ? i : 0;
                return result;
            }
        }
    }

    public GlassLockBlock(
        EntityID id, Vector2 position,
        string spritePath,
        string unlockSfx, bool stepMusicProgress,
        OpeningSettingsData openingSettings,
        bool behindFgTiles)
        : base(id, position, spritePath, unlockSfx, stepMusicProgress, openingSettings)
    {
        Depth = behindFgTiles ? -9995 : -10000;
        SurfaceSoundIndex = 32;
        
        Add(new LightOcclude());
        Add(new MirrorSurface());

        BuildFrameMetadata();
    }

    public GlassLockBlock(EntityData data, Vector2 offset, EntityID id)
        : this(id, data.Position + offset,
            data.Attr("spritePath"),
            data.Attr("unlockSfx"), data.Bool("stepMusicProgress"),
            ParseOpeningSettings(data.Bool("useVanillaKeys", true), data.Attr("dzhakeHelperKeySettings")),
            data.Bool("behindFgTiles"))
    { }

    private void BuildFrameMetadata()
    {
        if (aonHelperGFX.SpriteBank
                        .SpriteData[SpriteID]
                        .Sources
                        .Select(s => s.XML["Metadata"])
                        .FirstOrDefault(s => s is not null)
            is not { } item)
            return;
        
        frameMetadata = item.Attr("bounds")?
                            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select<string, Rectangle?>(t =>
                            {
                                if (t.Equals("x", StringComparison.OrdinalIgnoreCase))
                                    return null;
                                
                                int[] args = t.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
                                return new Rectangle(args[0], args[1], args[2], args[3]);
                            })
                            .ToList()
            ?? throw new KeyNotFoundException($"Could not find bounds metadata for sprite with ID {SpriteID}!");
    }

    public override void Render()
    {
        if (RenderBounds is { } rb && Scene.Tracker.GetEntity<GlassLockBlockController>() is { } controller)
        {
            Rectangle outline = new((int) (Center.X + rb.Left), (int) (Center.Y + rb.Top), rb.Width, rb.Height);
            Color lineColor = controller.LineColor;

            if (controller.VanillaEdgeBehavior)
            {
                Draw.Line(outline.TopLeft() - Vector2.UnitY, outline.TopRight() - Vector2.UnitY, lineColor);
                Draw.Line(outline.TopRight() + Vector2.UnitX, outline.BottomRight() + Vector2.UnitX, lineColor);
                Draw.Line(outline.BottomRight() + Vector2.UnitY, outline.BottomLeft() + Vector2.UnitY, lineColor);
                Draw.Line(outline.BottomLeft() - Vector2.UnitX, outline.TopLeft() - Vector2.UnitX, lineColor);
            }
            else
                Draw.HollowRect(outline, lineColor);
        }

        base.Render();
    }
}