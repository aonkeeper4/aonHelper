using Celeste.Mod.aonHelper.Components.Colliders;

namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[CustomEntity("aonHelper/SoundWaveBarrier")]
public class SoundWaveBarrier : Entity
{
    public SoundWaveBarrier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);

        ConditionHelper.Condition condition = data.Condition("flag");
        Add(new SoundWaveCollider(_ =>
            condition.Check(SceneAs<Level>())
                ? SoundWaveCollider.SoundWaveCollisionResults.DestroyQuietly
                : SoundWaveCollider.SoundWaveCollisionResults.None));
        
        if (data.Bool("attachToSolids"))
            Add(new StaticMover {
                SolidChecker = CollideCheck,
                JumpThruChecker = CollideCheck
            });
    }
}
