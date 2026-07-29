using Celeste.Mod.aonHelper.Components.Colliders;

namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[CustomEntity("aonHelper/SoundWaveBarrier")]
public class SoundWaveBarrier : Entity
{
    public SoundWaveBarrier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);

        string flag = data.Attr("flag");
        Add(new SoundWaveCollider(_ =>
            string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag)
                ? SoundWaveCollider.SoundWaveCollisionResults.DestroyQuietly
                : SoundWaveCollider.SoundWaveCollisionResults.None));
        
        if (data.Bool("attachToSolids"))
            Add(new StaticMover {
                SolidChecker = CollideCheck,
                JumpThruChecker = CollideCheck
            });
    }
}
