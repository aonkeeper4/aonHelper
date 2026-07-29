namespace Celeste.Mod.aonHelper.Components;

[Tracked]
public class SoundWaveCollider(Func<Vector2, SoundWaveCollider.SoundWaveCollisionResults> onCollide, Collider collider = null)
    : Component(false, false)
{
    public enum SoundWaveCollisionResults
    {
        None,
        Reflect,
        Destroy,
        DestroyQuietly
    }

    public readonly Func<Vector2, SoundWaveCollisionResults> OnCollide = onCollide;
    public Collider Collider => collider ?? Entity.Collider;
    
    public override void DebugRender(Camera camera)
        => Collider.Render(camera, Color.Gold * (Entity?.Collidable ?? false ? 1f : 0.5f));
    
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
