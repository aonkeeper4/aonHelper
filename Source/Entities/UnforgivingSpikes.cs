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
    private class UnforgivingSpikesComponent() : Component(false, false)
    {
        public Vector2? PreviousExactPosition;

        public override void Added(Entity entity)
        {
            if (entity is not Player)
                throw new Exception($"{nameof(UnforgivingSpikesComponent)} added to non-{nameof(Player)} entity!");
            
            base.Added(entity);
        }
    }

    private readonly bool checkVelocity;
    
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
        checkVelocity = data.Bool("checkVelocity");
        
        Remove(Get<PlayerCollider>());
        Add(pc = new PlayerCollider(OnCollide));
    }

    private new void OnCollide(Player player)
    {
        Vector2? previousExactPosition = checkVelocity
            ? player.Get<UnforgivingSpikesComponent>()?.PreviousExactPosition
            : null;
        Vector2? velocity = previousExactPosition is { } previous
            ? player.ExactPosition - previous
            : null;
        
        switch (Direction)
        {
            case Directions.Up:
                if ((velocity?.Y ?? 1f) >= 0f)
                    player.Die(-Vector2.UnitY);
                break;
            
            case Directions.Down:
                if ((velocity?.Y ?? -1f) <= 0f)
                    player.Die(Vector2.UnitY);
                break;
            
            case Directions.Left:
                if ((velocity?.X ?? 1f) >= 0f)
                    player.Die(-Vector2.UnitX);
                break;
            
            case Directions.Right:
                if ((velocity?.X ?? -1f) <= 0f)
                    player.Die(-Vector2.UnitX);
                break;
            
            default:
                throw new Exception($"collided with {nameof(UnforgivingSpikes)} with an unknown {nameof(Direction)}!");
        }
    }
    
    #region Hooks

    // TODO: maybe make these lazy loaded (a la GravityHelper)
    
    internal static void Load()
    {
        Everest.Events.Player.OnSpawn += OnSpawn;
        Everest.Events.Player.OnAfterUpdate += OnAfterUpdate;
        Everest.Events.AssetReload.OnBeforeReload += OnBeforeReload;
        
        IL.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Actor_MoveVExact;
    }

    internal static void Unload()
    {
        Everest.Events.Player.OnSpawn -= OnSpawn;
        Everest.Events.Player.OnAfterUpdate += OnAfterUpdate;
        Everest.Events.AssetReload.OnBeforeReload -= OnBeforeReload;
        
        IL.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
    }

    private static void OnSpawn(Player player)
    {
        if (player.Get<UnforgivingSpikesComponent>() is null)
            player.Add(new UnforgivingSpikesComponent());
    }

    private static void OnAfterUpdate(Player player)
    {
        if (player.Get<UnforgivingSpikesComponent>() is not { } component)
            return;

        component.PreviousExactPosition = player.ExactPosition;
    }
    
    private static void OnBeforeReload(bool silent)
    {
        if (Engine.Scene?.Tracker?.GetEntity<Player>() is { } player)
            player.Remove(player.Get<UnforgivingSpikesComponent>());
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