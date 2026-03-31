using Celeste.Mod.aonHelper.Entities.Misc;

namespace Celeste.Mod.aonHelper.Components;

[Tracked]
public class UnforgivingPlayerCollider(UnforgivingPlayerCollider.CollisionHandler onCollide, Collider collider = null) : Component(false, false)
{
    private static readonly string[] EntitySIDPrefixes = [UnforgivingSpikes.EntitySIDPrefix];

    public delegate bool CollisionHandler(Player player, Vector2 moveDir);

    private bool Check(Player player, Vector2 moveDir, Vector2? checkPosition = null)
    {
        Collider entityCollider = Entity.Collider;
        if (collider is not null)
            Entity.Collider = collider;
        
        bool result = checkPosition is { } position
            ? player.CollideCheck(Entity, position)
            : player.CollideCheck(Entity);
        
        Entity.Collider = entityCollider;
        
        return result && onCollide(player, moveDir);
    }
    
    #region Hooks
    
    [OnLoad]
    internal static void Load()
        => HookHelper.HookLazyLoadingManager.Register(nameof(UnforgivingPlayerCollider), ShouldLazyLoad, LazyLoad, LazyUnload);

    private static bool ShouldLazyLoad(MapData mapData)
        => mapData.Levels.SelectMany(levelData => levelData.Entities)
                         .Any(entityData => EntitySIDPrefixes.Any(prefix => entityData.Name.StartsWith(prefix)));
    
    private static void LazyLoad()
    {
        IL.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Actor_MoveVExact;
    }

    private static void LazyUnload()
    {
        IL.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
    }

    private static void Actor_MoveHExact(ILContext il) => UnforgivingSpikesCheck(il, true);
    private static void Actor_MoveVExact(ILContext il) => UnforgivingSpikesCheck(il, false);

    private static void UnforgivingSpikesCheck(ILContext il, bool horizontal)
    {
        ILCursor cursor = new(il);
        
        // i don't believe this can work with just a `dup` :disappointed_relieved:
        VariableDefinition checkPosition = new(il.Import(typeof(Vector2)));
        il.Body.Variables.Add(checkPosition);

        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchCall<Entity>("CollideFirst"),
            instr => instr.MatchStloc3()))
            throw new HookHelper.HookException(il, "Unable to find call to `Entity.CollideFirst` to insert local variable assignment before.");
        
        cursor.EmitStloc(checkPosition);
        cursor.EmitLdloc(checkPosition);

        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdloc3(),
            instr => instr.MatchBrfalse(out _)))
            throw new HookHelper.HookException(il, "Unable to find null check for `solid` to insert check for Unforgiving Spikes before.");

        ILLabel afterRet = cursor.DefineLabel();
        
        cursor.EmitLdarg0();
        cursor.EmitLdloc(checkPosition);
        cursor.EmitLdarg1();
        cursor.EmitLdcI4(horizontal ? 1 : 0);
        cursor.EmitDelegate(CheckForUnforgivingSpikes);
        cursor.EmitBrfalse(afterRet);
        
        cursor.EmitLdcI4(0); // return false since we didn't collide with a solid
        cursor.EmitRet();
        cursor.MarkLabel(afterRet);
        
        return;

        static bool CheckForUnforgivingSpikes(Actor actor, Vector2 checkPosition, int moveAmount, bool horizontal)
        {
            if (actor is not Player player)
                return false;
            
            Vector2 moveDir = (horizontal ? Vector2.UnitX : Vector2.UnitY) * Math.Sign(moveAmount);

            Collider collider = player.Collider;
            player.Collider = player.hurtbox;
            
            if (player.Scene.Tracker.GetComponents<UnforgivingPlayerCollider>()
                                    .Cast<UnforgivingPlayerCollider>()
                                    .Any(upc => upc.Check(player, moveDir)))
            {
                player.Collider = collider;
                return true;
            }
            
            if (player.Collider == player.hurtbox) // inb4 clouds yells at me
                player.Collider = collider;

            return false;
        }
    }
    
    #endregion
}
