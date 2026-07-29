namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

using BlossomBlockRendererBase = Renderer<BlossomBlockRenderer, BlossomBlock, BlossomBlockRenderer.BlossomBlockBuffers, BlossomBlockController>;

[Tracked]
public class BlossomBlockRenderer(int depth) :
    BlossomBlockRendererBase(depth),
    BlossomBlockRendererBase.IStaticMethods
{
    #region Static Method Implementation

    public static string Name => nameof(BlossomBlockRenderer);
    public static string LogID => $"{nameof(aonHelper)}/{nameof(BlossomBlockRenderer)}";

    public static BlossomBlockRenderer Create(int rendererDepth) => new(rendererDepth);

    #endregion

    public struct BlossomBlockBuffers : IBufferManager<BlossomBlockBuffers>
    {
        private static readonly Dictionary<int, BlossomBlockBuffers> Buffers = new();

        public VirtualRenderTarget Blocks;

        public static void QueryBuffers(int depth, out BlossomBlockBuffers glassLockBlockBuffers)
        {
            if (!Buffers.TryGetValue(depth, out BlossomBlockBuffers buffers))
            {
                buffers = new BlossomBlockBuffers();
                Logger.Info(LogID, $"Created new Blossom Block buffer at depth {depth}.");
            }

            string bufferIDPrefix = $"{nameof(aonHelper)}/{nameof(BlossomBlockRenderer)}:{depth}";
            RenderTargetHelper.CreateOrResizeGameplayTarget(ref buffers.Blocks, bufferIDPrefix + "_blocks");

            glassLockBlockBuffers = Buffers[depth] = buffers;
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
                BlossomBlockBuffers buffers = Buffers[depth];
                RenderTargetHelper.DisposeAndSetNull(ref buffers.Blocks);

                buffersDisposed++;
            }

            Buffers.Clear();
        }

        #endregion
    }

    private const int BaseVertexCount = 1024;
    public SwirlDisplacementVertex[] Vertices = new SwirlDisplacementVertex[BaseVertexCount];
    private int vertexCount;
    
    private bool dirty;

    private float timer;

    public override void EntityTracked(Rendered entity)
    {
        base.EntityTracked(entity);
        dirty = true;
    }
    public override void EntityUntracked(Rendered entity)
    {
        base.EntityUntracked(entity);
        dirty = true;
    }
    
    public override void Update()
    { 
        if (dirty)
            RemeshBlocks();
        
        timer += Engine.DeltaTime;
    }
    
    private void RemeshBlocks()
    {
        dirty = false;

        BlossomBlockController controller = GetController();
        
        int vertexIndex = 0;
        BlossomBlock[] blocks = GetEntities();
        foreach (BlossomBlock block in blocks)
        {
            int w = (int) (block.Width / 8f);
            int h = (int) (block.Height / 8f);
            int endVertexIndex = vertexIndex + w * h * 6;
            while (endVertexIndex >= Vertices.Length)
                Array.Resize(ref Vertices, Vertices.Length * 2);

            block.VerticesStart = vertexIndex;
            block.VerticesEnd = endVertexIndex;
            
            for (float x = block.Left; x < block.Right; x += 8f)
                for (float y = block.Top; y < block.Bottom; y += 8f)
                {
                    bool blockLeft = CheckForSame(blocks, x - 8f, y);
                    bool blockRight = CheckForSame(blocks, x + 8f, y);
                    bool blockAbove = CheckForSame(blocks, x, y - 8f);
                    bool blockBelow = CheckForSame(blocks, x, y + 8f);
                    
                    if (blockLeft && blockRight && blockAbove && blockBelow)
                    {
                        if (!CheckForSame(blocks, x + 8f, y - 8f))
                            AddTexture(controller, x, y, 3, 0, ref vertexIndex);
                        else if (!CheckForSame(blocks, x - 8f, y - 8f))
                            AddTexture(controller, x, y, 3, 1, ref vertexIndex);
                        else if (!CheckForSame(blocks, x + 8f, y + 8f))
                            AddTexture(controller, x, y, 3, 2, ref vertexIndex);
                        else if (!CheckForSame(blocks, x - 8f, y + 8f))
                            AddTexture(controller, x, y, 3, 3, ref vertexIndex);
                        else
                            AddTexture(controller, x, y, 1, 1, ref vertexIndex);
                    }
                    else if (blockLeft && blockRight && !blockAbove && blockBelow)
                        AddTexture(controller, x, y, 1, 0, ref vertexIndex);
                    else if (blockLeft && blockRight && blockAbove && !blockBelow)
                        AddTexture(controller, x, y, 1, 2, ref vertexIndex);
                    else if (blockLeft && !blockRight && blockAbove && blockBelow)
                        AddTexture(controller, x, y, 2, 1, ref vertexIndex);
                    else if (!blockLeft && blockRight && blockAbove && blockBelow)
                        AddTexture(controller, x, y, 0, 1, ref vertexIndex);
                    else if (blockLeft && !blockRight && !blockAbove && blockBelow)
                        AddTexture(controller, x, y, 2, 0, ref vertexIndex);
                    else if (!blockLeft && blockRight && !blockAbove && blockBelow)
                        AddTexture(controller, x, y, 0, 0, ref vertexIndex);
                    else if (blockLeft && !blockRight && blockAbove && !blockBelow)
                        AddTexture(controller, x, y, 2, 2, ref vertexIndex);
                    else if (!blockLeft && blockRight && blockAbove && !blockBelow)
                        AddTexture(controller, x, y, 0, 2, ref vertexIndex);
                }
        }

        vertexCount = vertexIndex;
    }

    private bool CheckForSame(BlossomBlock[] blocks, float x, float y)
        => blocks.Any(block => block.CollideRect(new Rectangle((int) x, (int) y, 8, 8)));
    
    private void AddTexture(BlossomBlockController controller, float x, float y, int tx, int ty, ref int vertexIndex)
    {
        string spritePath = controller?.SpritePath ?? BlossomBlockController.DefaultSpritePath;
        MTexture source = GFX.Game[spritePath];

        float minSwirlRadius = controller?.MinSwirlRadius ?? BlossomBlockController.DefaultMinSwirlRadius;
        float maxSwirlRadius = controller?.MaxSwirlRadius ?? BlossomBlockController.DefaultMaxSwirlRadius;
        float minSwirlSpeed = controller?.MinSwirlSpeed ?? BlossomBlockController.DefaultMinSwirlSpeed;
        float maxSwirlSpeed = controller?.MaxSwirlSpeed ?? BlossomBlockController.DefaultMaxSwirlSpeed;

        MTexture subtexture = source.GetSubtexture(tx * 8, ty * 8, 8, 8);

        Vector3 worldPos = new(x, y, 0f);
        SwirlDisplacementVertex a = new(worldPos + new Vector3(0f, 0f, 0f), Color.White, new Vector2(subtexture.LeftUV, subtexture.TopUV), minSwirlRadius, maxSwirlRadius, minSwirlSpeed, maxSwirlSpeed);
        SwirlDisplacementVertex b = new(worldPos + new Vector3(8f, 0f, 0f), Color.White, new Vector2(subtexture.RightUV, subtexture.TopUV), minSwirlRadius, maxSwirlRadius, minSwirlSpeed, maxSwirlSpeed);
        SwirlDisplacementVertex c = new(worldPos + new Vector3(0f, 8f, 0f), Color.White, new Vector2(subtexture.LeftUV, subtexture.BottomUV), minSwirlRadius, maxSwirlRadius, minSwirlSpeed, maxSwirlSpeed);
        SwirlDisplacementVertex d = new(worldPos + new Vector3(8f, 8f, 0f), Color.White, new Vector2(subtexture.RightUV, subtexture.BottomUV), minSwirlRadius, maxSwirlRadius, minSwirlSpeed, maxSwirlSpeed);
            
        Vertices[vertexIndex++] = a;
        Vertices[vertexIndex++] = b;
        Vertices[vertexIndex++] = c;
        Vertices[vertexIndex++] = b;
        Vertices[vertexIndex++] = c;
        Vertices[vertexIndex++] = d;
    }

    protected override void BeforeRender(BlossomBlockBuffers buffers, BlossomBlockController controller)
    {
        if (vertexCount <= 0)
            return;
        
        Camera camera = SceneAs<Level>().Camera;
        string spritePath = controller?.SpritePath ?? BlossomBlockController.DefaultSpritePath;
        
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffers.Blocks);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        
        using (new EffectHelper.ResetGraphicsDeviceOnDispose(Engine.Graphics.GraphicsDevice,
            out EffectHelper.ResetGraphicsDeviceOnDispose.SlotRegistrar registrar))
        {
            Engine.Graphics.GraphicsDevice.Textures[registrar.Current] = GFX.Game[spritePath].Texture.Texture_Safe;
            Engine.Graphics.GraphicsDevice.SamplerStates[registrar.Current] = SamplerState.PointClamp;
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Engine.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Effect swirlDisplacementEffect = aonHelperGFX.FxSwirlDisplacement;
            swirlDisplacementEffect.Parameters["time"].SetValue(timer);
            swirlDisplacementEffect.Parameters["depth"].SetValue(Depth);

            GFX.DrawVertices(camera.Matrix, Vertices, vertexCount, swirlDisplacementEffect);
        }
    }

    public override void Render()
    {
        if (vertexCount <= 0)
            return;
        
        QueryBuffers(out BlossomBlockBuffers buffers);
        
        Camera camera = SceneAs<Level>().Camera;
        Draw.SpriteBatch.Draw(buffers.Blocks, camera.Position, Color.White);
    }
}
