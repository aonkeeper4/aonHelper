namespace Celeste.Mod.aonHelper.Components.Colliders;

[Tracked]
public class ExplosionCollider(Action<ExplosionCollider.ExplosionTypes, Vector2> onCollide, Collider collider = null)
    : Component(false, false)
{
    public enum ExplosionTypes
    {
        Puffer,
        Seeker
    }

    public readonly Action<ExplosionTypes, Vector2> OnCollide = onCollide;
    public Collider Collider => collider ?? Entity.Collider;

    public bool Check(Entity other)
        => Entity.Collidable && other.Collidable && Collider.Collide(other);

    public override void DebugRender(Camera camera)
        => Collider.Render(camera, Color.Orange * (Entity?.Collidable ?? false ? 1f : 0.5f));
    
    #region Hooks

    private static ILHook il_Seeker_RegenerateCoroutine;

    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.Puffer.Explode += IL_Puffer_Explode;

        il_Seeker_RegenerateCoroutine = new ILHook(typeof(Seeker).GetMethod("RegenerateCoroutine", HookHelper.Bind.NonPublicInstance)!.GetStateMachineTarget()!, Seeker_RegenerateCoroutine);
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.Puffer.Explode -= IL_Puffer_Explode;
        
        HookHelper.DisposeAndSetNull(ref il_Seeker_RegenerateCoroutine);
    }

    private static void IL_Puffer_Explode(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdloc0(),
                instr => instr.MatchCall<Entity>("set_Collider")))
            throw new HookHelper.HookException(il, "Unable to find assignment to `this.Collider`.");

        cursor.EmitLdarg0();
        cursor.EmitLdcI4((int) ExplosionTypes.Puffer);
        cursor.EmitDelegate(TriggerExplosionColliders);
    }

    private static void Seeker_RegenerateCoroutine(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdloc1(),
                instr => instr.MatchLdloc1(),
                instr => instr.MatchLdfld<Seeker>("physicsHitbox"),
                instr => instr.MatchCallvirt<Entity>("set_Collider")))
            throw new HookHelper.HookException(il, "Unable to find assignment to `this.Collider`.");

        cursor.EmitLdloc1();
        cursor.EmitLdcI4((int) ExplosionTypes.Seeker);
        cursor.EmitDelegate(TriggerExplosionColliders);
    }

    private static void TriggerExplosionColliders(Entity entity, ExplosionTypes type)
    {
        foreach (ExplosionCollider collider in entity.SceneAs<Level>().Tracker
            .GetComponents<ExplosionCollider>()
            .Cast<ExplosionCollider>()
            .Where(c => c.Check(entity)))
        {
            Collider c = collider.Collider;
            Vector2 toEntity = c.Center - entity.Position;
            Vector2 toEntityNormalized = toEntity / new Vector2(c.Width, c.Height);
            collider.OnCollide(type, toEntityNormalized.FourWayNormal());
        }
    }

    #endregion
}