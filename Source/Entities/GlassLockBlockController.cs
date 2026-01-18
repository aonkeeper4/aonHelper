using Celeste.Mod.aonHelper.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[Tracked]
[CustomEntity("aonHelper/GlassLockBlockController", "MoreLockBlocks/GlassLockBlockController")]
public class GlassLockBlockController : Entity
{
    private static readonly string[] EntitySIDs = ["aonHelper/GlassLockBlockController", "MoreLockBlocks/GlassLockBlockController"];

    private struct Star
    {
        public Vector2 Position;
        public MTexture Texture;
        public Color Color;
        public Vector2 Scroll;
    }

    private struct Ray
    {
        public Vector2 Position;
        public float Width;
        public float Length;
        public Color Color;
    }

    private const int StarCount = 100;
    private const int RayCount = 50;

    private readonly Star[] stars = new Star[StarCount];
    private readonly Ray[] rays = new Ray[RayCount];

    private readonly VertexPositionColor[] verts = new VertexPositionColor[2700];

    private readonly Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();

    private VirtualRenderTarget beamsTarget;
    private VirtualRenderTarget starsTarget;
    private VirtualRenderTarget stencilTarget;

    private readonly BlendState overwriteColorBlendState = new()
    {
        ColorSourceBlend = Blend.DestinationAlpha,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
    };

    private bool hasBlocks;

    public readonly Color[] StarColors;
    public readonly Color BgColor, LineColor, RayColor;
    public readonly bool VanillaEdgeBehavior;

    public GlassLockBlockController(
        Vector2 position,
        Color bgColor, Color lineColor, Color rayColor, Color[] starColors,
        bool wavy, bool vanillaEdgeBehavior,
        bool persistent)
        : base(position)
    {
        Depth = -9990;

        BgColor = bgColor;
        LineColor = lineColor;
        RayColor = rayColor;
        StarColors = starColors;
        VanillaEdgeBehavior = vanillaEdgeBehavior;

        Add(new BeforeRenderHook(BeforeRender));
        if (wavy)
            Add(new DisplacementRenderHook(OnDisplacementRender));

        if (persistent)
            aonHelperModule.Session.GlassLockBlockCurrentSettings = new aonHelperModuleSession.GlassLockBlockState
            {
                BgColor = bgColor,
                LineColor = lineColor,
                RayColor = rayColor,
                StarColors = starColors,
                Wavy = wavy,
                VanillaEdgeBehavior = vanillaEdgeBehavior
            };
        else
            aonHelperModule.Session.GlassLockBlockCurrentSettings = null;
    }

    public GlassLockBlockController(EntityData data, Vector2 offset)
        : this(data.Position + offset,
            data.HexColor("bgColor"), data.HexColor("lineColor"), data.HexColor("rayColor"), data.Attr("starColors")
                                                                                                 .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                                                                 .Select(Calc.HexToColor)
                                                                                                 .ToArray(),
            data.Bool("wavy"), data.Bool("vanillaEdgeBehavior"),
            data.Bool("persistent"))
    { }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        List<MTexture> starTextures = GFX.Game.GetAtlasSubtextures("particles/stars/");
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Position.X = Calc.Random.Next(320);
            stars[i].Position.Y = Calc.Random.Next(180);
            stars[i].Texture = Calc.Random.Choose(starTextures);
            stars[i].Color = Calc.Random.Choose(StarColors);
            stars[i].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
        }

        for (int i = 0; i < rays.Length; i++)
        {
            rays[i].Position.X = Calc.Random.Next(320);
            rays[i].Position.Y = Calc.Random.Next(180);
            rays[i].Width = Calc.Random.Range(4f, 16f);
            rays[i].Length = Calc.Random.Choose(48, 96, 128);
            rays[i].Color = RayColor * Calc.Random.Range(0.2f, 0.4f);
        }
    }

    private void BeforeRender()
    {
        List<GlassLockBlock> glassBlocks = GetGlassBlocksToAffect().ToList();
        hasBlocks = glassBlocks.Count > 0;
        if (!hasBlocks)
            return;

        Camera camera = SceneAs<Level>().Camera;
        int screenWidth = aonHelperGFX.GameplayBufferWidth;
        int screenHeight = aonHelperGFX.GameplayBufferHeight;

        starsTarget ??= VirtualContent.CreateRenderTarget("aonHelper/glassLockBlockSurfaces", screenWidth, screenHeight);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        
        Vector2 origin = new(8f, 8f);
        for (int i = 0; i < stars.Length; i++)
        {
            MTexture starTexture = stars[i].Texture;
            Color starColor = stars[i].Color;
            Vector2 starScroll = stars[i].Scroll;
            
            Vector2 starActualPosition = new(Mod(stars[i].Position.X - camera.X * (1f - starScroll.X), screenWidth), Mod(stars[i].Position.Y - camera.Y * (1f - starScroll.Y), screenHeight));
            starTexture.Draw(starActualPosition, origin, starColor);
            
            if (starActualPosition.X < origin.X)
                starTexture.Draw(starActualPosition + new Vector2(screenWidth, 0f), origin, starColor);
            else if (starActualPosition.X > screenWidth - origin.X)
                starTexture.Draw(starActualPosition - new Vector2(screenWidth, 0f), origin, starColor);
            if (starActualPosition.Y < origin.Y)
                starTexture.Draw(starActualPosition + new Vector2(0f, screenHeight), origin, starColor);
            else if (starActualPosition.Y > screenHeight - origin.Y)
                starTexture.Draw(starActualPosition - new Vector2(0f, screenHeight), origin, starColor);
        }
        
        Draw.SpriteBatch.End();

        int vertex = 0;
        for (int j = 0; j < rays.Length; j++)
        {
            Vector2 rayPosition = new(Mod(rays[j].Position.X - camera.X * 0.9f, screenWidth), Mod(rays[j].Position.Y - camera.Y * 0.9f, screenHeight));
            DrawRay(rayPosition, ref vertex, ref rays[j]);
            
            if (rayPosition.X < 64f)
                DrawRay(rayPosition + new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
            else if (rayPosition.X > (screenWidth - 64))
                DrawRay(rayPosition - new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
            if (rayPosition.Y < 64f)
                DrawRay(rayPosition + new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
            else if (rayPosition.Y > (screenHeight - 64))
                DrawRay(rayPosition - new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
        }

        beamsTarget ??= VirtualContent.CreateRenderTarget("aonHelper/glassLockBlockBeams", screenWidth, screenHeight);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        
        GFX.DrawVertices(Matrix.Identity, verts, vertex);
    }

    private void DrawRay(Vector2 position, ref int vertex, ref Ray ray)
    {
        Vector2 rayDir = new(0f - rayNormal.Y, rayNormal.X);
        Vector2 rayWidth = rayNormal * ray.Width * 0.5f;
        Vector2 rayFadeStart = rayDir * ray.Length * 0.25f * 0.5f;
        Vector2 rayFadeLength = rayDir * ray.Length * 0.5f * 0.5f;
        
        Vector2 topLeftStart = position - rayWidth - rayFadeStart;
        Vector2 topLeftEnd = position - rayWidth - rayFadeStart - rayFadeLength;
        Vector2 topRightStart = position + rayWidth - rayFadeStart;
        Vector2 topRightEnd = position + rayWidth - rayFadeStart - rayFadeLength;
        
        Vector2 bottomLeftStart = position - rayWidth + rayFadeStart;
        Vector2 bottomLeftEnd = position - rayWidth + rayFadeStart + rayFadeLength;
        Vector2 bottomRightStart = position + rayWidth + rayFadeStart;
        Vector2 bottomRightEnd = position + rayWidth + rayFadeStart + rayFadeLength;
        
        Color color = ray.Color;

        Quad(ref vertex, topRightEnd, topRightStart, topLeftStart, topLeftEnd, Color.Transparent, color, color, Color.Transparent);
        Quad(ref vertex, topRightStart, bottomRightStart, bottomLeftStart, topLeftStart, color, color, color, color);
        Quad(ref vertex, bottomRightStart, bottomRightEnd, bottomLeftEnd, bottomLeftStart, color, Color.Transparent, Color.Transparent, color);
    }

    private void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3)
    {
        verts[vertex].Position.X = v0.X;
        verts[vertex].Position.Y = v0.Y;
        verts[vertex++].Color = c0;
        verts[vertex].Position.X = v1.X;
        verts[vertex].Position.Y = v1.Y;
        verts[vertex++].Color = c1;
        verts[vertex].Position.X = v2.X;
        verts[vertex].Position.Y = v2.Y;
        verts[vertex++].Color = c2;
        verts[vertex].Position.X = v0.X;
        verts[vertex].Position.Y = v0.Y;
        verts[vertex++].Color = c0;
        verts[vertex].Position.X = v2.X;
        verts[vertex].Position.Y = v2.Y;
        verts[vertex++].Color = c2;
        verts[vertex].Position.X = v3.X;
        verts[vertex].Position.Y = v3.Y;
        verts[vertex++].Color = c3;
    }

    public override void Render()
    {
        if (!hasBlocks)
            return;

        Vector2 position = SceneAs<Level>().Camera.Position;
        GlassLockBlock[] glassBlocks = GetGlassBlocksToAffect().ToArray();

        foreach (GlassLockBlock block in glassBlocks)
        {
            if (block.RenderBounds is not { } rb)
                continue;
            
            Draw.Rect(block.Center.X + rb.Left, block.Center.Y + rb.Top, rb.Width, rb.Height, BgColor);
        }

        if (starsTarget is not null && !starsTarget.IsDisposed)
            foreach (GlassLockBlock block in glassBlocks)
            {
                if (block.RenderBounds is not { } rb)
                    continue;
                
                Rectangle clipTarget = new((int)(block.Center.X + rb.Left - position.X), (int)(block.Center.Y + rb.Top - position.Y), rb.Width, rb.Height);
                Draw.SpriteBatch.Draw(starsTarget, block.Center + rb.TopLeft(), clipTarget, Color.White);
            }

        if (beamsTarget is not null && !beamsTarget.IsDisposed)
            foreach (GlassLockBlock block in glassBlocks)
            {
                if (block.RenderBounds is not { } rb)
                    continue;
                
                Rectangle clipTarget = new((int)(block.Center.X + rb.Left - position.X), (int)(block.Center.Y + rb.Top - position.Y), rb.Width, rb.Height);
                Draw.SpriteBatch.Draw(beamsTarget, block.Center + rb.TopLeft(), clipTarget, Color.White);
            }
    }

    protected virtual GlassLockBlock[] GetGlassBlocksToAffect()
        => Scene.Tracker.GetEntities<GlassLockBlock>().OfType<GlassLockBlock>().ToArray();

    public override void Removed(Scene scene)
    {
        Dispose();
        
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        Dispose();
        
        base.SceneEnd(scene);
    }

    private void Dispose()
    {
        if (starsTarget is { IsDisposed: false })
            starsTarget.Dispose();
        if (beamsTarget is { IsDisposed: false })
            beamsTarget.Dispose();
        if (stencilTarget is { IsDisposed: false })
            stencilTarget.Dispose();
        
        starsTarget = null;
        beamsTarget = null;
        stencilTarget = null;
    }

    private static float Mod(float x, float m)
        => (x % m + m) % m;

    private void OnDisplacementRender()
    {
        Camera camera = SceneAs<Level>().Camera;
        IEnumerable<GlassLockBlock> blocks = GetGlassBlocksToAffect();

        foreach (GlassLockBlock block in blocks)
        {
            if (block.RenderBounds is not { } rb)
                continue;
            
            if (VanillaEdgeBehavior)
                Draw.Rect(block.Center.X + rb.Left, block.Center.Y + rb.Top, rb.Width, rb.Height, new Color(0.5f, 0.5f, 0.2f, 1f));
            else
                Draw.Rect(block.Center.X + rb.Left + 1f, block.Center.Y + rb.Top + 1f, rb.Width - 2f, rb.Height - 2f, new Color(0.5f, 0.5f, 0.2f, 1f));
        }
        
        Draw.SpriteBatch.End();

        stencilTarget ??= VirtualContent.CreateRenderTarget("aonHelper/glassLockBlockDisplacementOverride", GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(stencilTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
        
        foreach (GlassLockBlock block in blocks)
            block.Sprite.Texture.DrawCentered(block.Center);
        
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, overwriteColorBlendState, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
        
        foreach (GlassLockBlock block in blocks)
        {
            MTexture tex = block.Sprite.Texture;
            Draw.Rect(block.Center.X - tex.Width / 2, block.Center.Y - tex.Height / 2, tex.Width, tex.Height, new Color(0.5f, 0.5f, 0f, 1f));
        }
        
        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Displacement);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(stencilTarget, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
    }

    #region Hooks

    public static void Load()
    {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
    }

    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        if (self.Session.LevelData is { } levelData
            && levelData.Entities.All(entity => !EntitySIDs.Contains(entity.Name)))
        {
            self.Add(aonHelperModule.Session.GlassLockBlockCurrentSettings is {} currentSettings
                ? new GlassLockBlockController(
                    Vector2.Zero,
                    currentSettings.BgColor, currentSettings.LineColor, currentSettings.RayColor, currentSettings.StarColors,
                    currentSettings.Wavy, currentSettings.VanillaEdgeBehavior,
                    false)
                : new GlassLockBlockController(
                    Vector2.Zero,
                    Calc.HexToColor("0d2e89"), Color.White, Color.White, [Calc.HexToColor("7f9fba"), Calc.HexToColor("9bd1cd"), Calc.HexToColor("bacae3")],
                    true, true,
                    false));
        }

        orig(self, playerIntro, isFromLoader);
    }

    #endregion
}