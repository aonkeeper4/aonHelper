using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using DarkerMatterEntity = Celeste.Mod.aonHelper.Entities.DarkerMatter.DarkerMatter;
using Celeste.Mod.aonHelper.Entities.DarkerMatter;

// todo:
// sound for being inside it
// what if it like  muffled everything like water
// more visual stuff, similar to dream block with displacements maybe
// add feather interaction (separate state?)
namespace Celeste.Mod.aonHelper.States;

public static class DarkerMatter
{
    public static int StDarkerMatter { get; private set; }  = -1;

    public class DarkerMatterComponent() : Component(false, false)
    {
        public DarkerMatterController Controller;
        public DarkerMatterEntity LastDarkerMatter;
        public float StopGraceTimer;
        
        public Sprite WarpSprite;
        public static readonly Vector2 WarpSpriteOffset = new(16f, 24f);
    }

    private static void DarkerMatterBegin(this Player player)
    {
        if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
            return;
        
        darkerMatterComponent.LastDarkerMatter = null;
        darkerMatterComponent.StopGraceTimer = darkerMatterComponent.Controller.StopGraceTime;
        darkerMatterComponent.WarpSprite.Visible = true;
    }

    private static void DarkerMatterEnd(this Player player)
    {
        if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
            return;

        player.RefillDash();
        darkerMatterComponent.WarpSprite.Visible = false;
    }

    private static int DarkerMatterUpdate(this Player player)
    {
        if (player.Get<DarkerMatterComponent>() is not { } darkerMatterComponent)
            return Player.StNormal;

        darkerMatterComponent.WarpSprite.Play("boost");

        bool shouldEnterDarkerMatterState = false;

        if (player.CollideFirst<DarkerMatterEntity>() is { } darkerMatter)
        {
            darkerMatterComponent.LastDarkerMatter = darkerMatter;
            shouldEnterDarkerMatterState = true;
        }

        // wrap check
        DarkerMatterEntity last = darkerMatterComponent.LastDarkerMatter;
        if (last is { WrapHorizontal: true })
        {
            if (player.Center.X <= last.Left && player.Speed.X < 0)
            {
                player.NaiveMove(last.Width * Vector2.UnitX);
                shouldEnterDarkerMatterState = true;
            }
            else if (player.Center.X >= last.Right && player.Speed.X > 0)
            {
                player.NaiveMove(-last.Width * Vector2.UnitX);
                shouldEnterDarkerMatterState = true;
            }
        }
        if (last is { WrapVertical: true })
        {
            if (player.Center.Y <= last.Top && player.Speed.Y < 0)
            {
                player.NaiveMove(last.Height * Vector2.UnitY);
                shouldEnterDarkerMatterState = true;
            }
            else if (player.Center.Y >= last.Bottom && player.Speed.Y > 0)
            {
                player.NaiveMove(-last.Height * Vector2.UnitY);
                shouldEnterDarkerMatterState = true;
            }
        }

        if (shouldEnterDarkerMatterState)
        {
            if (darkerMatterComponent.StopGraceTimer <= 0f)
                player.Die(Vector2.Zero, true);

            if (player.Speed == Vector2.Zero)
                darkerMatterComponent.StopGraceTimer -= Engine.DeltaTime;
            else
                darkerMatterComponent.StopGraceTimer = darkerMatterComponent.Controller.StopGraceTime;

            float amplitude = Math.Clamp(player.Speed.Length(), 0f, darkerMatterComponent.Controller.SpeedLimit);
            Vector2 unitMovement = player.Speed.SafeNormalize();
            player.Speed = unitMovement * amplitude;

            return StDarkerMatter;
        }

        return Player.StNormal;
    }

    private static IEnumerator DarkerMatterRoutine(this Player _)
    {
        yield return null;
    }
    
    #region Hooks

    public static void Load()
    {
        Everest.Events.Player.OnRegisterStates += OnRegisterStates;
        Everest.Events.Player.OnSpawn += OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;
    }

    public static void Unload()
    {
        Everest.Events.Player.OnRegisterStates -= OnRegisterStates;
        Everest.Events.Player.OnSpawn -= OnSpawn;
        Everest.Events.AssetReload.OnBeforeReload -= OnBeforeReload;
    }

    private static void OnRegisterStates(Player player)
    {
        StDarkerMatter = player.AddState("StDarkerMatter", DarkerMatterUpdate, DarkerMatterRoutine, DarkerMatterBegin, DarkerMatterEnd);
    }

    private static void OnSpawn(Player player)
    {
        DarkerMatterComponent darkerMatterComponent = new()
        {
            WarpSprite = aonHelperModule.SpriteBank.Create("aonHelper_darkerMatterWarp")
        };
        darkerMatterComponent.WarpSprite.Visible = false;
        darkerMatterComponent.WarpSprite.Origin = DarkerMatterComponent.WarpSpriteOffset;
        
        player.Add(darkerMatterComponent);
        player.Add(darkerMatterComponent.WarpSprite);
    }

    private static void OnBeforeReload(bool silent)
    {
        if (Engine.Scene?.Tracker?.GetEntity<Player>() is { } player)
            player.Remove(player.Get<DarkerMatterComponent>());
    }
    
    #endregion
}