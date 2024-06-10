using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.aonHelper.Entities
{
    [CustomEntity("aonHelper/FgStylegroundBloomController")]
    [Tracked]
    public class FgStylegroundBloomController : Entity
    {
        public FgStylegroundBloomController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}