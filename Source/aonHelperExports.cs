using Celeste.Mod.aonHelper.Components.Colliders;
using Celeste.Mod.aonHelper.Entities.Controllers;
using Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;
using MonoMod.ModInterop;

namespace Celeste.Mod.aonHelper;

public static class aonHelperExports
{
    internal static void Load()
    {
        typeof(FgStylegroundBloomControllerCompat).ModInterop();
        typeof(EchoingTheValleySings).ModInterop();
    }
    
    /// <summary>
    /// Provides <see cref="MonoMod.ModInterop"/> exports for interfacing with the rendering changes imposed by <see cref="FgStylegroundBloomController"/>s.
    /// </summary>
    [ModExportName("aonHelper.FgStylegroundBloomControllerCompat")]
    public static class FgStylegroundBloomControllerCompat
    {
        /// <summary>
        /// Adds a callback to be invoked before the <c>Foregound.Render</c> call in <see cref="Level.Render"/>.
        /// </summary>
        /// <param name="action">
        /// The callback to add, taking as arguments:
        /// <ul>
        ///   <li>the current <see cref="Level"/> instance</li>
        ///   <li>whether this callback is being invoked as part of the bloom rendering pass or not</li>
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
        ///   <li>whether this callback is being invoked as part of the bloom rendering pass or not</li>
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
    }

    /// <summary>
    /// Provides <see cref="MonoMod.ModInterop"/> exports for interfacing with entities from "echoing, the valley sings".
    /// </summary>
    [ModExportName("aonHelper.EchoingTheValleySings")]
    public static class EchoingTheValleySings
    {
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
            => new SoundWaveCollider(onCollide is null ? null : direction => (SoundWaveCollider.SoundWaveCollisionResults) onCollide(direction), collider);
    }
}
