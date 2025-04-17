using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.aonHelper.States.DarkerMatter;

// todo:
// add particles
namespace Celeste.Mod.aonHelper.Entities.DarkerMatter;

[CustomEntity("aonHelper/DarkerMatter")]
[Tracked]
public class DarkerMatter : Entity
{
    public readonly bool WrapHorizontal, WrapVertical;

    public enum EdgeType
    {
        Normal,
        Warp,
    }

    public EdgeType[] EdgeTypes { get; } = [EdgeType.Normal, EdgeType.Normal, EdgeType.Normal, EdgeType.Normal]; // left, right, top, bottom

    public DarkerMatter(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        WrapHorizontal = data.Bool("wrapHorizontal");
        WrapVertical = data.Bool("wrapVertical");

        Collider = new Hitbox(data.Width, data.Height);
        
        if (WrapHorizontal)
        {
            EdgeTypes[0] = EdgeTypes[1] = EdgeType.Warp;
        }
        if (WrapVertical)
        {
            EdgeTypes[2] = EdgeTypes[3] = EdgeType.Warp;
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        DarkerMatterController controller;
        if ((scene as Level)!.Tracker.GetEntities<DarkerMatterController>().Count >= 1)
        {
            controller = (scene as Level)!.Tracker.GetEntity<DarkerMatterController>();
        }
        else
        {
            scene.Add(controller = DarkerMatterController.Default());
        }
        if ((scene as Level)!.Tracker.GetEntity<Player>() is { } player)
            player.Get<DarkerMatterComponent>().Controller = controller;

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

        if (CollideFirst<Player>() is { } player 
            && player.Get<DarkerMatterComponent>() is { } darkerMatterComponent
            && player.Speed.Length() >= darkerMatterComponent.Controller.SpeedThreshold
            && player.StateMachine.State != StDarkerMatter)
            player.StateMachine.State = StDarkerMatter;
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