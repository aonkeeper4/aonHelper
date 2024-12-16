using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.aonHelper.States;

// todo:
// add particles
namespace Celeste.Mod.aonHelper.Entities.DarkerMatter
{
    [CustomEntity("aonHelper/DarkerMatter")]
    [Tracked]
    public class DarkerMatter : Entity
    {
        public readonly bool wrapHorizontal, wrapVertical;

        private static DarkerMatterController controller;

        public enum EdgeType
        {
            Normal,
            Warp,
        }

        public EdgeType[] EdgeTypes { get; private set; } = [EdgeType.Normal, EdgeType.Normal, EdgeType.Normal, EdgeType.Normal]; // left, right, top, bottom

        public DarkerMatter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            wrapHorizontal = data.Bool("wrapHorizontal");
            wrapVertical = data.Bool("wrapVertical");

            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (scene.Tracker.GetEntities<DarkerMatterRenderer>().Count < 1)
            {
                scene.Add(new DarkerMatterRenderer());
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (wrapHorizontal)
            {
                EdgeTypes[0] = EdgeTypes[1] = EdgeType.Warp;
            }
            if (wrapVertical)
            {
                EdgeTypes[2] = EdgeTypes[3] = EdgeType.Warp;
            }

            if ((scene as Level).Tracker.GetEntities<DarkerMatterController>().Count >= 1)
            {
                controller = (scene as Level).Tracker.GetEntity<DarkerMatterController>();
            }
            else
            {
                scene.Add(controller = DarkerMatterController.Default());
            }
            States.DarkerMatter.SetController(controller);

            DarkerMatterRenderer renderer = scene.Tracker.GetEntity<DarkerMatterRenderer>();
            if (renderer is not null)
                renderer.Track(this, SceneAs<Level>(), controller);
            else
            {
                scene.Add(renderer = new DarkerMatterRenderer());
                renderer.Track(this, scene as Level, controller);
            }
        }

        public override void Update()
        {
            base.Update();

            if (CollideFirst<Player>() is Player player && player.Speed.Length() >= controller.SpeedThreshold && player.StateMachine.State != St.DarkerMatter)
                player.StateMachine.State = St.DarkerMatter;
        }

        public override void Render()
        {
            base.Render();

            Level level = SceneAs<Level>();
            Draw.Rect(Collider, (level.Tracker.GetEntity<DarkerMatterRenderer>()?.ColorCycle(level, 0) ?? Color.White) * 0.6f);
        }

        public override void Removed(Scene scene)
        {
            scene.Tracker.GetEntity<DarkerMatterRenderer>().Untrack(this);

            base.Removed(scene);
        }
    }
}