namespace Celeste.Mod.aonHelper.Components.Colliders;

[Tracked]
public class SoundWaveCollider(SoundWaveCollider.CollisionHandler onCollide, Collider collider = null)
    : ColliderComponent<SoundWaveCollider, SoundWaveCollider.CollisionHandler>(onCollide, collider)
{
    public enum SoundWaveCollisionResults
    {
        None,
        Reflect,
        Destroy,
        DestroyQuietly
    }
    public delegate SoundWaveCollisionResults CollisionHandler(Vector2 direction);

    public override Color Color => Color.Gold;
    
    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 += On_TouchSwitch_ctor_Vector2;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.TouchSwitch.ctor_Vector2 -= On_TouchSwitch_ctor_Vector2;
    }
    
    private static void On_TouchSwitch_ctor_Vector2(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position)
    {
        orig(self, position);
        
        self.Add(new SoundWaveCollider(_ => {
            self.TurnOn();
            return SoundWaveCollisionResults.None;
        }));
    }

    #endregion
}
