namespace Celeste.Mod.aonHelper.Entities.Common;

public class ConditionalController<TSelf>(string condition, bool active = false, Vector2? position = null) : Controller<TSelf>(active, position)
    where TSelf : ConditionalController<TSelf>
{
    private readonly ConditionHelper.Condition condition = ConditionHelper.Create(condition);
    
    public override bool ControllerActive => base.ControllerActive && condition.Check(SceneAs<Level>());
}
