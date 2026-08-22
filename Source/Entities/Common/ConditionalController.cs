namespace Celeste.Mod.aonHelper.Entities.Common;

public class ConditionalController<TSelf>(ConditionHelper.Condition condition, bool active = false, Vector2? position = null) : Controller<TSelf>(active, position)
    where TSelf : ConditionalController<TSelf>
{
    public override bool ControllerActive => base.ControllerActive && condition.Check(SceneAs<Level>());
}
