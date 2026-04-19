namespace Celeste.Mod.aonHelper.Entities.Controllers;

public class ConditionalController<T>(Vector2 position, string condition) : Controller<T>(position) where T : ConditionalController<T>
{
    private readonly ConditionHelper.Condition condition = ConditionHelper.Create(condition);
    
    protected override bool Active => base.Active && condition.Check(SceneAs<Level>());
}
