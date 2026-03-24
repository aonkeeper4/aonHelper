using Monocle;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Celeste.Mod.aonHelper.Helpers;

// yess generics jank
public class FlagAffectedController<T>(Vector2 position, string flag) : Entity(position) where T : FlagAffectedController<T>
{
    protected readonly string Flag = string.IsNullOrEmpty(flag) ? null : flag;

    protected static bool ControllerActive(Level level, out T controller, bool checkNewlyAdded = false)
    {
        controller = null;
        if (level is null)
            return false;
        
        T controllerEntity = checkNewlyAdded
            ? level.Tracker.GetEntities<T>()
                           .Concat(level.Entities.ToAdd)
                           .OfType<T>()
                           .FirstOrDefault()
            : level.Tracker.GetEntity<T>();
        if (controllerEntity is null
            || !level.Session.GetFlag(controllerEntity.Flag) && controllerEntity.Flag is not null)
            return false;
        
        controller = controllerEntity;
        return true;
    }
}
