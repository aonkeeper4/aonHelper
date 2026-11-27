namespace Celeste.Mod.aonHelper.Entities.Common;

// okay i kinda hate this but it's serviceable

public interface IBufferManager<TBuffers>
{
    static abstract void QueryBuffers(int depth, out TBuffers buffers);
}

public abstract class Renderer<TSelf, TRendered, TBuffers, TController> : Entity
    where TSelf :
        Renderer<TSelf, TRendered, TBuffers, TController>,
        Renderer<TSelf, TRendered, TBuffers, TController>.IStaticMethods
    where TRendered : Entity
    where TBuffers: IBufferManager<TBuffers>
    where TController : RendererController<TController>
{
    private static readonly string LogID = $"{nameof(aonHelper)}/{typeof(TSelf)}";
    
    // this is so stupid
    public interface IStaticMethods
    {
        static abstract TSelf Create(int rendererDepth);
    }
    
    public class Rendered(int? overrideDepth = null, Func<Camera, bool> isVisible = null) : TypeRestrictedComponent<TRendered>(false, false)
    {
        protected override string Name => $"{typeof(TSelf)}.{nameof(Rendered)}";

        public readonly Func<Camera, bool> IsVisible = isVisible;
        
        public TSelf Renderer { get; private set; }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
                
            Renderer = GetOrCreateRenderer(true);
            Renderer.EntityTracked(this);
        }
        
        public override void EntityRemoved(Scene scene)
        {
            Renderer.EntityUntracked(this);
            
            base.EntityRemoved(scene);
        }

        private TSelf GetOrCreateRenderer(bool checkNewlyAdded = false)
        {
            if (Scene is null || (overrideDepth ?? Entity?.Depth) is not { } depth)
                return null;

            if (Scene.Tracker.GetEntities<TSelf>()
                             .Concat(checkNewlyAdded ? Scene.Entities.ToAdd : [])
                             .FirstOrDefault(r => r is TSelf && r.Depth == depth)
                is TSelf renderer)
                return renderer;
            
            Scene.Add(renderer = TSelf.Create(depth));
            Logger.Info(LogID, $"Created new {typeof(TSelf)} at depth {depth}.");
            return renderer;
        }
    }
    
    private readonly List<Rendered> tracked = [];
    
    protected Renderer(int depth) : base(Vector2.Zero)
    {
        Tag = Tags.Global | Tags.TransitionUpdate;
        Depth = depth;
        Collidable = false;
        
        Add(new BeforeRenderHook(BeforeRender));
    }

    public override void Added(Scene scene)
    {
        if (!Tracker.StoredEntityTypes.Contains(typeof(TSelf)))
            throw new InvalidOperationException($"Attempted to add a {nameof(Renderer<,,,>)} of type `{typeof(TSelf)}` without it being tracked!");
        if (!Tracker.StoredEntityTypes.Contains(typeof(TController)))
            throw new InvalidOperationException($"Attempted to add a {nameof(Renderer<,,,>)} with controller type `{typeof(TController)}` without it being tracked!");
        
        base.Added(scene);
    }

    public virtual void EntityTracked(Rendered entity)
        => tracked.Add(entity);
    public virtual void EntityUntracked(Rendered entity)
        => tracked.Remove(entity);

    protected void QueryBuffers(out TBuffers buffers)
        => TBuffers.QueryBuffers(Depth, out buffers);
    
    private void BeforeRender()
    {
        QueryBuffers(out TBuffers buffers);
        TController currentController = GetController();

        BeforeRender(buffers, currentController);
    }

    protected virtual void BeforeRender(TBuffers buffers, TController controller) { }
    
    #region Helpers

    protected TRendered[] GetEntities()
        => tracked.Select(r => r.Entity)
                  .ToArray();
    protected TRendered[] GetVisibleEntities()
        => tracked.Where(r => r.IsVisible?.Invoke(SceneAs<Level>().Camera) ?? true)
                  .Select(r => r.Entity)
                  .ToArray();
    
    public TController GetController()
        => RendererController<TController>.GetControllerForDepth(SceneAs<Level>(), Depth);
    public bool TryGetController(out TController controller)
        => RendererController<TController>.TryGetControllerForDepth(SceneAs<Level>(), Depth, out controller);

    #endregion
}
