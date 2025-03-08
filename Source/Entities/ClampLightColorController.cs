using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/ClampLightColorController")]
[Tracked]
public class ClampLightColorController : Entity
{
    private Color clampColor;

    public enum ClampMethod {
        Clamp,
        Tint,
    }
    private readonly ClampMethod clampMethod;

    // this might be wrongg but it looks fine to me
    private static readonly BlendState clampColorState = new()
    {
        ColorBlendFunction = BlendFunction.Min,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
    };
    private static readonly BlendState tintColorState = new()
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
    };

    public ClampLightColorController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        clampColor = data.HexColor("color", Color.White);
        clampMethod = data.Enum("clampMethod", ClampMethod.Clamp);
    }

    public static void Load()
    {
        On.Celeste.LightingRenderer.BeforeRender += LightingRenderer_BeforeRender;
    }

    public static void Unload()
    {
        On.Celeste.LightingRenderer.BeforeRender -= LightingRenderer_BeforeRender;
    }

    private static void LightingRenderer_BeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene)
    {
        orig(self, scene);

        if (scene.Tracker.GetEntity<ClampLightColorController>() is ClampLightColorController controller)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, controller.clampMethod switch {
                ClampMethod.Clamp => clampColorState,
                ClampMethod.Tint => tintColorState,
                _ => throw new Exception($"Invalid color clamping method: {controller.clampMethod}"),
            });
            Draw.Rect(new Rectangle(0, 0, GameplayBuffers.Light.Width, GameplayBuffers.Light.Height), controller.clampColor);
            Draw.SpriteBatch.End();
        }
    }
}