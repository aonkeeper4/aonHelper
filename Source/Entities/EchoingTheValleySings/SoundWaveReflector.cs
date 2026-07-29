namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[CustomEntity("aonHelper/SoundWaveReflector")]
public class SoundWaveReflector : Entity
{
    public enum Orientations
    {
        Left,
        Right,
        Up,
        Down
    }
    private readonly Orientations orientation;

    private readonly StaticMover staticMover;
    private Vector2 shake;

    private readonly MTexture[] tiles = new MTexture[3];
    
    public SoundWaveReflector(Vector2 position, float width, float height, Orientations orientation, string spriteDir)
        : base(position)
    {
        Depth = Depths.FakeWalls;
        this.orientation = orientation;

        Collider = this.orientation switch
        {
            Orientations.Up or Orientations.Down => new Hitbox(width, 8f),
            Orientations.Left or Orientations.Right => new Hitbox(8f, height),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        BuildSprite(string.IsNullOrEmpty(spriteDir) ? "objects/aonHelper/soundWaveReflector" : spriteDir);

        Add(staticMover = new StaticMover
        {
            OnAttach = p => Depth = p.Depth - 1,
            OnShake = v => shake += v,
            SolidChecker = IsRiding,
            OnEnable = () => Active = Visible = Collidable = true,
            OnDisable = () => Active = Visible = Collidable = false,
            OnDestroy = RemoveSelf
        });
        Add(new SoundWaveCollider(OnSoundWaveCollide));
    }
    
    public SoundWaveReflector(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Enum<Orientations>("orientation"), data.Attr("spriteDir"))
    { }

    private void BuildSprite(string spriteDir)
    {
        string spritePath = spriteDir + orientation switch {
            Orientations.Left => "/left",
            Orientations.Right => "/right",
            Orientations.Up => "/up",
            Orientations.Down => "/down",
            _ => throw new ArgumentOutOfRangeException()
        };
        MTexture source = GFX.Game[spritePath];
        
        for (int i = 0; i < 3; i++)
            tiles[i] = source.GetSubtexture(i * 8, 0, 8, 8);
    }

    // make sure at least one side aligns, and the rest are contained within the solid
    private bool IsRiding(Solid solid)
        => orientation switch
        {
            Orientations.Up => CollideCheckOutsideInside(solid, TopCenter - Vector2.UnitY * Height),
            Orientations.Down => CollideCheckOutsideInside(solid, BottomCenter + Vector2.UnitY),
            Orientations.Left => CollideCheckOutsideInside(solid, CenterLeft - Vector2.UnitX * Width),
            Orientations.Right => CollideCheckOutsideInside(solid, CenterRight + Vector2.UnitX),
            _ => throw new ArgumentOutOfRangeException()
        };

    private bool CollideCheckOutsideInside(Entity other, Vector2 at)
        => CollideCheck(other) && !CollideCheck(other, at);

    private SoundWaveCollider.SoundWaveCollisionResults OnSoundWaveCollide(Vector2 direction)
    {
        if (direction != orientation switch {
                Orientations.Left => Vector2.UnitX,
                Orientations.Right => -Vector2.UnitX,
                Orientations.Up => Vector2.UnitY,
                Orientations.Down => -Vector2.UnitY,
                _ => throw new ArgumentOutOfRangeException()
            })
            return SoundWaveCollider.SoundWaveCollisionResults.None;
        
        staticMover.TriggerPlatform();
        return SoundWaveCollider.SoundWaveCollisionResults.Reflect;
    }

    public override void Update()
    {
        if (staticMover.Platform is null)
        {
            RemoveSelf();
            return;
        }

        base.Update();
    }

    public override void Render()
    {
        switch (orientation)
        {
            case Orientations.Left:
            case Orientations.Right:
                int h = (int) (Height / 8f);
                for (int y = 0; y < h; y++)
                {
                    int tx = y == 0
                        ? 0
                        : y == h - 1
                            ? 2
                            : 1;
                    tiles[tx].Draw(Position + shake + Vector2.UnitY * y * 8f);
                }
                
                break;
            
            case Orientations.Up:
            case Orientations.Down:
                int w = (int) (Width / 8f);
                for (int x = 0; x < w; x++)
                {
                    int ty = x == 0
                        ? 0
                        : x == w - 1
                            ? 2
                            : 1;
                    tiles[ty].Draw(Position + shake + Vector2.UnitX * x * 8f);
                }
                
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
