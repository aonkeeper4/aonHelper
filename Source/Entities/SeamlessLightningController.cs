using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/SeamlessLightningController")]
[Tracked]
public class SeamlessLightningController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    #region Hooks

    internal static void Load()
    {
        On.Celeste.LightningRenderer.Edge.ctor += LightningRenderer_Edge_ctor;
    }

    internal static void Unload()
    {
        On.Celeste.LightningRenderer.Edge.ctor -= LightningRenderer_Edge_ctor;
    }

    private static void LightningRenderer_Edge_ctor(On.Celeste.LightningRenderer.Edge.orig_ctor orig, object self, Lightning parent, Vector2 a, Vector2 b)
    {
        Level level = parent.SceneAs<Level>();
        if (level.Tracker.GetEntity<SeamlessLightningController>() is null)
        {
            orig(self, parent, a, b);
            return;
        }
        
        (bool recalculateA, Vector2 sideA) = OnBorderOrOutside(a + parent.Position, level.Bounds);
        (bool recalculateB, Vector2 sideB) = OnBorderOrOutside(b + parent.Position, level.Bounds);
        if (recalculateA && recalculateB && sideA != -sideB)
        {
            orig(self, parent, new Vector2(float.MinValue), new Vector2(float.MinValue));
            return;
        }

        Vector2 newA = a, newB = b;
        if (recalculateA)
        {
            Vector2 dir = (a - b).SafeNormalize();
            newA = a + dir * 8f;
        }
        if (recalculateB)
        {
            Vector2 dir = (b - a).SafeNormalize();
            newB = b + dir * 8f;
        }

        orig(self, parent, newA, newB);
    }

    private static (bool, Vector2) OnBorderOrOutside(Vector2 point, Rectangle rectangle)
    {
        float differenceLeft = point.X - rectangle.X;
        float differenceRight =  rectangle.X + rectangle.Width - point.X;
        float differenceTop = point.Y - rectangle.Y;
        float differenceBottom = rectangle.Y + rectangle.Height - point.Y;

        if (differenceLeft > 0f && differenceRight > 0f && differenceTop > 0f && differenceBottom > 0f)
            return (false, Vector2.Zero);

        Vector2 side = Vector2.Zero;
        float leastDifference = Calc.Min(differenceLeft, differenceRight, differenceTop, differenceBottom);
        if (leastDifference == differenceLeft)
            side = -Vector2.UnitX;
        else if (leastDifference == differenceRight)
            side = Vector2.UnitX;
        else if (leastDifference == differenceTop)
            side = -Vector2.UnitY;
        else if (leastDifference == differenceBottom)
            side = Vector2.UnitY;

        return (true, side);
    }

    #endregion
}
