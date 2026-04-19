namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/LightningCornerboostController")]
[Tracked]
public class LightningCornerboostController(Vector2 position, bool always, string condition)
    : ConditionalController<LightningCornerboostController>(position, condition)
{
    private class LightningSolidComponent(LightningCornerboostController controller) : TypeRestrictedComponent<Lightning>(true, false)
    {
        protected override string Name => nameof(LightningSolidComponent);
        
        private Solid solid;
        private static readonly Vector2 SolidOffset = new(3f, 4f);

        public override void Added(Entity entity)
        {
            base.Added(entity);

            solid = new Solid(Entity.Position + SolidOffset, Entity.Width - 4f, Entity.Height - 5f, safe: false)
            {
                Active = false,
                Collidable = false,
                Visible = false
            };
            
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
            solid.MoveTo(Entity.Position + SolidOffset);

            bool inView = Entity.InView();
            bool playerHasDashAttack = Scene.Tracker.GetEntity<Player>()?.DashAttacking ?? false;
            solid.Collidable = inView && controller.Active && (controller.always || playerHasDashAttack);
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

    public LightningCornerboostController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("always", true), data.Attr("flag"))
    { }

    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        On.Monocle.Entity.Awake += On_Entity_Awake;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Monocle.Entity.Awake -= On_Entity_Awake;
    }

    private static void On_Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
    {
        orig(self, scene);

        if (self is Lightning lightning && TryGetController(lightning.SceneAs<Level>(), out LightningCornerboostController controller))
            lightning.Add(new LightningSolidComponent(controller));
    }

    #endregion
}
