namespace Celeste.Mod.aonHelper.Entities.Controllers;

// yess generics jank
public class ConditionalController<T>(Vector2 position, string condition) : Controller(position) where T : ConditionalController<T>
{
    private readonly ConditionHelper.Condition condition = ConditionHelper.Create(condition);
    
    protected new bool Active => condition.Check(SceneAs<Level>());

    public override void Added(Scene scene)
    {
        base.Added(scene);

        // no way to automatically track all instantiations of a generic type and no way to get trackedness information ahead of time
        if (!Tracker.StoredEntityTypes.Contains(typeof(T)))
            throw new InvalidOperationException($"{nameof(ConditionalController<T>)} added while {nameof(T)} is untracked!");
    }

    protected static bool TryGetActiveController(Level level, out T controller, bool checkNewlyAdded = false)
    {
        controller = null;
        if (level is null)
            return false;
        
        T controllerEntity = checkNewlyAdded
            ? level.Tracker.GetEntities<T>()
                           .Concat(level.Entities.ToAdd)
                           .OfType<T>()
                           .FirstOrDefault()
            : level.Tracker.GetEntity<T>();
        if (controllerEntity is null || !controllerEntity.Active)
            return false;
        
        controller = controllerEntity;
        return true;
    }
}
