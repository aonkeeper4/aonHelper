using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using MonoMod.Utils;
using System;
using DarkerMatterEntity = Celeste.Mod.aonHelper.Entities.DarkerMatter.DarkerMatter;
using Celeste.Mod.aonHelper.Entities.DarkerMatter;

// todo:
// sound for being inside it
// what if it like  muffled everything like water
// more visual stuff, similar to dream block with displacements maybe
// add feather interaction (separate state?)
namespace Celeste.Mod.aonHelper.States
{
    public static class DarkerMatter
    {
        private static DarkerMatterController controller = null;

        private const string f_Player_lastDarkerMatter = nameof(f_Player_lastDarkerMatter); // DarkerMatterEntity
        private const string f_Player_warpSprite = nameof(f_Player_warpSprite); // Sprite

        private static readonly Vector2 warpSpriteOffset = new(16f, 24f); // todo: gravityhelper support

        private static float stopGraceTimer;

        public static void DarkerMatterBegin(this Player player)
        {
            DynamicData data = DynamicData.For(player);

            data.Get<Sprite>(f_Player_warpSprite).Visible = true;
            data.Set(f_Player_lastDarkerMatter, null);

            stopGraceTimer = controller.StopGraceTimer;
        }

        public static void DarkerMatterEnd(this Player player)
        {
            DynamicData data = DynamicData.For(player);

            player.RefillDash();
            data.Get<Sprite>(f_Player_warpSprite).Visible = false;
        }

        public static int DarkerMatterUpdate(this Player player)
        {
            DynamicData data = DynamicData.For(player);

            Sprite warpSprite = data.Get<Sprite>(f_Player_warpSprite);
            warpSprite.Play("boost");

            bool shouldEnterDarkerMatterState = false;

            if (player.CollideFirst<DarkerMatterEntity>() is DarkerMatterEntity darkerMatter)
            {
                data.Set(f_Player_lastDarkerMatter, darkerMatter);
                shouldEnterDarkerMatterState = true;
            }

            // wrap check
            DarkerMatterEntity last = data.Get<DarkerMatterEntity>(f_Player_lastDarkerMatter);
            if (last is DarkerMatterEntity { wrapHorizontal: true })
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
            if (last is DarkerMatterEntity { wrapVertical: true })
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
                if (stopGraceTimer <= 0f)
                    player.Die(Vector2.Zero, true);

                if (player.Speed == Vector2.Zero)
                    stopGraceTimer -= Engine.DeltaTime;
                else
                    stopGraceTimer = controller.StopGraceTimer;

                float amplitude = Math.Clamp(player.Speed.Length(), 0f, controller.SpeedLimit);
                Vector2 unitMovement = player.Speed.SafeNormalize();
                player.Speed = unitMovement * amplitude;

                return St.DarkerMatter;
            }
            else
            {
                return Player.StNormal;
            }
        }

        public static IEnumerator DarkerMatterRoutine(this Player _)
        {
            yield return null;
        }

        public static void Initialize()
        {

        }

        public static void Load()
        {
            On.Celeste.Player.ctor += Player_ctor;
        }

        public static void Unload()
        {
            On.Celeste.Player.ctor -= Player_ctor;
        }

        private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            DynamicData data = DynamicData.For(self);
            Sprite warpSprite = aonHelperModule.SpriteBank.Create("aonHelper_darkerMatterWarp");
            warpSprite.Visible = false;
            warpSprite.Origin = warpSpriteOffset;
            data.Set(f_Player_warpSprite, warpSprite);
            self.Add(warpSprite);

            data.Set(f_Player_lastDarkerMatter, null);
        }

        public static void SetController(DarkerMatterController controller)
        {
            DarkerMatter.controller ??= controller;
        }
    }
}