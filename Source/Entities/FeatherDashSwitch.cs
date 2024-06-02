using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.aonHelper.Entities
{
    [CustomEntity("aonHelper/FeatherDashSwitch")]
    public class FeatherDashSwitch : DashSwitch
    {
        private DynamicData baseData;

        private new ParticleType P_PressA;
        private new ParticleType P_PressB;

        public FeatherDashSwitch(EntityData data, Vector2 offset) : base(data.Position + offset, (Sides)data.Int("side", 0), false, false, new EntityID(data.Level.Name, data.ID), "default")
        {
            baseData = new DynamicData(typeof(DashSwitch), this);
            P_PressA = new ParticleType
            {
                Color = Calc.HexToColor(data.Attr("particleColor1")),
                Color2 = Calc.HexToColor(data.Attr("particleColor2")),
                ColorMode = ParticleType.ColorModes.Blink,
                Size = 1f,
                SizeRange = 0f,
                SpeedMin = 60f,
                SpeedMax = 80f,
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                DirectionRange = 0.7f,
                SpeedMultiplier = 0.2f
            };
            P_PressB = new ParticleType(P_PressA)
            {
                SpeedMin = 100f,
                SpeedMax = 110f,
                DirectionRange = 0.35f
            };
            OnDashCollide = OnDashed;
            // OnCollide = OnFeatherHit;

            Sprite sprite = baseData.Get<Sprite>("sprite");
            Vector2 spritePos = sprite.Position;
            float spriteRot = sprite.Rotation;
            sprite.Stop();
            Remove(sprite);
            sprite = aonHelperModule.SpriteBank.Create("aonHelper_featherDashSwitch");
            sprite.Position = spritePos;
            sprite.Rotation = spriteRot;
            baseData.Set("sprite", sprite);
            Add(sprite);
            sprite.Play("idle");
        }

        public void OnFeatherHit(Vector2 direction)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player is null) { return; }
            DynamicData playerData = DynamicData.For(player);

            // Logger.Log("aonHelper/OnFeatherHit", $"we hit the dash switch with feather timer {playerData.Get<float>("starFlyTimer")}");

            Vector2 pressDirection = baseData.Get<Vector2>("pressDirection");
            Sprite sprite = baseData.Get<Sprite>("sprite");
            if (!baseData.Get<bool>("pressed") && Vector2.Dot(direction, pressDirection) > 0)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("push");
                baseData.Set("pressed", true);
                MoveTo(baseData.Get<Vector2>("pressedTarget"));
                Collidable = false;
                Position -= pressDirection * 2f;
                SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
                SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - (float)Math.PI);
            }
            if (baseData.Get<bool>("allGates"))
            {
                foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>())
                {
                    if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == baseData.Get<EntityID>("id").Level)
                    {
                        entity.SwitchOpen();
                    }
                }
            }
            else
            {
                baseData.Invoke<TempleGate>("GetGate", [])?.SwitchOpen();
            }
            if (baseData.Get<bool>("persistent"))
            {
                SceneAs<Level>().Session.SetFlag(baseData.Get<string>("FlagName"));
            }

            // make it work with crystalline all dash switch temple gates (whenever they update that helper)
            OnDashCollide.Invoke(player, direction);
            // OnDashed(player, direction);
        }

        private static new DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            return DashCollisionResults.NormalCollision;
        }

        private static void mod_PlayerOnCollideH(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Player), "starFlyTimer"));
            // Logger.Log("aonHelper/FeatherDashSwitch/mod_PlayerOnCollideH", $"Inserting call to checkOnFeather at index {cursor.Index} in CIL code for {cursor.Method.FullName}");
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(checkOnFeather);
        }

        private static void mod_PlayerOnCollideV(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Player), "starFlyTimer"));
            // Logger.Log("aonHelper/FeatherDashSwitch/mod_PlayerOnCollideV", $"Inserting call to checkOnFeather at index {cursor.Index} in CIL code for {cursor.Method.FullName}");
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(checkOnFeather);
        }

        private static void checkOnFeather(CollisionData data)
        {
            if (data.Hit is null) { return; }
            if (data.Hit is FeatherDashSwitch dashSwitch)
            {
                dashSwitch.OnFeatherHit(data.Direction);
            }
        }

        public static void Load()
        {
            IL.Celeste.Player.OnCollideH += mod_PlayerOnCollideH;
            IL.Celeste.Player.OnCollideV += mod_PlayerOnCollideV;
        }

        public static void Unload()
        {
            IL.Celeste.Player.OnCollideH -= mod_PlayerOnCollideH;
            IL.Celeste.Player.OnCollideV -= mod_PlayerOnCollideV;
        }
    }
}