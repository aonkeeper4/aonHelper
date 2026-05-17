namespace Celeste.Mod.aonHelper.Entities.Common;

public class Controller<TSelf> : Entity where TSelf : Controller<TSelf>
{
    private string homeLevelName;
    
    public virtual bool ControllerActive => true;

    private int? priority;
    protected int Priority
    {
        get => priority ?? (TagCheck(Tags.Global) ? -1 : 0); // i don't like this. where to initialize priority?
        set => priority = value;
    }
    
    protected Controller(bool active = false, Vector2? position = null) : base(position ?? Vector2.Zero)
    {
        Depth = int.MinValue;
        Active = active;
        Collidable = Visible = false;
    }
    
    public override void Added(Scene scene)
    {
        if (!Tracker.StoredEntityTypes.Contains(typeof(TSelf)))
            throw new InvalidOperationException($"Attempted to add a {nameof(Controller<>)} of type {typeof(TSelf)} without it being tracked!");
        
        base.Added(scene);
        
        homeLevelName = SceneAs<Level>().Session.Level;
    }
    
    public override sealed void Update() { }
    public override sealed void Render() { }
    
    #region Helpers
    
    // todo: maybe don't use so much linq
    
    public static TSelf[] GetControllers(Level level, bool checkNewlyAdded = false, bool onlyActive = true)
        => level?.Tracker.GetEntities<TSelf>()
                         .Concat(checkNewlyAdded ? level.Entities.ToAdd : [])
                         .OfType<TSelf>()
                         .Where(c => 
                             (!onlyActive || c.ControllerActive)
                             && (c.TagCheck(Tags.Global) || c.homeLevelName == level.Session.Level))
                         .OrderByDescending(c => c.Priority)
                         .ToArray() ?? [];
    
    public static TSelf GetController(Level level, bool checkNewlyAdded = false, bool onlyActive = true)
        => GetControllers(level, checkNewlyAdded, onlyActive).FirstOrDefault();
    
    public static bool TryGetController(Level level, out TSelf controller, bool checkNewlyAdded = false, bool onlyActive = true)
    {
        controller = null;

        TSelf controllerEntity = GetController(level, checkNewlyAdded, onlyActive);
        if (controllerEntity is null)
            return false;
    
        controller = controllerEntity;
        return true;
    }
    
    #endregion
}
