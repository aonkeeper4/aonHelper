using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FeatherDashSwitch")]
internal class LegacyFeatherDashSwitch : DashSwitch
{
    private new readonly ParticleType P_PressA;
    private new readonly ParticleType P_PressB;

    public LegacyFeatherDashSwitch(EntityID id, Vector2 position, Sides side,
        bool persistent, bool allGates,
        string spriteDir, Color particleColor1, Color particleColor2)
        : base(position, side, persistent, allGates, id, "default")
    {
        Vector2 spritePos = sprite.Position;
        float spriteRot = sprite.Rotation;
        sprite.Stop();
        Remove(sprite);
        
        sprite = string.IsNullOrEmpty(spriteDir) ? aonHelperModule.SpriteBank.Create("aonHelper_featherDashSwitch") : BuildSprite(spriteDir);
        sprite.Position = spritePos;
        sprite.Rotation = spriteRot;
        Add(sprite);
        sprite.Play("idle");
        
        OnDashCollide = OnDashed;
        
        P_PressA = new ParticleType
        {
            Color = particleColor1,
            Color2 = particleColor2,
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
    }

    public LegacyFeatherDashSwitch(EntityData data, Vector2 offset, EntityID id)
        : this(id, data.Position + offset, data.Enum("side", Sides.Up),
            data.Bool("persistent"), data.Bool("allGates"),
            data.Attr("spriteDir"), data.HexColor("particleColor1"), data.HexColor("particleColor2"))
    { }

    private static Sprite BuildSprite(string spriteDir)
    {
        Sprite sprite = new(GFX.Game, spriteDir);
            
        // <Loop id="idle" path="" delay="0.08" frames="0-20"/>
        sprite.AddLoop("idle", "", 0.08f, Enumerable.Range(0, 21).ToArray());
        // <Loop id="pushed" path="" delay="0.08" frames="27"/>
        sprite.AddLoop("pushed", "", 0.08f, 27);
        // <Anim id="push" path="" delay="0.07" frames="21-27" goto="pushed"/>
        sprite.Add("push", "", 0.07f, "pushed", Enumerable.Range(21, 7).ToArray());
        
        return sprite;
    }

    private void OnFeatherHit(Vector2 direction)
    {
        if (Scene.Tracker.GetEntity<Player>() is not { } player) 
            return;

        if (!pressed && Vector2.Dot(direction, pressDirection) > 0)
        {
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
            
            sprite.Play("push");
            pressed = true;
            
            MoveTo(pressedTarget);
            Collidable = false;
            Position -= pressDirection * 2f;

            Vector2 particlePos = Position + sprite.Position;
            Vector2 particleSpread = direction.Perpendicular() * 6f;
            float particleRot = sprite.Rotation - (float) Math.PI;
            SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, particlePos, particleSpread, particleRot);
            SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, particlePos, particleSpread, particleRot);
        }
        
        if (allGates)
            foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>()
                                                       .Cast<TempleGate>()
                                                       .Where(entity => entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level))
                entity.SwitchOpen();
        else
            GetGate()?.SwitchOpen();
        
        if (persistent)
            SceneAs<Level>().Session.SetFlag(FlagName);

        // make it work with crystalline all dash switch temple gates
        OnDashCollide(player, direction);
    }

    private static new DashCollisionResults OnDashed(Player player, Vector2 direction)
        => DashCollisionResults.NormalCollision;
    
    #region Hooks
    
    internal static void Load()
    {
        IL.Celeste.Player.OnCollideH += Player_OnCollideH;
        IL.Celeste.Player.OnCollideV += Player_OnCollideV;
    }

    internal static void Unload()
    {
        IL.Celeste.Player.OnCollideH -= Player_OnCollideH;
        IL.Celeste.Player.OnCollideV -= Player_OnCollideV;
    }

    private static void Player_OnCollideH(ILContext il)
    {
        ILCursor cursor = new(il);
        
        if (!cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("starFlyTimer")))
            throw new HookHelper.HookException(il, "Unable to find reference to `Player.starFlyTimer`.");
        
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(CheckOnFeather);
    }

    private static void Player_OnCollideV(ILContext il)
    {
        ILCursor cursor = new(il);
        
        if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("starFlyTimer")))
            throw new HookHelper.HookException(il, "Unable to find reference to `Player.starFlyTimer`.");
        
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(CheckOnFeather);
    }

    private static void CheckOnFeather(CollisionData data)
    {
        switch (data.Hit)
        {
            case null:
                return;
            
            case LegacyFeatherDashSwitch dashSwitch:
                dashSwitch.OnFeatherHit(data.Direction);
                break;
        }
    }
    
    #endregion
}