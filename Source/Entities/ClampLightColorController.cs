using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/ClampLightColorController")]
[Tracked]
public class ClampLightColorController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    private readonly Color clampColor = data.HexColor("color", Color.White);

    private enum ClampMethod {
        Clamp,
        Tint,
    }
    private readonly ClampMethod clampMethod = data.Enum("clampMethod", ClampMethod.Clamp);

    // this might be wrongg but it looks fine to me
    private static readonly BlendState ClampColorState = new()
    {
        ColorBlendFunction = BlendFunction.Min,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
    };
    private static readonly BlendState TintColorState = new()
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
    };

    #region Hooks
    
    internal static void Load()
    {
        On.Celeste.LightingRenderer.BeforeRender += LightingRenderer_BeforeRender;
    }

    internal static void Unload()
    {
        On.Celeste.LightingRenderer.BeforeRender -= LightingRenderer_BeforeRender;
    }

    private static void LightingRenderer_BeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene)
    {
        orig(self, scene);

        if (scene.Tracker.GetEntity<ClampLightColorController>() is not { } controller)
            return;
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, controller.clampMethod switch {
            ClampMethod.Clamp => ClampColorState,
            ClampMethod.Tint => TintColorState,
            _ => throw new ArgumentOutOfRangeException()
        });
        Draw.Rect(new Rectangle(0, 0, GameplayBuffers.Light.Width, GameplayBuffers.Light.Height), controller.clampColor);
        Draw.SpriteBatch.End();
    }
    
    #endregion
}