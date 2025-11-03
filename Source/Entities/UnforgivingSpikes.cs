using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity(
    "aonHelper/UnforgivingSpikesUp = LoadUp",
    "aonHelper/UnforgivingSpikesDown = LoadDown",
    "aonHelper/UnforgivingSpikesLeft = LoadLeft",
    "aonHelper/UnforgivingSpikesRight = LoadRight"
)]
[Tracked]
public class UnforgivingSpikes : Spikes
{
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Up);
    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Down);
    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Left);
    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new UnforgivingSpikes(entityData, offset, Directions.Right);

    public UnforgivingSpikes(EntityData data, Vector2 offset, Directions dir)
        : base(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"))
    {
        Remove(Get<PlayerCollider>());
        Add(pc = new PlayerCollider(OnCollide));
    }

    private new void OnCollide(Player player)
    {
        switch (Direction)
        {
            case Directions.Up:
                player.Die(-Vector2.UnitY);
                break;
            
            case Directions.Down:
                player.Die(Vector2.UnitY);
                break;
            
            case Directions.Left:
                player.Die(-Vector2.UnitX);
                break;
            
            case Directions.Right:
                player.Die(Vector2.UnitX);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    #region Hooks

    // TODO: maybe make these lazy loaded (a la GravityHelper)
    
    internal static void Load()
    {
        IL.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Actor_MoveVExact;
    }

    internal static void Unload()
    {
        IL.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
    }

    private static void Actor_MoveHExact(ILContext il) => UnforgivingSpikesCheck(new ILCursor(il), true);
    private static void Actor_MoveVExact(ILContext il) => UnforgivingSpikesCheck(new ILCursor(il), false);

    private static void UnforgivingSpikesCheck(ILCursor cursor, bool horizontal)
    { 
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdloc3(),
            instr => instr.MatchBrfalse(out ILLabel _)))
            return;

        ILLabel afterRet = cursor.DefineLabel();
        
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdcI4(horizontal ? 1 : 0);
        cursor.EmitDelegate(CheckForUnforgivingSpikes);
        cursor.EmitBrfalse(afterRet);
        cursor.EmitLdcI4(0); // return false since we didn't collide with a solid
        cursor.EmitRet();
        cursor.MarkLabel(afterRet);
        
        return;

        static bool CheckForUnforgivingSpikes(Actor actor, int move, bool horizontal)
        {
            if (actor is not Player player)
                return false;

            Vector2 checkDir = horizontal ? Vector2.UnitX : Vector2.UnitY;
            if (player.CollideFirst<UnforgivingSpikes>(player.Position + checkDir * Math.Sign(move)) is not { } spikes)
                return false;
            
            spikes.OnCollide(player);
            return true;
        }
    }
    
    #endregion
}