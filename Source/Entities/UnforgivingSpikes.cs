namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity(
    $"{EntitySIDPrefix}Up = LoadUp",
    $"{EntitySIDPrefix}Down = LoadDown",
    $"{EntitySIDPrefix}Left = LoadLeft",
    $"{EntitySIDPrefix}Right = LoadRight"
)]
[Tracked]
[TrackedAs(typeof(Spikes))]
public class UnforgivingSpikes : Spikes
{
    public const string EntitySIDPrefix = "aonHelper/UnforgivingSpikes";
    
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Up);
    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Down);
    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Left);
    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Right);

    private readonly bool checkMoveDirection;

    public UnforgivingSpikes(EntityData data, Vector2 offset, Directions dir)
        : base(data, offset, dir)
    {
        checkMoveDirection = data.Bool("checkMoveDirection");
        
        Remove(Get<PlayerCollider>());
        Add(new UnforgivingPlayerCollider(OnCollide));
    }

    private bool OnCollide(Player player, Vector2 moveDir)
    {
        switch (Direction)
        {
            case Directions.Up when !checkMoveDirection || moveDir.Y > 0f:
                player.Die(-Vector2.UnitY);
                return true;
            
            case Directions.Down when !checkMoveDirection || moveDir.Y < 0f:
                player.Die(Vector2.UnitY);
                return true;
            
            case Directions.Left when !checkMoveDirection || moveDir.X > 0f:
                player.Die(-Vector2.UnitX);
                return true;
            
            case Directions.Right when !checkMoveDirection || moveDir.X < 0f:
                player.Die(Vector2.UnitX);
                return true;
            
            default:
                return false;
        }
    }
}