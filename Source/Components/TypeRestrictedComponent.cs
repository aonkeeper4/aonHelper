namespace Celeste.Mod.aonHelper.Components;

public class TypeRestrictedComponent<TEntity>(bool active, bool visible) : Component(active, visible)
    where TEntity : Entity
{
    protected virtual string Name => nameof(TypeRestrictedComponent<>);
    
    public new TEntity Entity => EntityAs<TEntity>();
    
    public override void Added(Entity entity)
    {
        if (entity is not TEntity)
            throw new InvalidOperationException($"{Name} added to non-{typeof(TEntity)} entity!");
        
        base.Added(entity);
    }
}
