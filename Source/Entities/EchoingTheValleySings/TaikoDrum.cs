using Celeste.Mod.aonHelper.Components.Colliders;

namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[CustomEntity("aonHelper/TaikoDrum")]
[Tracked]
public class TaikoDrum : Solid
{
    [Pooled]
	private class TaikoDrumDebris : Actor
	{
		private readonly Image image;
	
		private float lifeTimer;
		private float alpha;
	
		private Vector2 speed;
		private int rotateSign;
	
		private bool hasHitGround;
	
		public TaikoDrumDebris()
			: base(Vector2.Zero)
		{
			Collider = new Hitbox(4f, 4f, -2f, -2f);
			Tag = Tags.Persistent;
			Depth = 2000;
			
			Add(image = new Image(null));
		}
	
		public TaikoDrumDebris Init(Vector2 pos, MTexture texture)
		{
			Position = pos;

			lifeTimer = Calc.Random.Range(0.6f, 2.6f);
			alpha = 1f;
			
			speed = Vector2.Zero;
			rotateSign = Calc.Random.Choose(1, -1);
			
			hasHitGround = false;
			
			image.Texture = texture;
			image.CenterOrigin();
			image.Color = Color.White * alpha;
			image.Rotation = Calc.Random.NextAngle();
			image.Scale = Calc.Random.Range(Vector2.One * 0.5f, Vector2.One);
			image.FlipX = Calc.Random.Chance(0.5f);
			image.FlipY = Calc.Random.Chance(0.5f);
			
			return this;
		}
	
		public TaikoDrumDebris BlastFrom(Vector2 from)
		{
			float length = Calc.Random.Range(30, 40);
			speed = (Position - from).SafeNormalize(length);
			speed = speed.Rotate(Calc.Random.Range(-MathF.PI / 12f, MathF.PI / 12f));
			
			return this;
		}
	
		private void OnCollideH(CollisionData data)
		{
			speed.X *= -0.8f;
		}

        private void OnCollideV(CollisionData data)
        {
            if (speed.Y > 0f)
                hasHitGround = true;

            speed.Y *= -0.6f;
            if (speed.Y < 0f && speed.Y > -50f)
                speed.Y = 0f;

            if (speed.Y != 0f || !hasHitGround)
                Audio.Play(SFX.game_gen_debris_wood, Position, "debris_velocity", Calc.ClampedMap(MathF.Abs(speed.Y), 0f, 150f));
        }

        public override void Update()
		{
			base.Update();
            
			MoveH(speed.X * Engine.DeltaTime, OnCollideH);
			MoveV(speed.Y * Engine.DeltaTime, OnCollideV);

			bool onGround = OnGround();
			speed.X = Calc.Approach(speed.X, 0f, (onGround ? 50f : 20f) * Engine.DeltaTime);
			if (!onGround)
				speed.Y = Calc.Approach(speed.Y, 100f, 400f * Engine.DeltaTime);
			
			if (lifeTimer > 0f)
				lifeTimer -= Engine.DeltaTime;
			else if (alpha > 0f)
			{
				alpha -= 4f * Engine.DeltaTime;
				if (alpha <= 0f)
					RemoveSelf();
			}
			
            image.Rotation += Math.Abs(speed.X) * rotateSign * Engine.DeltaTime;
			image.Color = Color.White * alpha;
		}
	}
    
    private readonly ParticleType P_Activate;
    
    public enum Axes
    {
        Horizontal,
        Vertical,
        Both
    }
    private readonly Axes axes;

    private readonly bool fragile;

    private readonly string doNotLoadFlag, flagOnBreak;
    private bool broken;
    
    private const float HitCooldownTime = 0.2f;
    private float hitCooldownTimer;
    private bool CanActivate => hitCooldownTimer <= 0f;

    private MTexture[,] textures;
    private MTexture[] debris;
    private float alpha = 1f;
    
    private Vector2 scale = Vector2.One;
    private readonly Vector2 scaleStrength = Vector2.One;

    private IEnumerable<Spikes> Spikes => staticMovers.Select(s => s.Entity).OfType<Spikes>();
    
    public TaikoDrum(Vector2 position, int width, int height,
        Axes axes, bool fragile,
        string doNotLoadFlag, string flagOnBreak,
        string spriteDir, int surfaceIndex, Color activateParticleColor)
        : base(position, width, height, true)
    {
        SurfaceSoundIndex = surfaceIndex;
        
        this.axes = axes;

        this.fragile = fragile;

        this.doNotLoadFlag = string.IsNullOrEmpty(doNotLoadFlag) ? null : doNotLoadFlag;
        this.flagOnBreak = string.IsNullOrEmpty(flagOnBreak) ? null : flagOnBreak;
        
        OnDashCollide = OnDashCollision;
        Add(new SoundWaveCollider(OnSoundWaveCollision));
        Add(new ExplosionCollider(OnExplosionCollision));

        BuildSprite(string.IsNullOrEmpty(spriteDir) ? "objects/aonHelper/taikoDrum" : spriteDir);
        
        if (Width > 32f)
            scaleStrength.X = Width / 32f;
        if (Height > 32f)
            scaleStrength.Y = Height / 32f;
        
        P_Activate = new ParticleType(Seeker.P_Regen) {
            SpeedMin = 20f,
            SpeedMax = 30f,
            Color = activateParticleColor,
            Color2 = activateParticleColor * 0.5f
        };
        
        Add(new LightOcclude());
    }

    public TaikoDrum(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height,
            data.Enum("axes", Axes.Horizontal), data.Bool("fragile"),
            data.Attr("doNotLoadFlag"), data.Attr("flagOnBreak"),
            data.Attr("spriteDir"), data.Int("surfaceIndex", SurfaceIndex.ResortWood), data.HexColor("activateParticleColor", Calc.HexToColor("f1dbc7")))
    { }

    private void BuildSprite(string spriteDir)
    {
        string spritePath = spriteDir
            + axes switch {
                Axes.Horizontal => "/horizontal",
                Axes.Vertical => "/vertical",
                Axes.Both => "/both",
                _ => throw new ArgumentOutOfRangeException()
            }
            + (fragile ? "_fragile" : "");
        MTexture source = GFX.Game[spritePath];
        
        int w = (int) (Width / 8f);
        int h = (int) (Height / 8f);
        textures = new MTexture[w, h];
        
        Calc.PushRandom(Position.GetHashCode());

        switch (axes)
        {
            case Axes.Horizontal:
                for (int j = 0; j < h; j++)
                {
                    // ensure horizontal consistency
                    int tyOffset = Calc.Random.Choose(0, 1);

                    for (int i = 0; i < w; i++)
                    {
                        int tx, ty;
                        int centerY = (int) MathF.Floor(h / 2f - 0.5f);

                        if (i == 0) tx = 0;
                        else if (i == w - 1) tx = 3;
                        else tx = Calc.Random.Choose(1, 2);
                
                        if (j == 0) ty = 0;
                        else if (j == h - 1) ty = 6;
                        else if (j < centerY) ty = 1 + tyOffset;
                        else if (j > centerY) ty = 4 + tyOffset;
                        else ty = 3;
                
                        textures[i, j] = source.GetSubtexture(tx * 8, ty * 8, 8, 8);
                    }
                }
                break;
            
            case Axes.Vertical:
            case Axes.Both:
                for (int i = 0; i < w; i++)
                {
                    // ensure vertical consistency
                    int txOffset = Calc.Random.Choose(0, 1);

                    for (int j = 0; j < h; j++)
                    {
                        int tx, ty;
                        int centerY = (int) MathF.Floor(h / 2f - 0.5f);

                        if (i == 0) tx = 0;
                        else if (i == w - 1) tx = 3;
                        else tx = 1 + txOffset;
                
                        if (j == 0) ty = 0;
                        else if (j == h - 1) ty = 6;
                        else if (j < centerY) ty = Calc.Random.Choose(1, 2);
                        else if (j > centerY) ty = Calc.Random.Choose(4, 5);
                        else ty = 3;
                
                        textures[i, j] = source.GetSubtexture(tx * 8, ty * 8, 8, 8);
                    }
                }
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Calc.PopRandom();

        string debrisPath = spriteDir + "/debris";
        debris = GFX.Game.GetAtlasSubtextures(debrisPath).ToArray();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Level level = SceneAs<Level>();
        if (doNotLoadFlag is not null && level.Session.GetFlag(doNotLoadFlag))
        {
            RemoveSelf();
            return;
        }

        if (flagOnBreak is not null && level.Session.GetFlag(flagOnBreak) && !broken)
            level.Session.SetFlag(flagOnBreak, false);
    }

    public override void Update()
    {
        base.Update();
        
        RescaleSpikes();

        hitCooldownTimer = Calc.Approach(hitCooldownTimer, 0f, Engine.DeltaTime);
        scale = Calc.Approach(scale, Vector2.One, Engine.DeltaTime / HitCooldownTime);
    }
    
    private void RescaleSpikes()
    {
        foreach (Spikes spikes in Spikes)
        {
            spikes.SetOrigins(Center);
            foreach (Component component in spikes.Components)
                if (component is Image image)
                {
                    image.Scale = scale;
                    image.Color = Color.White * alpha;
                }
        }
    }

    private DashCollisionResults OnDashCollision(Player player, Vector2 direction)
    { 
        if (player.StateMachine.State == Player.StRedDash)
            player.StateMachine.State = Player.StNormal;
        
        if (!CanActivate)
            return DashCollisionResults.NormalCollision;

        if (!SaveData.Instance.Assists.Invincible && player.CollideCheck<Spikes>())
            return DashCollisionResults.NormalCollision;

        // cornercorrection leniency for wallbounces
        bool shouldCornerCorrect = player.Left >= Right - Player.DashCornerCorrection
            || player.Right < Left + Player.DashCornerCorrection;
        if (direction.Y == -1f && player.DashDir.X == 0f && shouldCornerCorrect)
            return DashCollisionResults.NormalCollision;
        
        if (axes == Axes.Horizontal && direction.Abs() == Vector2.UnitX
            || axes == Axes.Vertical && direction.Abs() == Vector2.UnitY
            || axes == Axes.Both)
        {
            Activate(direction, true);
            return DashCollisionResults.Rebound;
        }

        return DashCollisionResults.NormalCollision;
    }

    private SoundWaveCollider.SoundWaveCollisionResults OnSoundWaveCollision(Vector2 direction)
    {
        if (!CanActivate)
            return SoundWaveCollider.SoundWaveCollisionResults.Destroy;
        
        Activate(direction, false);
        return SoundWaveCollider.SoundWaveCollisionResults.Destroy;
    }

    private void OnExplosionCollision(ExplosionCollider.ExplosionTypes _, Vector2 direction)
    {
        if (CanActivate)
            Activate(direction, false);
    }
    
    private static bool EasterEggCheck()
        => Calc.Random.Chance(aonHelperModule.Settings.TaikoDrumEasterEggChance);

    private void Activate(Vector2 direction, bool shake)
    {
        Level level = SceneAs<Level>();

        hitCooldownTimer = HitCooldownTime;
        
        string activateSound =
            (EasterEggCheck()
                ? Calc.Random.Choose(
                    aonHelperSFX.game_echoingthevalleysings_taiko_drum_vineboom,
                    aonHelperSFX.game_echoingthevalleysings_taiko_drum_waterphone)
                : aonHelperSFX.game_echoingthevalleysings_taiko_drum_activate)
            + (fragile ? "_fragile" : "");
        Vector2 activateSoundPosition = Center;
        
        if (direction == -Vector2.UnitX)
        {
            level.ParticlesFG.Emit(P_Activate, (int) (Height / 4f), CenterRight, Vector2.UnitY * Height / 2f, 0f);
            activateSoundPosition = CenterRight;
        }
        else if (direction == Vector2.UnitX)
        {
            level.ParticlesFG.Emit(P_Activate, (int) (Height / 4f), CenterLeft, Vector2.UnitY * Height / 2f, MathF.PI);
            activateSoundPosition = CenterLeft;
        }
        else if (direction == -Vector2.UnitY)
        {
            level.ParticlesFG.Emit(P_Activate, (int) (Width / 4f), BottomCenter, Vector2.UnitX * Width / 2f, MathF.PI / 2f);
            activateSoundPosition = BottomCenter;
        }
        else if (direction == Vector2.UnitY)
        {
            level.ParticlesFG.Emit(P_Activate, (int) (Width / 4f), TopCenter, Vector2.UnitX * Width / 2f, -MathF.PI / 2f);
            activateSoundPosition = TopCenter;
        }
        
        Audio.Play(activateSound, activateSoundPosition);

        if (shake)
        {
            level.DirectionalShake(direction);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
        }

        Vector2[] soundWaveDirections = axes switch {
            Axes.Horizontal => [-Vector2.UnitX, Vector2.UnitX],
            Axes.Vertical => [-Vector2.UnitY, Vector2.UnitY],
            Axes.Both => [-Vector2.UnitX, Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY],
            _ => throw new ArgumentOutOfRangeException()
        };
        foreach (Vector2 soundWaveDirection in soundWaveDirections)
        {
            SoundWave.Directions dir = soundWaveDirection.ToDirection();
            
            Rectangle collisionRect = dir switch {
                SoundWave.Directions.Left => new Rectangle((int) (Left - 1f), (int) Top, 1, (int) Height),
                SoundWave.Directions.Right => new Rectangle((int) Right, (int) Top, 1, (int) Height),
                SoundWave.Directions.Up => new Rectangle((int) Left, (int) (Top - 1f), (int) Width, 1),
                SoundWave.Directions.Down => new Rectangle((int) Left, (int) Bottom, (int) Width, 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (level.Tracker.GetEntities<Solid>().Any(s => s.CollideRect(collisionRect)))
                continue;
            
            Vector2 wavePos = dir switch {
                SoundWave.Directions.Left => CenterLeft - Vector2.UnitX * SoundWave.WaveWidth / 2f,
                SoundWave.Directions.Right => CenterRight + Vector2.UnitX * SoundWave.WaveWidth / 2f,
                SoundWave.Directions.Up => TopCenter - Vector2.UnitY * SoundWave.WaveWidth / 2f,
                SoundWave.Directions.Down => BottomCenter + Vector2.UnitY * SoundWave.WaveWidth / 2f,
                _ => throw new ArgumentOutOfRangeException()
            };
            float waveWidth = dir switch {
                SoundWave.Directions.Left or SoundWave.Directions.Right => Height,
                SoundWave.Directions.Up or SoundWave.Directions.Down => Width,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            level.Add(SoundWave.Create(level, wavePos, waveWidth, dir));
        }
        
        scale = new Vector2(
            1f + (MathF.Abs(direction.Y) * 0.5f - MathF.Abs(direction.X) * 0.5f) / scaleStrength.X,
            1f + (MathF.Abs(direction.X) * 0.5f - MathF.Abs(direction.Y) * 0.5f) / scaleStrength.Y);

        if (!fragile)
            return;

        Collidable = false;
        DisableStaticMovers();
        foreach (Spikes spikes in Spikes)
            spikes.Visible = true;

        broken = true;
        if (flagOnBreak is not null)
            level.Session.SetFlag(flagOnBreak);

        Vector2 from = Center - direction * 8f;
        for (int i = 0; i < Width / 8f; i++)
            for (int j = 0; j < Height / 8f; j++)
                level.Add(Engine.Pooler.Create<TaikoDrumDebris>()
                                .Init(Position + new Vector2(4f + i * 8f, 4f + j * 8f), Calc.Random.Choose(debris))
                                .BlastFrom(from));

        Tween fadeOutTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, HitCooldownTime, true);
        fadeOutTween.OnUpdate = t => alpha = 1f - t.Eased;
        fadeOutTween.OnComplete = _ => {
            DestroyStaticMovers();
            RemoveSelf();
        };
        Add(fadeOutTween);
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += Shake;

        for (int i = 0; i < textures.GetLength(0); i++)
            for (int j = 0; j < textures.GetLength(1); j++)
            {
                Vector2 renderPos = new Vector2(i, j) * 8f + Vector2.One * 4f + Position;
                renderPos = Center + (renderPos - Center) * scale;
                
                textures[i, j].DrawCentered(renderPos, Color.White * alpha, scale);
            }

        base.Render();
        
        Position = position;
    }
    
    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.FallingBlock.LandParticles += On_FallingBlock_LandParticles;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.FallingBlock.LandParticles -= On_FallingBlock_LandParticles;
    }

    private static void On_FallingBlock_LandParticles(On.Celeste.FallingBlock.orig_LandParticles orig, FallingBlock self)
    {
        orig(self);

        foreach (TaikoDrum drum in self
                .CollideAll<TaikoDrum>(self.Position + Vector2.UnitY)
                .Cast<TaikoDrum>()
                .Where(drum => drum.CanActivate))
            drum.Activate(Vector2.UnitY, false);
    }
    
    #endregion
}