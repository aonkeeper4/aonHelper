namespace Celeste.Mod.aonHelper.Components.Common;

public class ColliderComponent<TSelf, TCollision>(TCollision onCollide, Collider collider = null) : Component(false, false)
    where TSelf : ColliderComponent<TSelf, TCollision>
    where TCollision : Delegate
{
    public readonly TCollision OnCollide = onCollide;
    public readonly Collider Collider = collider;

    public virtual Color Color => Color.Red;

    public override void Added(Entity entity)
    {
        if (!Tracker.StoredComponentTypes.Contains(typeof(TSelf)))
            throw new InvalidOperationException($"Attempted to add a {nameof(ColliderComponent<,>)} of type `{typeof(TSelf)}` without it being tracked!");
        
        base.Added(entity);
    }
    
    public override void DebugRender(Camera camera)
    {
        Collider entityCollider = Entity.Collider;
        if (Collider is not null)
            Entity.Collider = Collider;
        
        Entity.Collider.Render(camera, Color * (Entity.Collidable ? 1f : 0.5f));
        
        Entity.Collider = entityCollider;
    }
    
    #region Collision
    
    public virtual bool Check(Entity other, Vector2? checkPosition = null)
    {
        Collider entityCollider = Entity.Collider;
        if (Collider is not null)
            Entity.Collider = Collider;

        bool result = checkPosition is { } at
            ? other.CollideCheck(Entity, at)
            : other.CollideCheck(Entity);

        Entity.Collider = entityCollider;
        return result;
    }

    public static TSelf[] GetCollidingColliders(Level level, Entity entity, Vector2? checkPosition = null)
        => level.Tracker.GetComponents<TSelf>()
            .Cast<TSelf>()
            .Where(c => c.OnCollide is not null && c.Check(entity, checkPosition))
            .ToArray();
    
    #endregion
}
