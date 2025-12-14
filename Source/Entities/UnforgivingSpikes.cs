using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities;

// todo: check whether these work in all cases with moving blocks

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

    private readonly bool checkMoveDirection;

    public UnforgivingSpikes(EntityData data, Vector2 offset, Directions dir)
        : base(data, offset, dir)
    {
        checkMoveDirection = data.Bool("checkMoveDirection");
        
        Remove(Get<PlayerCollider>());
    }

    private bool OnMoveCollision(Player player, Vector2 moveDir)
    {
        switch (Direction)
        {
            case Directions.Up when !checkMoveDirection || moveDir.Y > 0f:
                player.Die(-Vector2.UnitY);
                return true;
            
            case Directions.Down when !checkMoveDirection || moveDir.Y < 0f:
                player.Die(Vector2.UnitY);
                return true;
            
            case Directions.Left when !checkMoveDirection || moveDir.X > 0f:
                player.Die(-Vector2.UnitX);
                return true;
            
            case Directions.Right when !checkMoveDirection || moveDir.X < 0f:
                player.Die(Vector2.UnitX);
                return true;
            
            default:
                return false;
        }
    }
    
    #region Hooks

    // todo: make these lazy loaded
    
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

    private static void Actor_MoveHExact(ILContext il) => UnforgivingSpikesCheck(il, true);
    private static void Actor_MoveVExact(ILContext il) => UnforgivingSpikesCheck(il, false);

    private static void UnforgivingSpikesCheck(ILContext il, bool horizontal)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchCall<Entity>("CollideFirst"),
            instr => instr.MatchStloc3()))
            throw new HookHelper.HookException(il, "Unable to find call to `Entity.CollideFirst` to insert Unforgiving Spikes check before.");

        ILLabel afterRet = cursor.DefineLabel();

        cursor.EmitDup();
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitLdcI4(horizontal ? 1 : 0);
        cursor.EmitDelegate(CheckForUnforgivingSpikes);
        cursor.EmitBrfalse(afterRet);
        cursor.EmitLdcI4(0); // return false since we didn't collide with a solid
        cursor.EmitRet();
        cursor.MarkLabel(afterRet);
        
        Logger.Info(nameof(aonHelper) + "/unforgiving spieks", il.ToString());
        
        return;

        static bool CheckForUnforgivingSpikes(Vector2 checkPosition, Actor actor, int moveAmount, bool horizontal)
        {
            if (actor is not Player player)
                return false;

            Collider collider = player.Collider;
            player.Collider = player.hurtbox;
            
            UnforgivingSpikes spikes = player.CollideFirst<UnforgivingSpikes>(checkPosition);
            
            player.Collider = collider;
            
            if (spikes is null)
                return false;
            
            Vector2 moveDir = (horizontal ? Vector2.UnitX : Vector2.UnitY) * Math.Sign(moveAmount);
            return spikes.OnMoveCollision(player, moveDir);
        }
    }
    
    #endregion
}