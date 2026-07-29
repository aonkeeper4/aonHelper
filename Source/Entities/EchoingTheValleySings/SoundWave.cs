using FMOD.Studio;

namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[Tracked]
public class SoundWave : Entity
{
	private readonly ParticleType P_Reflect;
	private readonly ParticleType P_Destroy;

    public enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }
    private Directions direction;
    private readonly float speed;
    private Vector2 movementCounter;

    public const float WaveWidth = 8f;
    private readonly Color color;
    
    // not a `SoundSource` because direct access to the instance is cleaner imo
    private EventInstance sfx;

    private const float FadeTime = 0.1f;
    private float percent;
    private bool destroyed;
    
    private SoundWave(
        Vector2 position, float width, Directions direction,
        float speed, int depth, Color color) : base(position)
    {
        Depth = depth;
        Collider = direction switch {
            Directions.Left or Directions.Right => new Hitbox(WaveWidth, width, -4f, -width / 2f),
            Directions.Up or Directions.Down => new Hitbox(width, WaveWidth, -width / 2f, -4f),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        this.direction = direction;
        this.speed = speed;
        Add(new Coroutine(Sequence()));
        
        this.color = color;
        Color particleColor = Color.Lerp(this.color, Color.White, 0.4f);
        P_Reflect = new ParticleType(CrushBlock.P_Impact) {
            Color = particleColor
        };
        P_Destroy = new ParticleType(Lightning.P_Shatter) {
            Color = particleColor,
            Color2 = particleColor * 0.5f
        };
        
        Add(new DisplacementRenderHook(OnRenderDisplacement));
    }

    public static SoundWave Create(Scene scene, Vector2 position, float width, Directions direction)
    {
        TaikoDrumController controller = scene.Tracker.GetEntity<TaikoDrumController>();

        float speed = controller?.SoundWaveSpeed ?? TaikoDrumController.DefaultSoundWaveSpeed;
        int depth = controller?.SoundWaveDepth ?? TaikoDrumController.DefaultSoundWaveDepth;
        Color color = controller?.SoundWaveColor ?? TaikoDrumController.DefaultSoundWaveColor;
        return new SoundWave(position, width, direction, speed, depth, color);
    }

    public override void Update()
    {
        base.Update();

        percent = Calc.Approach(percent, destroyed ? 0f : 1f, Engine.DeltaTime / FadeTime);
    }

    private IEnumerator Sequence()
    {
        sfx = Audio.Play(aonHelperSFX.game_echoingthevalleysings_sound_wave_travel, Position);
        Audio.SetParameter(sfx, "destroy", 0f);

        while (InBounds())
        {
            (bool shouldReflect, bool shouldDestroy, bool shouldDestroyQuietly) = Move(direction.ToVector() * speed * Engine.DeltaTime);

            if (shouldDestroy)
            {
                yield return DestroyRoutine(true);
                yield break;
            }
            if (shouldDestroyQuietly)
            {
                yield return DestroyRoutine(false);
                yield break;
            }
            if (shouldReflect)
                Reflect();
            
            yield return null;
        }

        yield return DestroyRoutine(false);
    }

    // hmm i think this logic is right
    // todo: probably refactor?
    private (bool, bool, bool) Move(Vector2 moveAmount)
    {
        Level level = SceneAs<Level>();
        
        movementCounter += moveAmount;
        float moveX = MathF.Round(movementCounter.X, MidpointRounding.ToEven);
        float moveY = MathF.Round(movementCounter.Y, MidpointRounding.ToEven);
        Vector2 moveVector = new(moveX, moveY);
        Vector2 moveDir = moveVector.Sign();
        
        if (moveVector == Vector2.Zero)
            return (false, false, false);
        
        movementCounter -= moveVector;
        
        while (moveVector != Vector2.Zero)
        {
            bool hasBeenReflected = false, hasBeenDestroyed = false, hasBeenDestroyedQuietly = false;
            foreach (SoundWaveCollider soundWaveCollider in level.Tracker
                .GetComponents<SoundWaveCollider>()
                .Cast<SoundWaveCollider>()
                .Where(c => Collidable && c.Entity.Collidable && ColliderCheck(c.Collider, Position + moveDir)))
            {
                SoundWaveCollider.SoundWaveCollisionResults result = soundWaveCollider.OnCollide?.Invoke(moveDir)
                    ?? SoundWaveCollider.SoundWaveCollisionResults.None;

                hasBeenReflected |= result is SoundWaveCollider.SoundWaveCollisionResults.Reflect;
                hasBeenDestroyed |= result is SoundWaveCollider.SoundWaveCollisionResults.Destroy;
                hasBeenDestroyedQuietly |= result is SoundWaveCollider.SoundWaveCollisionResults.DestroyQuietly;
            }

            if (hasBeenReflected || hasBeenDestroyed || hasBeenDestroyedQuietly)
                return (hasBeenReflected, hasBeenDestroyed, hasBeenDestroyedQuietly);

            moveVector -= moveDir;
            Position += moveDir;
            Audio.Position(sfx, Position);
        }

        return (false, false, false);
    }

    private bool ColliderCheck(Collider other, Vector2 at)
    {
        Vector2 position = Position;
        
        Position = at;
        bool result = Collider.Collide(other);
        Position = position;

        return result;
    }

    private bool InBounds()
    {
        const int boundsPadding = 16;
        
        Rectangle bounds = SceneAs<Level>().Bounds;
        bounds.Inflate(boundsPadding, boundsPadding);
        
        return bounds.Intersects(Collider.Bounds);
    }

    private void Reflect()
    {
        Level level = SceneAs<Level>();
        
        Audio.Play(aonHelperSFX.game_echoingthevalleysings_sound_wave_reflect, Position);

        switch (direction)
        {
            case Directions.Left:
                for (int i = 0; i < Height / 8f; i++)
                {
                    Vector2 particlePos = new(Left - 1f, Top + 4f + i * 8);
                    if (level.CollideCheck<Water>(particlePos) || !level.CollideCheck<Solid>(particlePos))
                        continue;

                    level.ParticlesFG.Emit(P_Reflect, particlePos, 0f);
                }
                break;

            case Directions.Right:
                for (int i = 0; i < Height / 8f; i++)
                {
                    Vector2 particlePos = new(Right + 1f, Top + 4f + i * 8);
                    if (level.CollideCheck<Water>(particlePos) || !level.CollideCheck<Solid>(particlePos))
                        continue;

                    level.ParticlesFG.Emit(P_Reflect, particlePos, MathF.PI);
                }
                break;
            
            case Directions.Up:
                for (int i = 0; i < Width / 8f; i++)
                {
                    Vector2 particlePos = new(Left + 4f + i * 8, Top - 1f);
                    if (level.CollideCheck<Water>(particlePos) || !level.CollideCheck<Solid>(particlePos))
                        continue;

                    level.ParticlesFG.Emit(P_Reflect, particlePos, -MathF.PI / 2f);
                }
                break;
            
            case Directions.Down:
                for (int i = 0; i < Width / 8f; i++)
                {
                    Vector2 particlePos = new(Left + 4f + i * 8, Bottom + 1f);
                    if (level.CollideCheck<Water>(particlePos) || !level.CollideCheck<Solid>(particlePos))
                        continue;

                    level.ParticlesFG.Emit(P_Reflect, particlePos, MathF.PI / 2f);
                }
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        direction = direction.Reverse();
    }

    private IEnumerator DestroyRoutine(bool withEffects)
    {
        Level level = SceneAs<Level>();
        
        Collidable = false;
        destroyed = true;

        if (withEffects)
        {
            Audio.SetParameter(sfx, "destroy", 1f);

            Vector2 breakPosition = direction switch {
                Directions.Left => CenterLeft,
                Directions.Right => CenterRight,
                Directions.Up => TopCenter,
                Directions.Down => BottomCenter,
                _ => throw new ArgumentOutOfRangeException()
            };
            Vector2 particleSpread = direction switch {
                Directions.Left or Directions.Right => Vector2.UnitY * Height / 2f,
                Directions.Up or Directions.Down => Vector2.UnitX * Width / 2f,
                _ => throw new ArgumentOutOfRangeException()
            };
            level.ParticlesFG.Emit(P_Destroy, 12, breakPosition, particleSpread);
            level.Displacement.AddBurst(breakPosition, FadeTime * 4f, 8f, 24f, 0.4f);
        }
        else
            Audio.Stop(sfx);

        while (percent > 0f)
        {
            Move(direction.ToVector() * speed * percent * Engine.DeltaTime);
            yield return null;
        }
        // allow the destroy sound to finish playing
        while (Audio.IsPlaying(sfx))
            yield return null;
        
        RemoveSelf();
    }

    public override void Render()
    {
        const float thick = 5f, thin = 2f;
        Color thickColor = color * percent;
        Color thinColor = color * percent * 0.4f;
        
        switch (direction)
        {
            case Directions.Left:
                Draw.Rect(TopLeft, thick, Height, thickColor);
                Draw.Rect(new Vector2(Right - thin, Top), thin, Height, thinColor);
                break;

            case Directions.Right:
                Draw.Rect(new Vector2(Right - thick, Top), thick, Height, thickColor);
                Draw.Rect(TopLeft, thin, Height, thinColor);
                break;
            
            case Directions.Up:
                Draw.Rect(TopLeft, Width, thick, thickColor);
                Draw.Rect(new Vector2(Left, Bottom - thin), Width, thin, thinColor);
                break;

            case Directions.Down:
                Draw.Rect(new Vector2(Left, Bottom - thick), Width, thick, thickColor);
                Draw.Rect(TopLeft, Width, thin, thinColor);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnRenderDisplacement()
    {
        const float maxDisplacementFactor = 0.3f;
        Color noDisplacement = new Color(0.5f, 0.5f, 0f) * percent;
        Color maxLeftDisplacement = new Color(0.5f - maxDisplacementFactor, 0.5f, 0f) * percent;
        Color maxRightDisplacement = new Color(0.5f + maxDisplacementFactor, 0.5f, 0f) * percent;
        Color maxUpDisplacement = new Color(0.5f, 0.5f - maxDisplacementFactor, 0f) * percent;
        Color maxDownDisplacement = new Color(0.5f, 0.5f + maxDisplacementFactor, 0f) * percent;
        
        switch (direction)
        {
            case Directions.Left:
                for (float i = 0f; i <= Width; i++)
                    Draw.Line(Right - i, Top, Right - i, Bottom, Color.Lerp(maxRightDisplacement, noDisplacement, i / Width) * (i / Width));
                break;

            case Directions.Right:
                for (float i = 0f; i <= Width; i++)
                    Draw.Line(Left + i, Top, Left + i, Bottom, Color.Lerp(maxLeftDisplacement, noDisplacement, i / Width) * (i / Width));
                break;
            
            case Directions.Up:
                for (float i = 0f; i <= Height; i++)
                    Draw.Line(Left, Bottom - i, Right, Bottom - i, Color.Lerp(maxDownDisplacement, noDisplacement, i / Height) * (i / Height));
                break;

            case Directions.Down:
                for (float i = 0f; i <= Height; i++)
                    Draw.Line(Left, Top + i, Right, Top + i, Color.Lerp(maxUpDisplacement, noDisplacement, i / Height) * (i / Height));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void Removed(Scene scene)
    {
        Audio.Stop(sfx);
        
        base.Removed(scene);
    }
    
    public override void SceneEnd(Scene scene)
    {
        Audio.Stop(sfx);
       
        base.SceneEnd(scene);
    }

    #region Hooks

    private class SoundWaveTriggerable() : Component(true, false)
    {
        private Solid Solid => EntityAs<Solid>()!;

        private enum TriggerState
        {
            Waiting,
            Moving,
        }
        private TriggerState state = TriggerState.Waiting;
        
        public bool Triggered { get; private set; }
        public bool PreviouslyTriggered { get; private set; }
        
        private Vector2? triggerPosition;

        public override void Added(Entity entity)
        {
            // ewww namespacing
            if (entity is not global::Celeste.Solid)
                throw new Exception($"{nameof(SoundWaveTriggerable)} added to non-{nameof(global::Celeste.Solid)} entity!");
            
            base.Added(entity);
        }

        public override void Update()
        {
            switch (state)
            {
                case TriggerState.Waiting:
                    if (triggerPosition is not null && Solid.Position != triggerPosition)
                        state = TriggerState.Moving;
                    
                    break;
                
                case TriggerState.Moving:
                    if (Solid.Position == triggerPosition)
                    {
                        state = TriggerState.Waiting;
                        
                        Triggered = false;
                        triggerPosition = null;
                    }

                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PreviouslyTriggered = Triggered;
        }
        
        public void Trigger()
        {
            if (Triggered)
                return;
            
            Triggered = true;
            triggerPosition = Solid.Position;
        }
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Solid.Awake += On_Solid_Awake;
        On.Celeste.Solid.GetPlayerClimbing += On_Solid_GetPlayerClimbing;
        On.Celeste.Solid.GetPlayerOnTop += On_Solid_GetPlayerOnTop;
        On.Celeste.Solid.GetPlayerRider += On_Solid_GetPlayerRider;

        IL.Celeste.Spring.BounceAnimate += IL_Spring_BounceAnimate;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Solid.Awake -= On_Solid_Awake;
        On.Celeste.Solid.GetPlayerClimbing -= On_Solid_GetPlayerClimbing;
        On.Celeste.Solid.GetPlayerOnTop -= On_Solid_GetPlayerOnTop;
        On.Celeste.Solid.GetPlayerRider -= On_Solid_GetPlayerRider;
        
        IL.Celeste.Spring.BounceAnimate -= IL_Spring_BounceAnimate;
    }
    
    private static void On_Solid_Awake(On.Celeste.Solid.orig_Awake orig, Solid self, Scene scene)
    {
        orig(self, scene);
        
        TaikoDrumController controller = self.Scene.Tracker.GetEntity<TaikoDrumController>();
        if (controller is null)
            return;

        if (controller.AffectAll || controller.AffectedTypes.Contains(self.GetType()))
            self.AddAt(new SoundWaveTriggerable(), 0); // fixes activating a frame late due to component update order
    }

    private static Player On_Solid_GetPlayerClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Solid self) => SoundWaveActivationCheck(() => orig(self), self);
    private static Player On_Solid_GetPlayerOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self) => SoundWaveActivationCheck(() => orig(self), self);
    private static Player On_Solid_GetPlayerRider(On.Celeste.Solid.orig_GetPlayerRider orig, Solid self) => SoundWaveActivationCheck(() => orig(self), self);

    private static Player SoundWaveActivationCheck(Func<Player> callOrig, Solid self)
    {
        if (self.Get<SoundWaveTriggerable>() is not { } triggerable)
            return callOrig();

        if (self.CollideCheck<SoundWave>())
            triggerable.Trigger();

        return (triggerable.PreviouslyTriggered, triggerable.Triggered) switch {
            (_, false) => null,
            (false, true) => self.Scene.Tracker.GetEntity<Player>(),
            (true, true) => callOrig()
        };
    }
    
    private static void IL_Spring_BounceAnimate(ILContext il)
    {
        ILCursor cursor = new(il);
        
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Spring>("staticMover"),
            instr => instr.MatchCallvirt<StaticMover>("TriggerPlatform")))
            throw new HookHelper.HookException(il, "Unable to find call to `StaticMover.TriggerPlatform`.");

        ILLabel afterPlatformActivation = cursor.DefineLabel();
        cursor.EmitLdarg0();
        cursor.EmitDelegate(SkipPlatformActivation);
        cursor.EmitBrtrue(afterPlatformActivation);
        
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<StaticMover>("TriggerPlatform"));
        cursor.MarkLabel(afterPlatformActivation);

        return;

        static bool SkipPlatformActivation(Spring spring)
            => spring.staticMover.Platform?.Get<SoundWaveTriggerable>() is not null;
    }
    
    #endregion
}

public static class SoundWaveDirectionsExtensions
{
    public static Vector2 ToVector(this SoundWave.Directions direction)
        => direction switch {
            SoundWave.Directions.Left => -Vector2.UnitX,
            SoundWave.Directions.Right => Vector2.UnitX,
            SoundWave.Directions.Up => -Vector2.UnitY,
            SoundWave.Directions.Down => Vector2.UnitY,
            _ => throw new ArgumentOutOfRangeException()
        };
    
    public static SoundWave.Directions Reverse(this SoundWave.Directions direction)
        => direction switch {
            SoundWave.Directions.Left => SoundWave.Directions.Right,
            SoundWave.Directions.Right => SoundWave.Directions.Left,
            SoundWave.Directions.Up => SoundWave.Directions.Down,
            SoundWave.Directions.Down => SoundWave.Directions.Up,
            _ => throw new ArgumentOutOfRangeException()
        };

    public static SoundWave.Directions ToDirection(this Vector2 direction)
    {
        if (direction == -Vector2.UnitX) return SoundWave.Directions.Left;
        if (direction == Vector2.UnitX) return SoundWave.Directions.Right;
        if (direction == -Vector2.UnitY) return SoundWave.Directions.Up;
        if (direction == Vector2.UnitY) return SoundWave.Directions.Down;

        throw new ArgumentOutOfRangeException();
    }
}
