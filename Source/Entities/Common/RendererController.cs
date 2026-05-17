namespace Celeste.Mod.aonHelper.Entities.Common;

public class RendererController<TSelf>(int? affectedDepth) : Controller<TSelf> where TSelf : RendererController<TSelf>
{
    private readonly int? affectedDepth = affectedDepth;
    
    public static TSelf GetControllerForDepth(Level level, int depth)
        => GetControllers(level).FirstOrDefault(c => c.affectedDepth is null || c.affectedDepth == depth);

    public static bool TryGetControllerForDepth(Level level, int depth, out TSelf controller)
    {
        controller = null;

        TSelf controllerEntity = GetControllerForDepth(level, depth);
        if (controllerEntity is null)
            return false;
    
        controller = controllerEntity;
        return true;
    }
}
