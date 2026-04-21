namespace Celeste.Mod.aonHelper.Entities.Controllers;

// yess generics jank
public class Controller<T> : Entity where T : Controller<T>
{
    private string homeLevelName;
    
    protected virtual new bool Active => true;
    
    protected Controller(Vector2 position) : base(position)
    {
        Depth = int.MinValue;
        base.Active = Collidable = Visible = false;
        
        Components.LockMode = ComponentList.LockModes.Error;
    }
    
    public override void Added(Scene scene)
    {
        Tracker.AddTypeToTracker(typeof(T));
        
        base.Added(scene);
        
        homeLevelName = SceneAs<Level>().Session.Level;
        Tracker.Refresh(); // hmm
    }
    
    public override sealed void Update() { }
    public override sealed void Render() { }
    
    #region Helpers
    
    // todo: maybe don't use so much linq
    
    protected static T[] GetControllers(Level level, bool checkNewlyAdded = false)
        => level?.Tracker.GetEntities<T>()
                         .Concat(checkNewlyAdded ? level.Entities.ToAdd : [])
                         .OfType<T>()
                         .Where(c => c.homeLevelName == level.Session.Level)
                         .ToArray() ?? [];
    
    protected static T[] GetActiveControllers(Level level, bool checkNewlyAdded = false)
        => GetControllers(level, checkNewlyAdded).Where(c => c.Active).ToArray();
    
    protected static bool TryGetController(Level level, out T controller, bool checkNewlyAdded = false)
    {
        controller = null;

        T controllerEntity = GetControllers(level, checkNewlyAdded).FirstOrDefault();
        if (controllerEntity is null)
            return false;
    
        controller = controllerEntity;
        return true;
    }
    
    protected static bool TryGetActiveController(Level level, out T controller, bool checkNewlyAdded = false)
    {
        controller = null;

        T controllerEntity = GetActiveControllers(level, checkNewlyAdded).FirstOrDefault();
        if (controllerEntity is null)
            return false;
    
        controller = controllerEntity;
        return true;
    }
    
    #endregion
}
