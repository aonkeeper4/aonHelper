using Celeste.Mod.aonHelper.Components.Colliders;
using Celeste.Mod.aonHelper.Entities.Controllers;
using Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;
using Celeste.Mod.aonHelper.Entities.Misc;
using MonoMod.ModInterop;

namespace Celeste.Mod.aonHelper;

[ModExportName("aonHelper")]
public static class aonHelperExports
{
    internal static void Load()
    {
        typeof(aonHelperExports).ModInterop();
    }

    #region Exports

    #region Fg Styleground Bloom Controller

    // exports for interfacing with the rendering changes imposed by fg styleground bloom controllers

    /// <summary>
    /// Adds a callback to be invoked before the <c>Foregound.Render</c> call in <see cref="Level.Render"/>.
    /// </summary>
    /// <param name="action">
    /// The callback to add, taking as arguments:
    /// <ul>
    ///   <li>the current <see cref="Level"/> instance</li>
    ///   <li>whether this callback is being invoked as part of the bloom rendering pass or not.</li>
    /// </ul>
    /// </param>
    public static void AddBeforeForegroundRenderAction(Action<Level, bool> action)
        => FgStylegroundBloomController.AddBeforeForegroundRenderAction(new FgStylegroundBloomController.RenderAction(action));

    /// <summary>
    /// Removes a callback from being invoked before the <c>Foregound.Render</c> call in <see cref="Level.Render"/>.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public static void RemoveBeforeForegroundRenderAction(Action<Level, bool> action)
        => FgStylegroundBloomController.RemoveBeforeForegroundRenderAction(new FgStylegroundBloomController.RenderAction(action));

    /// <summary>
    /// Adds a callback to be invoked after the <c>Foregound.Render</c> call in <see cref="Level.Render"/>.
    /// </summary>
    /// <param name="action">
    /// The callback to add, taking as arguments:
    /// <ul>
    ///   <li>the current <see cref="Level"/> instance</li>
    ///   <li>whether this callback is being invoked as part of the bloom rendering pass or not.</li>
    /// </ul>
    /// </param>
    public static void AddAfterForegroundRenderAction(Action<Level, bool> action)
        => FgStylegroundBloomController.AddAfterForegroundRenderAction(new FgStylegroundBloomController.RenderAction(action));

    /// <summary>
    /// Removes a callback from being invoked after the <c>Foregound.Render</c> call in <see cref="Level.Render"/>.
    /// </summary>
    /// <param name="action">The callback to remove.</param>
    public static void RemoveAfterForegroundRenderAction(Action<Level, bool> action)
        => FgStylegroundBloomController.RemoveAfterForegroundRenderAction(new FgStylegroundBloomController.RenderAction(action));

    /// <summary>
    /// Retrieves the bloom tag of the current <see cref="FgStylegroundBloomController"/>.
    /// </summary>
    /// <param name="level">The current <see cref="Level"/> instance to use.</param>
    /// <returns>The current controller's bloom tag, or <c>null</c> if there is no controller or it does not have a bloom tag set.</returns>
    public static string GetCurrentBloomTag(Level level)
        => FgStylegroundBloomController.GetCurrentBloomTag(level);

    #endregion

    #region echoing, the valley sings

    // exports for interfacing with entities from "echoing, the valley sings"

    /// <summary>
    /// Creates a <see cref="SoundWaveCollider"/> with the specified callback and collider.
    /// </summary>
    /// <param name="onCollide">
    /// The callback to run when a <see cref="SoundWave"/> collides with this collider, taking the direction of the incoming Sound Wave as argument.<br/>
    /// Must return the integer value of a valid <see cref="SoundWaveCollider.SoundWaveCollisionResults"/>.
    /// </param>
    /// <param name="collider">The <see cref="Collider"/> to use. If <c>null</c>, this defaults to the parent entity's collider.</param>
    /// <returns>The created Sound Wave Collider.</returns>
    public static Component CreateSoundWaveCollider(Func<Vector2, int> onCollide, Collider collider)
        => new SoundWaveCollider(direction => (SoundWaveCollider.SoundWaveCollisionResults) onCollide(direction), collider);

    #endregion

    #region Darker Matter

    // exports for interfacing with hit entity Darker Matter, as seen in hit campaign "Dark Matter Journey" by HexaliaCach9095 :yay:

    /// <summary>
    /// Returns the ID of the Darker Matter player state.
    /// </summary>
    /// <returns>The ID of the Darker Matter player state. If this is <c>-1</c>, the state has not been registered yet.</returns>
    public static int GetDarkerMatterState()
        => DarkerMatter.StDarkerMatter;

    #endregion

    #endregion
}
