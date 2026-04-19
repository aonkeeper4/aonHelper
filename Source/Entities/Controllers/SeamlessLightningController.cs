namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/SeamlessLightningController")]
[Tracked]
public class SeamlessLightningController(EntityData data, Vector2 offset) : Controller<SeamlessLightningController>(data.Position + offset)
{
    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.LightningRenderer.Edge.ctor += On_LightningRenderer_Edge_ctor;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.LightningRenderer.Edge.ctor -= On_LightningRenderer_Edge_ctor;
    }

    private static void On_LightningRenderer_Edge_ctor(On.Celeste.LightningRenderer.Edge.orig_ctor orig, object self, Lightning parent, Vector2 a, Vector2 b)
    {
        Level level = parent.SceneAs<Level>();
        if (!TryGetActiveController(level, out _))
        {
            orig(self, parent, a, b);
            return;
        }
        
        Rectangle bounds = parent.SourceData?.Level?.Bounds ?? level.Bounds;
        (bool aTouchingBounds, Vector2 sideA) = OnBorder(a + parent.Position, bounds);
        (bool bTouchingBounds, Vector2 sideB) = OnBorder(b + parent.Position, bounds);
        if (aTouchingBounds && bTouchingBounds && sideA != -sideB)
        {
            // exiled to the void
            orig(self, parent, new Vector2(float.NaN), new Vector2(float.NaN));
            return;
        }

        orig(self, parent, a, b);
    }

    private static (bool, Vector2) OnBorder(Vector2 point, Rectangle rectangle)
    {
        if (rectangle.X - point.X == 0f)
            return (true, -Vector2.UnitX);
        if (rectangle.X + rectangle.Width - point.X == 0f)
            return (true, Vector2.UnitX);
        if (rectangle.Y - point.Y == 0f)
            return (true, -Vector2.UnitY);
        if (rectangle.Y + rectangle.Height - point.Y == 0f)
            return (true, Vector2.UnitY);

        return (false, Vector2.Zero);
    }

    #endregion
}
