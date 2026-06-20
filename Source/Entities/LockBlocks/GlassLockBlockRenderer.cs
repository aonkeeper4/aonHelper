namespace Celeste.Mod.aonHelper.Entities.LockBlocks;

using GlassLockBlockRendererBase = Renderer<GlassLockBlockRenderer, GlassLockBlock, GlassLockBlockRenderer.GlassLockBlockBuffers, GlassLockBlockController>;

[Tracked]
public class GlassLockBlockRenderer : 
    GlassLockBlockRendererBase,
    GlassLockBlockRendererBase.IStaticMethods
{
    #region Static Method Implementation
    
    public static string Name => nameof(GlassLockBlockRenderer);
    public static string LogID => $"{nameof(aonHelper)}/{nameof(GlassLockBlockRenderer)}";
    
    public static GlassLockBlockRenderer Create(int rendererDepth) => new(rendererDepth);
    
    #endregion
    
    // as long as this isn't kept alive somehow by the gc i don't think it'll get cloned by srt
    // it should be cleaned up immediately?
    public struct GlassLockBlockBuffers : IBufferManager<GlassLockBlockBuffers>
    {
        private static readonly Dictionary<int, GlassLockBlockBuffers> Buffers = new();
        
        public VirtualRenderTarget Beams, Stars, Stencil;

        public static void QueryBuffers(int depth, out GlassLockBlockBuffers glassLockBlockBuffers)
        {
            if (!Buffers.TryGetValue(depth, out GlassLockBlockBuffers buffers)
                || buffers.Beams is not { IsDisposed: false }
                || buffers.Stars is not { IsDisposed: false }
                || buffers.Stencil is not { IsDisposed: false })
            {
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Beams);
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Stars);
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Stencil);

                string bufferIDPrefix = $"{nameof(aonHelper)}/{nameof(GlassLockBlockRenderer)}:{depth}";
                buffers = new GlassLockBlockBuffers
                {
                    Beams = VirtualContent.CreateRenderTarget(bufferIDPrefix + "_beams", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight),
                    Stars = VirtualContent.CreateRenderTarget(bufferIDPrefix + "_stars", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight),
                    Stencil = VirtualContent.CreateRenderTarget(bufferIDPrefix + "_stencil", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight)
                };
                Buffers[depth] = buffers;
                
                Logger.Info(LogID, $"Created new Glass Lock Block buffer triplet at depth {depth}.");
            }

            glassLockBlockBuffers = buffers;
        }
        
        #region Content Loading

        [OnLoadContent]
        internal static void LoadContent(bool _)
        {
            Buffers.Clear();
            aonHelperGFX.OnDisposeBuffers += DisposeBuffers;
        }

        private static void DisposeBuffers(ref int buffersDisposed)
        {
            foreach (int depth in Buffers.Keys)
            {
                GlassLockBlockBuffers buffers = Buffers[depth];
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Beams);
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Stars);
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Stencil);

                buffersDisposed += 3;
            }

            Buffers.Clear();
        }
        
        #endregion
    }

    private const int StarCount = 100;
    private struct Star
    {
        public Vector2 Position;
        public MTexture Texture;
        public int ColorIndex;
        public Vector2 Scroll;
    }
    private readonly Star[] stars = new Star[StarCount];

    private const int RayCount = 50;
    private struct Ray
    {
        public Vector2 Position;
        public float Width;
        public float Length;
        public float Alpha;
    }
    private readonly Ray[] rays = new Ray[RayCount];
    private readonly Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();

    private readonly VertexPositionColor[] verts = new VertexPositionColor[2700];

    private readonly BlendState overwriteColorBlendState = new()
    {
        ColorSourceBlend = Blend.DestinationAlpha,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };

    private GlassLockBlock[] toRender;
    private bool hasBlocks;

    public GlassLockBlockRenderer(int depth) : base(depth)
    {
        Add(new DisplacementRenderHook(OnRenderDisplacement));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        List<MTexture> starTextures = GFX.Game.GetAtlasSubtextures("particles/stars/");
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Position.X = Calc.Random.Next(320);
            stars[i].Position.Y = Calc.Random.Next(180);
            stars[i].Texture = Calc.Random.Choose(starTextures);
            stars[i].ColorIndex = Calc.Random.Next();
            stars[i].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
        }

        for (int i = 0; i < rays.Length; i++)
        {
            rays[i].Position.X = Calc.Random.Next(320);
            rays[i].Position.Y = Calc.Random.Next(180);
            rays[i].Width = Calc.Random.Range(4f, 16f);
            rays[i].Length = Calc.Random.Choose(48, 96, 128);
            rays[i].Alpha = Calc.Random.Range(0f, 1f);
        }
    }

    protected override void BeforeRender(GlassLockBlockBuffers buffers, GlassLockBlockController controller)
    {
        toRender = GetEntitiesToRender();
        hasBlocks = toRender.Length > 0;
        if (!hasBlocks)
            return;

        Camera camera = SceneAs<Level>().Camera;
        int screenWidth = RenderTargetHelper.GameplayWidth;
        int screenHeight = RenderTargetHelper.GameplayHeight;

        Color[] starColors = controller?.StarColors ?? GlassLockBlockController.DefaultStarColors;
        Color rayColor = controller?.RayColor ?? GlassLockBlockController.DefaultRayColor;
        
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffers.Stars);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        Vector2 origin = new(8f, 8f);
        for (int i = 0; i < stars.Length; i++)
        {
            MTexture starTexture = stars[i].Texture;
            Color starColor = starColors[stars[i].ColorIndex % starColors.Length];
            Vector2 starScroll = stars[i].Scroll;
            
            Vector2 starActualPosition = new(Calc.Mod(stars[i].Position.X - camera.X * (1f - starScroll.X), screenWidth), Calc.Mod(stars[i].Position.Y - camera.Y * (1f - starScroll.Y), screenHeight));
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
        for (int i = 0; i < rays.Length; i++)
        {
            Vector2 rayPosition = new(Calc.Mod(rays[i].Position.X - camera.X * 0.9f, screenWidth), Calc.Mod(rays[i].Position.Y - camera.Y * 0.9f, screenHeight));
            DrawRay(rays[i], rayPosition, rayColor * rays[i].Alpha, ref vertex);
            
            if (rayPosition.X < 64f)
                DrawRay(rays[i], rayPosition + new Vector2(screenWidth, 0f), rayColor * rays[i].Alpha, ref vertex);
            else if (rayPosition.X > screenWidth - 64f)
                DrawRay(rays[i], rayPosition - new Vector2(screenWidth, 0f), rayColor * rays[i].Alpha, ref vertex);
            if (rayPosition.Y < 64f)
                DrawRay(rays[i], rayPosition + new Vector2(0f, screenHeight), rayColor * rays[i].Alpha, ref vertex);
            else if (rayPosition.Y > screenHeight - 64f)
                DrawRay(rays[i], rayPosition - new Vector2(0f, screenHeight), rayColor * rays[i].Alpha, ref vertex);
        }

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffers.Beams);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        GFX.DrawVertices(Matrix.Identity, verts, vertex);
    }

    private void DrawRay(Ray ray, Vector2 position, Color color, ref int vertex)
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

        Quad(ref vertex, topRightEnd, topRightStart, topLeftStart, topLeftEnd, Color.Transparent, color, color, Color.Transparent);
        Quad(ref vertex, topRightStart, bottomRightStart, bottomLeftStart, topLeftStart, color, color, color, color);
        Quad(ref vertex, bottomRightStart, bottomRightEnd, bottomLeftEnd, bottomLeftStart, color, Color.Transparent, Color.Transparent, color);
    }

    private void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3)
    {
        // tri 1
        verts[vertex].Position.X = v0.X;
        verts[vertex].Position.Y = v0.Y;
        verts[vertex++].Color = c0;
        verts[vertex].Position.X = v1.X;
        verts[vertex].Position.Y = v1.Y;
        verts[vertex++].Color = c1;
        verts[vertex].Position.X = v2.X;
        verts[vertex].Position.Y = v2.Y;
        verts[vertex++].Color = c2;
        
        // tri 2
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

        Vector2 cameraPos = SceneAs<Level>().Camera.Position;
        QueryBuffers(out GlassLockBlockBuffers buffers);
        GlassLockBlockController controller = GetController();
        Dictionary<GlassLockBlock, Rectangle> blockRenderBounds = toRender.Where(block => block.RenderBounds is not null)
                                                                          .ToDictionary(block => block, block => block.RenderBounds ?? new Rectangle(0, 0, -1, -1));

        foreach ((GlassLockBlock block, Rectangle rb) in blockRenderBounds)
            Draw.Rect(block.Center.X + rb.Left, block.Center.Y + rb.Top, rb.Width, rb.Height, controller?.BgColor ?? GlassLockBlockController.DefaultBgColor);
        foreach ((GlassLockBlock block, Rectangle rb) in blockRenderBounds)
        {
            Rectangle clipTarget = new((int)(block.Center.X + rb.Left - cameraPos.X), (int)(block.Center.Y + rb.Top - cameraPos.Y), rb.Width, rb.Height);
            Draw.SpriteBatch.Draw(buffers.Stars, block.Center + rb.TopLeft(), clipTarget, Color.White);
        }
        foreach ((GlassLockBlock block, Rectangle rb) in blockRenderBounds)
        {
            Rectangle clipTarget = new((int)(block.Center.X + rb.Left - cameraPos.X), (int)(block.Center.Y + rb.Top - cameraPos.Y), rb.Width, rb.Height);
            Draw.SpriteBatch.Draw(buffers.Beams, block.Center + rb.TopLeft(), clipTarget, Color.White);
        }
    }

    private void OnRenderDisplacement()
    {
        if (!hasBlocks)
            return;
        
        Camera camera = SceneAs<Level>().Camera;
        QueryBuffers(out GlassLockBlockBuffers buffers);
        GlassLockBlockController controller = GetController();

        foreach (GlassLockBlock block in toRender)
        {
            if (block.RenderBounds is not { } rb)
                continue;
            
            if (controller?.VanillaEdgeBehavior ?? GlassLockBlockController.DefaultVanillaEdgeBehavior)
                Draw.Rect(block.Center.X + rb.Left, block.Center.Y + rb.Top, rb.Width, rb.Height, new Color(0.5f, 0.5f, 0.2f, 1f));
            else
                Draw.Rect(block.Center.X + rb.Left + 1f, block.Center.Y + rb.Top + 1f, rb.Width - 2f, rb.Height - 2f, new Color(0.5f, 0.5f, 0.2f, 1f));
        }
        
        Draw.SpriteBatch.End();
        
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffers.Stencil);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
        foreach (GlassLockBlock block in toRender)
            block.Sprite.Texture.DrawCentered(block.Center);
        Draw.SpriteBatch.End();
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, overwriteColorBlendState, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
        foreach (GlassLockBlock block in toRender)
        {
            MTexture tex = block.Sprite.Texture;
            Draw.Rect(block.Center.X - tex.Width / 2, block.Center.Y - tex.Height / 2, tex.Width, tex.Height, new Color(0.5f, 0.5f, 0f, 1f));
        }
        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Displacement);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
        Draw.SpriteBatch.Draw(buffers.Stencil, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
    }
}