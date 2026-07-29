namespace Celeste.Mod.aonHelper.Entities.Misc;

[CustomEntity("aonHelper/IceZipMover")]
public class IceZipMover : Solid
{
	private class IceZipMoverPathRenderer : Entity
	{
		private readonly IceZipMover iceZipMover;

		private readonly MTexture cog;

		private readonly Vector2 from;
		private readonly Vector2 to;

		private readonly Vector2 sparkAdd;

		private readonly float sparkDirFromA;
		private readonly float sparkDirFromB;
		private readonly float sparkDirToA;
		private readonly float sparkDirToB;

		private readonly Shaker shaker;
		private Vector2 Shake => (shaker.Value / 2f).Floor();

		public IceZipMoverPathRenderer(IceZipMover iceZipMover)
		{
			Depth = 5000;
			
			this.iceZipMover = iceZipMover;
			
			from = this.iceZipMover.start + new Vector2(this.iceZipMover.Width / 2f, this.iceZipMover.Height / 2f);
			to = this.iceZipMover.target + new Vector2(this.iceZipMover.Width / 2f, this.iceZipMover.Height / 2f);
			
			sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
			
			float num = (from - to).Angle();
			sparkDirFromA = num + MathF.PI / 8f;
			sparkDirFromB = num - MathF.PI / 8f;
			sparkDirToA = num + MathF.PI - MathF.PI / 8f;
			sparkDirToB = num + MathF.PI + MathF.PI / 8f;
			
			cog = GFX.Game[this.iceZipMover.spriteDir + "/cog"];

			Add(shaker = new Shaker(false));
		}

		public void CreateSparks()
		{
			ParticleSystem particles = SceneAs<Level>().ParticlesBG;

			Vector2 from = this.from + Shake;
			Vector2 to = this.to + Shake;
			particles.Emit(iceZipMover.P_Sparks, from + sparkAdd + RandomVector(), sparkDirFromA);
			particles.Emit(iceZipMover.P_Sparks, from - sparkAdd + RandomVector(), sparkDirFromB);
			particles.Emit(iceZipMover.P_Sparks, to + sparkAdd + RandomVector(), sparkDirToA);
			particles.Emit(iceZipMover.P_Sparks, to - sparkAdd + RandomVector(), sparkDirToB);

			return;
			
			static Vector2 RandomVector()
				=> Calc.Random.Range(-Vector2.One, Vector2.One);
		}

		public override void Render()
		{
			DrawCogs(Vector2.UnitY, Color.Black);
			DrawCogs(Vector2.Zero);
		}

		private void DrawCogs(Vector2 offset, Color? colorOverride = null)
		{
			Vector2 from = this.from + Shake;
			Vector2 to = this.to + Shake;
			
			Vector2 dir = (to - from).SafeNormalize();
			Vector2 lineAOffset = dir.Perpendicular() * 3f;
			Vector2 lineBOffset = -dir.Perpendicular() * 4f;
			
			Draw.Line(from + lineAOffset + offset, to + lineAOffset + offset, colorOverride ?? iceZipMover.ropeColor);
			Draw.Line(from + lineBOffset + offset, to + lineBOffset + offset, colorOverride ?? iceZipMover.ropeColor);
			
			for (float distAlongTrack = 4f - iceZipMover.percent * MathF.PI * 8f % 4f; distAlongTrack < (to - from).Length(); distAlongTrack += 4f)
			{
				Vector2 toothStart = from + lineAOffset + dir.Perpendicular() + dir * distAlongTrack;
				Vector2 toothEnd = to + lineBOffset - dir * distAlongTrack;
				Draw.Line(toothStart + offset, toothStart + dir * 2f + offset, colorOverride ?? iceZipMover.ropeLightColor);
				Draw.Line(toothEnd + offset, toothEnd - dir * 2f + offset, colorOverride ?? iceZipMover.ropeLightColor);
			}
			
			float cogRotation = iceZipMover.percent * MathF.PI * 2f;
			cog.DrawCentered(from + offset, colorOverride ?? Color.White, 1f, cogRotation);
			cog.DrawCentered(to + offset, colorOverride ?? Color.White, 1f, cogRotation);
		}

		public void StartShaking(float time)
			=> shaker.ShakeFor(time, false);
	}

	[Pooled]
	private class BreakDebris : Entity
	{
		private Image sprite;

		private Vector2 speed;

		private float percent;
		private float duration;

		public BreakDebris Init(Vector2 position, Vector2 direction, string textures, float additionalSpeed)
		{
			MTexture texture = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(textures));
			
			if (sprite == null)
			{
				Add(sprite = new Image(texture));
				sprite.CenterOrigin();
			}
			else
				sprite.Texture = texture;
			
			Position = position;
			
			direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
			speed = direction * (Calc.Random.Range(20f, 40f) + additionalSpeed);
			
			percent = 0f;
			duration = Calc.Random.Range(2f, 3f);
			
			return this;
		}

		public override void Update()
		{
			base.Update();
			
			if (percent >= 1f)
			{
				RemoveSelf();
				return;
			}
			
			Position += speed * Engine.DeltaTime;
			
			speed.X = Calc.Approach(speed.X, 0f, 180f * Engine.DeltaTime);
			speed.Y += 200f * Engine.DeltaTime;
			
			percent += Engine.DeltaTime / duration;
			sprite.Color = Color.White * (1f - percent);
		}

		public override void Render()
		{
			sprite.DrawOutline(Color.Black);
			
			base.Render();
		}
	}
	
	[Pooled]
	private class RespawnDebris : Entity
	{
		private Image sprite;

		private Vector2 from;
		private Vector2 to;

		private float percent;
		private float duration;

		public RespawnDebris Init(Vector2 from, Vector2 to, string textures, float duration)
		{
			MTexture texture = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(textures));
			
			if (sprite == null)
			{
				Add(sprite = new Image(texture));
				sprite.CenterOrigin();
			}
			else
				sprite.Texture = texture;
			
			Position = this.from = from;
			this.to = to;
			
			percent = 0f;
			this.duration = duration;
			
			return this;
		}

		public override void Update()
		{
			base.Update();
			
			if (percent > 1f)
			{
				RemoveSelf();
				return;
			}
			
			percent += Engine.DeltaTime / duration;
			Position = Vector2.Lerp(from, to, Ease.CubeIn(percent));
			sprite.Color = Color.White * percent;
		}

		public override void Render()
		{
			sprite.DrawOutline(Color.Black);
			
			base.Render();
		}
	}

	private readonly ParticleType P_Scrape, P_Sparks, P_Break, P_Reform;

	private readonly string spriteDir;

	private IceZipMoverPathRenderer pathRenderer;
	private readonly Color ropeColor, ropeLightColor;

	private readonly Vector2 start, target;
	private float percent;

	private readonly SoundSource sfx;
	
	private bool broken, reformed = true;
	private float respawnFlash;

	private readonly string moveSfx, breakSfx, respawnSfx;

	private readonly bool breakEarly;
	
	public IceZipMover(Vector2 position, int width, int height,
		Vector2 target, bool breakEarly,
		string spriteDir, Color ropeColor, Color ropeLightColor,
		Color sparkParticleColor, Color breakParticleColor, Color breakParticleFadeColor,
		int surfaceIndex, string moveSfx, string breakSfx, string respawnSfx)
		: base(position, width, height, safe: false)
	{
		Depth = -9000;
		SurfaceSoundIndex = surfaceIndex;
		
		start = Position;
		this.target = target;

		this.breakEarly = breakEarly;
		
		this.spriteDir = string.IsNullOrEmpty(spriteDir) ? "objects/aonHelper/iceZipMover" : spriteDir;
		BuildImages(GFX.Game[this.spriteDir + "/block"]);
		BuildSprite(this.spriteDir + "/center");
		this.ropeColor = ropeColor;
		this.ropeLightColor = ropeLightColor;

		P_Scrape = new ParticleType(ZipMover.P_Scrape);
		P_Sparks = new ParticleType(ZipMover.P_Sparks) { Color = sparkParticleColor };
		P_Break = new ParticleType(BounceBlock.P_IceBreak) { Color = breakParticleColor, Color2 = breakParticleFadeColor };
		P_Reform = new ParticleType(BounceBlock.P_Reform);
		
		Add(sfx = new SoundSource { Position = new Vector2(Width, Height) / 2f });
		this.moveSfx = SFX.EventnameByHandle(moveSfx);
		this.breakSfx = SFX.EventnameByHandle(breakSfx);
		this.respawnSfx = SFX.EventnameByHandle(respawnSfx);
		
		Add(new Coroutine(Sequence()));
		Add(new LightOcclude());
	}

	public IceZipMover(EntityData data, Vector2 offset)
		: this(data.Position + offset, data.Width, data.Height,
			data.Nodes[0] + offset, data.Bool("breakEarly", false),
			data.Attr("spriteDir", ""), data.HexColor("ropeColor", Calc.HexToColor("663931")), data.HexColor("ropeLightColor", Calc.HexToColor("9b6157")),
			data.HexColor("sparkParticleColor", Calc.HexToColor("fff538")), data.HexColor("breakParticleColor", Calc.HexToColor("33ffe7")), data.HexColor("breakParticleFadeColor", Calc.HexToColor("0151d0")),
			data.Int("surfaceIndex", 8), data.Attr("moveSfx", SFX.game_01_zipmover), data.Attr("breakSfx", SFX.game_09_iceblock_touch), data.Attr("respawnSfx", SFX.game_09_iceblock_reappear))
	{ }
	
	private void BuildImages(MTexture source)
	{
		int tilesX = source.Width / 8;
		int tilesY = source.Height / 8;
		for (int i = 0; i < Width; i += 8)
		{
			for (int j = 0; j < Height; j += 8)
			{
				int imageX = i != 0 ? i < Width - 8f ? Calc.Random.Next(1, tilesX - 1) : tilesX - 1 : 0;
				int imageY = j != 0 ? j < Height - 8f ? Calc.Random.Next(1, tilesY - 1) : tilesY - 1 : 0;

				Image image = new(source.GetSubtexture(imageX * 8, imageY * 8, 8, 8)) { Position = new Vector2(i, j) };
				Add(image);
			}
		}
	}

	private void BuildSprite(string spriteDir)
	{
		Sprite sprite = new(GFX.Game, spriteDir);
		
		sprite.AddLoop("idle", "", 0.1f);
		
		sprite.CenterOrigin();
		sprite.Play("idle");
		sprite.Position = new Vector2(Width, Height) / 2f;
		
		Add(sprite);
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		
		scene.Add(pathRenderer = new IceZipMoverPathRenderer(this));
	}

	public override void Removed(Scene scene)
	{
		scene.Remove(pathRenderer);
		pathRenderer = null;
		
		base.Removed(scene);
	}

	public override void Update()
	{
		base.Update();
		
		respawnFlash = Calc.Approach(respawnFlash, 0f, Engine.DeltaTime * 8f);
	}

	public override void Render()
	{
		Vector2 position = Position;
		Position += Shake;
		
		if (!broken && reformed)
			base.Render();
		
		if (respawnFlash > 0f)
		{
			float flash = Ease.CubeOut(respawnFlash);
			float flashRectOffset = flash * 2f;
			Draw.Rect(X - flashRectOffset, Y - flashRectOffset, Width + flashRectOffset * 2f, Height + flashRectOffset * 2f, Color.White * flash);
		}
		
		Position = position;
	}

	private void ScrapeParticlesCheck(Vector2 to)
	{
		if (!Scene.OnInterval(0.03f))
			return;
		
		bool atFinalX = ExactPosition.X == to.X;
		bool atFinalY = ExactPosition.Y == to.Y;
		if (atFinalX && !atFinalY)
		{
			int checkDir = Math.Sign(to.Y - ExactPosition.Y);
			Vector2 checkFrom = checkDir == 1 ? BottomLeft : TopLeft;
			
			int particleStartOffset = 4;
			if (checkDir == 1)
				particleStartOffset = Math.Min((int) Height - 12, 20);
			
			int particleEndOffset = (int) Height;
			if (checkDir == -1)
				particleEndOffset = Math.Max(16, (int) Height - 16);
			
			if (Scene.CollideCheck<Solid>(checkFrom + new Vector2(-2f, checkDir * -2f)))
				for (int y = particleStartOffset; y < particleEndOffset; y += 8)
					SceneAs<Level>().ParticlesFG.Emit(P_Scrape, TopLeft + new Vector2(0f, y + checkDir * 2f), checkDir == 1 ? -MathF.PI / 4f : MathF.PI / 4f);
			
			if (Scene.CollideCheck<Solid>(checkFrom + new Vector2(Width + 2f, checkDir * -2f)))
				for (int y = particleStartOffset; y < particleEndOffset; y += 8)
					SceneAs<Level>().ParticlesFG.Emit(P_Scrape, TopRight + new Vector2(-1f, y + checkDir * 2f), checkDir == 1 ? MathF.PI * -3f / 4f : MathF.PI * 3f / 4f);
		}
		else if (!atFinalX && atFinalY)
		{
			int checkDir = Math.Sign(to.X - ExactPosition.X);
			Vector2 checkFrom = checkDir == 1 ? TopRight : TopLeft;
			
			int particleStartOffset = 4;
			if (checkDir == 1)
				particleStartOffset = Math.Min((int) Width - 12, 20);
			
			int particleEndOffset = (int) Width;
			if (checkDir == -1)
				particleEndOffset = Math.Max(16, (int) Width - 16);
			
			if (Scene.CollideCheck<Solid>(checkFrom + new Vector2(checkDir * -2f, -2f)))
				for (int x = particleStartOffset; x < particleEndOffset; x += 8)
					SceneAs<Level>().ParticlesFG.Emit(P_Scrape, TopLeft + new Vector2(x + checkDir * 2f, -1f), checkDir == 1 ? MathF.PI * 3f / 4f : MathF.PI / 4f);
			
			if (Scene.CollideCheck<Solid>(checkFrom + new Vector2(checkDir * -2f, Height + 2f)))
				for (int x = particleStartOffset; x < particleEndOffset; x += 8)
					SceneAs<Level>().ParticlesFG.Emit(P_Scrape, BottomLeft + new Vector2(x + checkDir * 2f, 0f), checkDir == 1 ? MathF.PI * -3f / 4f : -MathF.PI / 4f);
		}
	}

	private IEnumerator Sequence()
	{
		while (true)
		{
			if (!HasPlayerRider())
			{
				yield return null;
				continue;
			}
			
			sfx.Play(moveSfx);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			StartShaking(0.1f);
			yield return 0.1f;
			
			StopPlayerRunIntoAnimation = false;
			Vector2 oldPos = ExactPosition, newPos = ExactPosition;
			float t = 0f;
			while (t < 1f)
			{
				yield return null;
				oldPos = newPos;
				
				t = Calc.Approach(t, 1f, 2f * Engine.DeltaTime);
				percent = Ease.SineIn(t);
				newPos = Vector2.Lerp(start, target, percent);
				
				ScrapeParticlesCheck(newPos);
				if (Scene.OnInterval(0.1f))
					pathRenderer.CreateSparks();
				
				if (percent > 0.6f && breakEarly)
				{
					StartShaking(0.1f);
					pathRenderer.StartShaking(0.1f);
				}
				
				MoveTo(newPos);
			}

			StartShaking(0.2f);
			pathRenderer.StartShaking(0.2f);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			SceneAs<Level>().Shake();
			
			if (breakEarly)
			{
				yield return null;
				
				sfx.Stop();
				sfx.Play(breakSfx);

				Vector2 dir = (target - start).SafeNormalize();
				float speed = ((ExactPosition - oldPos) / Engine.DeltaTime)
                    .Clamp(-Player.LiftXCap, Player.LiftYCap, Player.LiftXCap, 0f).Length(); // clamp to liftboost cap
				Break(dir, speed);
			}
			else
			{
				StopPlayerRunIntoAnimation = true;
				yield return 0.2f;

				sfx.Stop();
				sfx.Play(breakSfx);
				
				Vector2 startPos = ExactPosition;
				Vector2 targetPos = startPos + Vector2.UnitY * 16f;
				float moveSpeed = 0f;
				while (Vector2.DistanceSquared(ExactPosition, targetPos) > 2f)
				{
					yield return null;

					moveSpeed = Calc.Approach(moveSpeed, 35f, 600f * Engine.DeltaTime);

					Vector2 pos = Calc.Approach(ExactPosition, targetPos, moveSpeed * Engine.DeltaTime / 3f);
					Vector2 liftSpeed = (pos - ExactPosition).SafeNormalize(moveSpeed / 3f);
					liftSpeed.X *= 0.75f;
					MoveTo(pos, liftSpeed);

					if (Vector2.DistanceSquared(ExactPosition, targetPos) <= 12f)
						StartShaking(0.1f);
				}

				Break();
			}

			Depth = 8990;
			MoveToNaive(start);
			reformed = false;
			yield return 1.6f;
			while (CollideCheck<Actor>() || CollideCheck<Solid>())
				yield return null;
			
			sfx.Play(respawnSfx);
			for (int x = 0; x < Width; x += 8)
			{
				for (int y = 0; y < Height; y += 8)
				{
					Vector2 vector6 = new(X + x + 4f, Y + y + 4f);
					Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector6 + (vector6 - Center).SafeNormalize() * 12f, vector6, spriteDir + "/debris", 0.35f));
				}
			}
			Depth = -9000;
			Collidable = true;
			broken = false;
			yield return 0.35f;
			
			EnableStaticMovers();
			ReformParticles();
			reformed = true;
			respawnFlash = 0.6f;
		}
	}

	private void Break(Vector2? direction = null, float additionalSpeed = 0f)
	{
		Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
		
		Collidable = false;
		DisableStaticMovers();
		broken = true;

		for (int x = 0; x < Width; x += 8)
		{
			for (int y = 0; y < Height; y += 8)
			{
				Vector2 pos = new(X + x + 4f, Y + y + 4f);
				Vector2 dir = direction ?? (pos - Center).SafeNormalize();
				Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(pos, dir, spriteDir + "/debris", additionalSpeed));
			}
		}

		Level level = SceneAs<Level>();
		for (int x = 0; x < Width; x += 4)
		{
			for (int y = 0; y < Height; y += 4)
			{
				Vector2 pos = Position + new Vector2(2 + x, 2 + y) + Calc.Random.Range(-Vector2.One, Vector2.One);
				Vector2 effectivePos = pos + (direction ?? Vector2.Zero) * additionalSpeed * 0.2f;
				float angle = (effectivePos - Center).Angle();
				level.Particles.Emit(P_Break, pos, angle);
			}
		}
	}
	
	private void ReformParticles()
	{
		Level level = SceneAs<Level>();
		
		for (int x = 0; x < Width; x += 4)
		{
			level.Particles.Emit(P_Reform, new Vector2(X + 2f + x + Calc.Random.Range(-1, 1), Y), -MathF.PI / 2f);
			level.Particles.Emit(P_Reform, new Vector2(X + 2f + x + Calc.Random.Range(-1, 1), Bottom - 1f), MathF.PI / 2f);
		}
		for (int y = 0; y < Height; y += 4)
		{
			level.Particles.Emit(P_Reform, new Vector2(X, Y + 2f + y + Calc.Random.Range(-1, 1)), MathF.PI);
			level.Particles.Emit(P_Reform, new Vector2(Right - 1f, Y + 2f + y + Calc.Random.Range(-1, 1)), 0f);
		}
	}
}
