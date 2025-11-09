using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/LightningCornerboostController")]
[Tracked]
public class LightningCornerboostController(Vector2 position, bool always, string flag) : Entity(position)
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(LightningCornerboostController)}";
    
    private class LightningSolidComponent(LightningCornerboostController controller) : TypeRestrictedComponent<Lightning>(true, false)
    {
        protected override string Name => nameof(LightningSolidComponent);
        
        private Solid solid;

        private static readonly Vector2 Offset = new(3f, 4f);

        public override void Added(Entity entity)
        {
            base.Added(entity);

            solid = new Solid(Entity.Position + Offset, Entity.Width - 4f, Entity.Height - 5f, safe: false);
            
            // is this just a Monocle bug?
            if (entity.Scene is { } scene)
                EntityAdded(scene);
        }
        
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            
            scene.Add(solid);
        }

        public override void Update()
        {
            Level level = SceneAs<Level>();
            
            solid.Position = Entity.Position + Offset;

            bool inView = Entity.InView();
            bool playerHasDashAttack = Scene.Tracker.GetEntity<Player>()?.DashAttacking ?? false;
            solid.Collidable = inView
                && (controller.always || playerHasDashAttack)
                && (level.Session.GetFlag(controller.flag) || controller.flag is null);
            solid.Visible = inView;
        }

        private void RemoveSolid()
        {
            if (Scene is not { } scene || solid.Scene is null)
                return;
            
            scene.Remove(solid);
        }
        
        public override void Removed(Entity entity)
        {
            RemoveSolid();
            
            base.Removed(entity);
        }
        
        public override void EntityRemoved(Scene scene)
        {
            RemoveSolid();
            
            base.EntityRemoved(scene);
        }
    }

    private readonly bool always = always;

    private readonly string flag = string.IsNullOrEmpty(flag) ? null : flag;

    public LightningCornerboostController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("always", true), data.Attr("flag"))
    { }

    public override void Added(Scene scene)
    {
        if (scene.Tracker.GetEntities<LightningCornerboostController>().Count >= 1)
        {
            Logger.Warn(LogID, $"Tried to load a {nameof(LightningCornerboostController)} when one was already present!");
            RemoveSelf();
            return;
        }
        
        base.Added(scene);
    }

    #region Hooks

    internal static void Load()
    {
        On.Monocle.Entity.Awake += Entity_Awake;
    }

    internal static void Unload()
    {
        On.Monocle.Entity.Awake -= Entity_Awake;
    }

    private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
    {
        orig(self, scene);

        if (self is not Lightning lightning || scene.Tracker.GetEntity<LightningCornerboostController>() is not { } controller)
            return;

        lightning.Add(new LightningSolidComponent(controller));
    }

    #endregion
}
