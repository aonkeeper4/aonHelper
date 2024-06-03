using Celeste.Mod.Entities;
using Celeste;
using Monocle;
using Celeste.Mod.aonHelper;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections;
using System;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities
{
    [TrackedAs(typeof(HeartGem))]
    [CustomEntity("aonHelper/ResizableHeart")]
    public class ResizableHeart : HeartGem
    {
        private Sprite spriteOutline;

        private Color color;

        private readonly string spriteId;

        private readonly int width, height;

        public float respawnTimer;

        public float baseRespawnTimer = 3f;

        public bool fake;

        public ResizableHeart(EntityData data, Vector2 offset) : base(data, offset)
        {
            entityID = new EntityID(data.Level.Name, data.ID);
            color = Calc.HexToColor(data.Attr("color", "00a81f"));
            spriteId = data.Attr("path");
            width = data.Width;
            height = data.Height;
            fake = data.Bool("isFake");
            baseRespawnTimer = data.Float("respawnTimer", 3f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AreaKey area = (scene as Level).Session.Area;
            IsGhost = !IsFake && !fake && SaveData.Instance.Areas_Safe[area.ID].Modes[(int)area.Mode].HeartGem;
            Collider = new Hitbox(width, height, -width / 2, -height / 2);
            Remove(sprite);
            if (!string.IsNullOrWhiteSpace(spriteId))
            {
                sprite = GFX.SpriteBank.Create(spriteId);
                sprite.Play("spin");
                if (IsGhost)
                {
                    sprite.Color = Color.White * 0.8f;
                }
                switch (spriteId)
                {
                    case "heartgem0":
                        color = Color.Aqua;
                        shineParticle = P_BlueShine;
                        break;
                    case "heartgem1":
                        color = Color.Red;
                        shineParticle = P_RedShine;
                        break;
                    case "heartgem2":
                        color = Color.Gold;
                        shineParticle = P_GoldShine;
                        break;
                    case "heartgem3":
                        color = Calc.HexToColor("dad8cc");
                        shineParticle = P_FakeShine;
                        break;
                    default:
                        shineParticle = new ParticleType(P_BlueShine)
                        {
                            Color = color
                        };
                        break;
                }
            }
            else
            {
                sprite = aonHelperModule.SpriteBank.Create("aonHelper_resizableHeart");
                spriteOutline = aonHelperModule.SpriteBank.Create("aonHelper_resizableHeartOutline");
                sprite.Color = IsGhost && !fake ? (Color.Lerp(color, Color.White, 0.8f) * 0.8f) : color;
                shineParticle = new ParticleType(P_BlueShine)
                {
                    Color = color
                };
            }
            sprite.OnLoop = delegate (string anim)
            {
                if (Visible && anim == "spin" && autoPulse)
                {
                    if (IsFake)
                    {
                        Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
                    }
                    else
                    {
                        Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    }
                    ScaleWiggler.Start();
                    (base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
                }
            };
            Remove(ScaleWiggler);
            ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            });
            Add(ScaleWiggler);
            Add(sprite);
            if (spriteOutline != null)
            {
                Add(spriteOutline);
            }
            light.Color = Color.Lerp(color, Color.White, 0.5f);
        }

        public override void Update()
        {
            if (fake)
                collected = respawnTimer > 0f;
            base.Update();

            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Collidable = Visible = true;
                    ScaleWiggler.Start();
                }
            }
            if (spriteOutline != null)
            {
                spriteOutline.Position = sprite.Position;
                spriteOutline.Scale = sprite.Scale;
                if (spriteOutline.CurrentAnimationID != sprite.CurrentAnimationID)
                {
                    spriteOutline.Play(sprite.CurrentAnimationID);
                }
                spriteOutline.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
        }

        public void CollectFake(Player player, float angle)
        {
            if (Collidable)
            {
                Collidable = Visible = false;
                respawnTimer = baseRespawnTimer;
                Celeste.Freeze(0.05f);
                SceneAs<Level>().Shake();
                SlashFx.Burst(Position, angle);
                player?.RefillDash();
            }
        }
        private static void mod_HeartGemOnHoldable(On.Celeste.HeartGem.orig_OnHoldable orig, HeartGem self, Holdable h)
        {
            if (self is ResizableHeart resizable)
            {
                Player entity = resizable.Scene.Tracker.GetEntity<Player>();
                if (resizable.fake)
                {

                    if (resizable.Visible && h.Dangerous(resizable.holdableCollider))
                    {
                        resizable.CollectFake(entity, h.GetSpeed().Angle());
                    }
                }
                else
                {
                    orig(self, h);
                }
            }
            else
            {
                orig(self, h);
            }
        }

        private static void mod_HeartGemOnPlayer(On.Celeste.HeartGem.orig_OnPlayer orig, HeartGem self, Player player)
        {
            if (self is ResizableHeart resizable)
            {
                if (resizable.fake)
                {
                    if (!resizable.Visible || (resizable.Scene as Level).Frozen)
                    {
                        return;
                    }
                    if (player.DashAttacking)
                    {
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        resizable.CollectFake(player, player.Speed.Angle());
                        return;
                    }
                    if (resizable.bounceSfxDelay <= 0f)
                    {
                        Audio.Play("event:/game/general/crystalheart_bounce", resizable.Position);
                        resizable.bounceSfxDelay = 0.1f;
                    }
                    player.PointBounce(resizable.Center);
                    resizable.moveWiggler.Start();
                    resizable.ScaleWiggler.Start();
                    resizable.moveWiggleDir = (resizable.Center - player.Center).SafeNormalize(Vector2.UnitY);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else
                {
                    orig(self, player);
                }
            }
            else
            {
                orig(self, player);
            }
        }

        public static void Load()
        {
            // On.Celeste.HeartGem.Collect += modCollect;
            On.Celeste.HeartGem.OnPlayer += mod_HeartGemOnPlayer;
            On.Celeste.HeartGem.OnHoldable += mod_HeartGemOnHoldable;
        }

        public static void Unload()
        {

            // On.Celeste.HeartGem.Collect -= modCollect;
            On.Celeste.HeartGem.OnPlayer -= mod_HeartGemOnPlayer;
            On.Celeste.HeartGem.OnHoldable -= mod_HeartGemOnHoldable;
        }
    }
}