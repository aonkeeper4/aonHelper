using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Entities;

// im sure this already exists somewhere but i couldn't find it
[CustomEntity(
    "aonHelper/UnforgivingSpikesUp = LoadUp",
    "aonHelper/UnforgivingSpikesDown = LoadDown",
    "aonHelper/UnforgivingSpikesLeft = LoadLeft",
    "aonHelper/UnforgivingSpikesRight = LoadRight"
)]
public class UnforgivingSpikes : Spikes
{
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Up);
    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Down);
    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Left);
    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Right);

    public UnforgivingSpikes(EntityData data, Vector2 offset, Directions dir)
        : base(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"))
    {
        Remove(Get<PlayerCollider>());
        Add(pc = new PlayerCollider(OnCollide));
    }

    private new void OnCollide(Player player)
    {
        switch (Direction)
        {
            case Directions.Up:
                player.Die(-Vector2.UnitY);
                break;
            case Directions.Down:
                player.Die(Vector2.UnitY);
                break;
            case Directions.Left:
                player.Die(-Vector2.UnitX);
                break;
            case Directions.Right:
                player.Die(Vector2.UnitX);
                break;
        }
    }
}