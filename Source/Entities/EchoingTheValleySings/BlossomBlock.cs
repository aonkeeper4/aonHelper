namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[CustomEntity("aonHelper/BlossomBlock")]
[Tracked]
public class BlossomBlock : Solid
{
    private const float AmbientParticleInterval = 1f, AmbientParticleChance = 0.1f;
    
    public class BlossomBlockGroupData(float width, float height, Vector2 center, Vector2 scaleStrength)
        : Component(false, false)
    {
        public readonly float Width = width, Height = height;
        public readonly Vector2 Center = center;

        public Vector2 Scale = Vector2.One;
        public Vector2 TargetScale = Vector2.One;
        public readonly Vector2 ScaleStrength = scaleStrength;

        public override void Added(Entity entity)
        {
            if (entity is not BlossomBlock)
                throw new Exception($"{nameof(BlossomBlockGroupData)} added to non-{nameof(BlossomBlock)} entity!");
            
            base.Added(entity);
        }
    }
    
    private ParticleType P_Ambient;
    private ParticleType P_Break;
    
    private BlossomBlock groupLeader;
    private List<BlossomBlock> group;
    private bool GroupLeader => groupLeader == this;
    private BlossomBlockGroupData GroupData => groupLeader.Get<BlossomBlockGroupData>();

    private readonly BlossomBlockRenderer.Rendered rendererComponent;
    private SwirlDisplacementVertex[] RendererVertices => rendererComponent.Renderer.Vertices;
    public int VerticesStart;
    public int VerticesEnd;

    private const float BreakTime = 0.2f;
    private bool broken;
    private readonly string doNotLoadFlag;
    private readonly string flagOnBreak;

    private string homeLevelName;

    private IEnumerable<Spikes> Spikes => staticMovers.Select(s => s.Entity).OfType<Spikes>();
    
    public BlossomBlock(Vector2 position, int width, int height,
        int depth, string doNotLoadFlag, string flagOnBreak)
        : base(position, width, height, false)
    {
        Depth = depth;
        
        this.doNotLoadFlag = string.IsNullOrEmpty(doNotLoadFlag) ? null : doNotLoadFlag;
        this.flagOnBreak = string.IsNullOrEmpty(flagOnBreak) ? null : flagOnBreak;

        Add(rendererComponent = new BlossomBlockRenderer.Rendered());
        
        Add(new SoundWaveCollider(OnSoundWaveCollide));
        Add(new LightOcclude(0.4f));
    }

    public BlossomBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height,
            data.Int("depth", BlossomBlockController.DefaultDepth), data.Attr("doNotLoadFlag"), data.Attr("flagOnBreak"))
    { }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        Level level = SceneAs<Level>();
        homeLevelName = level.Session.Level;

        if (doNotLoadFlag is not null && level.Session.GetFlag(doNotLoadFlag))
        {
            RemoveSelf();
            return;
        }

        if (flagOnBreak is not null && level.Session.GetFlag(flagOnBreak) && !broken)
            level.Session.SetFlag(flagOnBreak, false);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (group is null)
        {
            groupLeader = this;
            group = [this];
            FindInGroup(this);

            float groupLeft = float.MaxValue, groupRight = float.MinValue, groupTop = float.MaxValue, groupBottom = float.MinValue;
            foreach (BlossomBlock block in group)
            {
                if (block.Left < groupLeft)
                    groupLeft = block.Left;

                if (block.Right > groupRight)
                    groupRight = block.Right;

                if (block.Top < groupTop)
                    groupTop = block.Top;

                if (block.Bottom > groupBottom)
                    groupBottom = block.Bottom;
            }

            float groupWidth = groupRight - groupLeft;
            float groupHeight = groupBottom - groupTop;
            Vector2 groupCenter = new Vector2(groupLeft + groupRight, groupTop + groupBottom) / 2f;
            Vector2 groupScaleStrength = Vector2.One;
            if (groupWidth > 32f)
                groupScaleStrength.X = groupWidth / 32f;
            if (groupHeight > 32f)
                groupScaleStrength.Y = groupHeight / 32f;
            
            Add(new BlossomBlockGroupData(groupWidth, groupHeight, groupCenter, groupScaleStrength));
        }

        SetupFromController(rendererComponent.Renderer.GetController());
    }
    
    private static void FindInGroup(BlossomBlock parent)
    {
        foreach (BlossomBlock block in parent.Scene.Tracker
            .GetEntities<BlossomBlock>()
            .Cast<BlossomBlock>()
            .Where(block => CanGroupWith(block, parent)))
        {
            parent.group.Add(block);
            block.groupLeader = parent.groupLeader;
            block.group = parent.group;
            FindInGroup(block);
        }
    }

    private static bool CanGroupWith(BlossomBlock other, BlossomBlock parent)
        => other != parent
            && other.Depth == parent.Depth
            && other.homeLevelName == parent.homeLevelName // prevent jank when blocks overlap a screen transition
            && CheckNextTo(parent, other)
            && !parent.group.Contains(other);

    private static bool CheckNextTo(BlossomBlock a, BlossomBlock b)
    {
        Rectangle horizontalCheckRect = new((int) (a.X - 1f), (int) a.Y, (int) (a.Width + 2f), (int) a.Height);
        Rectangle verticalCheckRect = new((int) a.X, (int) (a.Y - 1f), (int) a.Width, (int) (a.Height + 2f));

        return b.CollideRect(horizontalCheckRect) || b.CollideRect(verticalCheckRect);
    }

    private void SetupFromController(BlossomBlockController controller)
    {
        SurfaceSoundIndex = controller?.SurfaceIndex ?? BlossomBlockController.DefaultSurfaceIndex;

        float ambientParticleDirection = controller?.AmbientParticleDirection ?? BlossomBlockController.DefaultAmbientParticleDirection;
        ParticleType baseParticleType = new() {
            Source = GFX.Game["particles/petal"],
            Size = 1f,
            Color = controller?.BreakParticleColor1 ?? BlossomBlockController.DefaultParticleColor1,
            Color2 = controller?.BreakParticleColor2 ?? BlossomBlockController.DefaultParticleColor2,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            RotationMode = ParticleType.RotationModes.Random
        };
        P_Ambient = new ParticleType(baseParticleType) {
            Friction = 10f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            Acceleration = Calc.AngleToVector(ambientParticleDirection, 20f),
            Direction = ambientParticleDirection,
            DirectionRange = 20f * Calc.DegToRad,
            LifeMin = 2f,
            LifeMax = 3f,
            SpinMin = 0.6f,
            SpinMax = 1f
        };
        P_Break = new ParticleType(baseParticleType) {
            Friction = 120f,
            SpeedMin = 40f,
            SpeedMax = 60f,
            Acceleration = Vector2.UnitY * 20f,
            DirectionRange = 20f * Calc.DegToRad,
            LifeMin = 0.4f,
            LifeMax = 0.8f
        };
    }

    private SoundWaveCollider.SoundWaveCollisionResults OnSoundWaveCollide(Vector2 direction)
    {
        if (broken)
            return SoundWaveCollider.SoundWaveCollisionResults.None;
        
        Break(direction);
        return SoundWaveCollider.SoundWaveCollisionResults.Destroy;
    }

    private void Break(Vector2 direction)
    {
        if (broken)
            return;
        
        Level level = SceneAs<Level>();

        foreach (BlossomBlock block in group)
        {
            block.Collidable = false;
            block.broken = true;
            
            block.DisableStaticMovers();
            foreach (Spikes spikes in block.Spikes)
                spikes.Visible = true;
            
            if (block.flagOnBreak is not null)
                level.Session.SetFlag(block.flagOnBreak);
        }

        Camera camera = level.Camera;
        Vector2 cameraPosition = new Vector2(camera.Left + camera.Right, camera.Top + camera.Bottom) / 2f;
        Vector2 breakSoundPosition = group.Select(block => block.Center).MinBy(v => Vector2.Distance(v, cameraPosition));
        Audio.Play(aonHelperSFX.game_echoingthevalleysings_blossom_block_break, breakSoundPosition);

        float burstRadius = Calc.ClampedMap(MathF.Min(GroupData.Width, GroupData.Height), 16f, 64f, 8f, 24f);
        level.Displacement.AddBurst(GroupData.Center, BreakTime, burstRadius / 2f, burstRadius, 0.4f);

        GroupData.TargetScale = new Vector2(
            1f + (MathF.Abs(direction.Y) * 0.5f - MathF.Abs(direction.X) * 0.5f) / GroupData.ScaleStrength.X,
            1f + (MathF.Abs(direction.X) * 0.5f - MathF.Abs(direction.Y) * 0.5f) / GroupData.ScaleStrength.Y);
        
        foreach (BlossomBlock block in group)
        {
            int particleCount = GetParticleCount(0.4f, 4);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 particlePos = Calc.Random.Range(block.TopLeft, block.BottomRight);
                level.ParticlesBG.Emit(P_Break, particlePos, (particlePos - block.GroupData.Center).Angle());
            }
        }
        
        Add(new Coroutine(DisappearRoutine()));
    }

    private IEnumerator DisappearRoutine()
    {
        Dictionary<BlossomBlock, Vector3[]> originalVertexPositions = group.ToDictionary(
            block => block,
            block => block.RendererVertices
                .Take(block.VerticesStart..block.VerticesEnd)
                .Select(vertex => vertex.Position)
                .ToArray()
        );
        
        for (float t = 0f; t <= 1f; t += Engine.DeltaTime / BreakTime)
        {
            float tEased = Ease.CubeOut(t);

            foreach ((BlossomBlock block, Vector3[] vertexPositions) in originalVertexPositions)
            {
                Color fadeColor = Color.White * (1f - tEased);
                
                for (int i = 0; i < vertexPositions.Length; i++)
                {
                    int offsetIndex = block.VerticesStart + i;

                    Vector3 groupCenter = new(block.GroupData.Center, 0f);
                    Vector3 groupScale = new(block.GroupData.Scale, 0f);
                    Vector3 scaledPos = groupCenter + (vertexPositions[i] - groupCenter) * groupScale;
                    
                    block.RendererVertices[offsetIndex].Position = scaledPos;
                    block.RendererVertices[offsetIndex].Color = fadeColor;
                }
                
                foreach (Spikes spikes in block.Spikes)
                {
                    spikes.SetOrigins(block.GroupData.Center);

                    foreach (Image image in spikes.Components.OfType<Image>())
                    {
                        image.Scale = block.GroupData.Scale;
                        image.Color = fadeColor;
                    }
                }
            }

            yield return null;
        }

        foreach (BlossomBlock block in group)
        {
            block.DestroyStaticMovers();
            block.RemoveSelf();
        }
    }
    
    private int GetParticleCount(float chance, int minimum)
        => (int) MathF.Ceiling(MathF.Max(Width * Height / 64f * chance, minimum) * Calc.Random.Range(0.8f, 1.2f));

    public override void Update()
    {
        base.Update();

        if (!broken
            && Scene.OnInterval(AmbientParticleInterval)
            && Calc.Random.Chance(AmbientParticleChance))
        {
            int particleCount = GetParticleCount(0.2f, 1);
            SceneAs<Level>().ParticlesBG.Emit(P_Ambient, particleCount, Center, new Vector2(Width, Height) / 2f);
        }

        if (GroupLeader)
            GroupData.Scale = Calc.Approach(GroupData.Scale, GroupData.TargetScale, Engine.DeltaTime / BreakTime);
    }
}
