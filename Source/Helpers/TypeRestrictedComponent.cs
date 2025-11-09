using Monocle;
using System;

namespace Celeste.Mod.aonHelper.Helpers;

public class TypeRestrictedComponent<T>(bool active, bool visible) : Component(active, visible)
    where T : Entity
{
    public new T Entity => EntityAs<T>();

    protected virtual string Name => nameof(TypeRestrictedComponent<T>);
    
    public override void Added(Entity entity)
    {
        if (entity is not T)
            throw new InvalidOperationException($"{Name} added to non-{nameof(T)} entity!");
        
        base.Added(entity);
    }
}
