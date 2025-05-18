using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/LightningCornerboostController")]
[Tracked]
public class LightningCornerboostController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{ 
    private const string LogId = $"{nameof(aonHelperModule)}/{nameof(LightningCornerboostController)}";
    
    private class LightningSolidComponent(LightningCornerboostController controller) : Component(true, false)
    {
        private Lightning lightning;
        private Solid solid;

        private static readonly Vector2 Offset = new(3f, 4f);

        public override void Added(Entity entity)
        {
            if (entity is not Lightning lightningEntity)
                throw new Exception("LightningSolidComponent added to non-Lightning entity!");
            
            base.Added(entity);

            lightning = lightningEntity;
            solid = new Solid(lightning.Position + Offset, lightning.Width - 4f, lightning.Height - 5f, safe: false);
            
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
            solid.Position = lightning.Position + Offset;

            bool inView = lightning.InView();
            bool playerHasDashAttack = Scene.Tracker.GetEntity<Player>()?.DashAttacking ?? false;
            solid.Collidable = inView && (controller.always || playerHasDashAttack);
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
            base.Removed(entity);
            
            RemoveSolid();
        }
        
        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            
            RemoveSolid();
        }
    }

    private readonly bool always = data.Bool("always", true); // backwards compatibility

    public override void Added(Scene scene)
    {
        if (scene.Tracker.GetEntities<LightningCornerboostController>().Count >= 1)
        {
            Logger.Warn(LogId, "tried to load LightningCornerboostController when one was already present!");
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
