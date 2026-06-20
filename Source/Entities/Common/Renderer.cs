namespace Celeste.Mod.aonHelper.Entities.Common;

// okay i kinda hate this but it's serviceable

public abstract class Renderer<TSelf, TRendered, TBuffers, TController> : Entity
    where TSelf :
        Renderer<TSelf, TRendered, TBuffers, TController>,
        Renderer<TSelf, TRendered, TBuffers, TController>.IStaticMethods
    where TRendered : Entity
    where TBuffers: IBufferManager<TBuffers>
    where TController : RendererController<TController>
{
    // this is so stupid
    public interface IStaticMethods
    {
        static abstract string Name { get; }
        static abstract string LogID { get; }
        
        static abstract TSelf Create(int rendererDepth);
    }
    
    public class Rendered(int? overrideDepth = null, Func<Camera, bool> isVisible = null) : TypeRestrictedComponent<TRendered>(false, false)
    {
        public readonly Func<Camera, bool> IsVisible = isVisible;
        
        public TSelf Renderer { get; private set; }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
                
            Renderer = GetOrCreateRenderer(true);
            Renderer.tracked.Add(this);
        }
        
        public override void EntityRemoved(Scene scene)
        {
            Renderer.tracked.Remove(this);
            
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
            Logger.Info(TSelf.LogID, $"Created new {TSelf.Name} at depth {depth}.");
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
            throw new InvalidOperationException($"Attempted to add a {nameof(Renderer<,,,>)} of type {typeof(TSelf)} without it being tracked!");
        if (!Tracker.StoredEntityTypes.Contains(typeof(TController)))
            throw new InvalidOperationException($"Attempted to add a {nameof(Renderer<,,,>)} with controller type {typeof(TController)} without it being tracked!");
        
        base.Added(scene);
    }

    private void BeforeRender()
    {
        QueryBuffers(out TBuffers buffers);
        TController currentController = GetController();

        BeforeRender(buffers, currentController);
    }

    protected virtual void BeforeRender(TBuffers buffers, TController controller) { }
    
    protected TRendered[] GetEntitiesToRender()
        => tracked.Where(r => r.IsVisible?.Invoke(SceneAs<Level>().Camera) ?? true)
                  .Select(r => r.Entity)
                  .ToArray();

    protected void QueryBuffers(out TBuffers buffers)
        => TBuffers.QueryBuffers(Depth, out buffers);
    
    public TController GetController()
        => RendererController<TController>.GetControllerForDepth(SceneAs<Level>(), Depth);
    
    public bool TryGetController(out TController controller)
        => RendererController<TController>.TryGetControllerForDepth(SceneAs<Level>(), Depth, out controller);
}
