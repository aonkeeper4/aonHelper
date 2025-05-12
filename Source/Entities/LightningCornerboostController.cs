using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/LightningCornerboostController")]
[Tracked]
public class LightningCornerboostController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{ 
    private readonly ConditionalWeakTable<Lightning, Solid> lightningSolids = new();

    public override void Added(Scene scene)
    {
        if (scene.Tracker.GetEntities<LightningCornerboostController>().Count >= 1)
        {
            Logger.Warn(nameof(aonHelperModule), "tried to load LightningCornerboostController when one was already present!");
            RemoveSelf();
            return;
        }
        
        base.Added(scene);
    }

    #region Hooks

    private static ILHook ilHook_Lightning_Move;

    internal static void Load()
    {
        // entity hooks
        On.Monocle.Entity.Awake += Entity_Awake;
        On.Monocle.Entity.RemoveSelf += Entity_RemoveSelf;

        // lightning hooks
        On.Celeste.Lightning.ToggleCheck += Lightning_ToggleCheck;
        
        ilHook_Lightning_Move = new ILHook(typeof(Lightning).GetMethod("Move", BindingFlags.NonPublic | BindingFlags.Instance)!.GetStateMachineTarget()!, Lightning_Move);
    }

    internal static void Unload()
    {
        On.Monocle.Entity.Awake -= Entity_Awake;
        On.Monocle.Entity.RemoveSelf -= Entity_RemoveSelf;
        
        On.Celeste.Lightning.ToggleCheck -= Lightning_ToggleCheck;
        
        ilHook_Lightning_Move?.Dispose();
        ilHook_Lightning_Move = null;
    }

    private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
    {
        orig(self, scene);

        if (self is not Lightning lightning || scene.Tracker.GetEntity<LightningCornerboostController>() is not { } controller)
            return;

        Solid solid = new(lightning.Position + new Vector2(3f, 4f), lightning.Width - 4f, lightning.Height - 5f, safe: false);
        controller.lightningSolids.Add(lightning, solid);
        scene.Add(solid);
    }

    private static void Entity_RemoveSelf(On.Monocle.Entity.orig_RemoveSelf orig, Entity self)
    {
        if (self is Lightning lightning &&
            (lightning.Scene.Tracker.GetEntity<LightningCornerboostController>()?.lightningSolids?.TryGetValue(lightning, out Solid solid) ?? false))
        {
            solid!.RemoveSelf();
        }

        orig(self);
    }
    
    private static void Lightning_ToggleCheck(On.Celeste.Lightning.orig_ToggleCheck orig, Lightning self)
    {
        orig(self);
        
        if (!(self.Scene.Tracker.GetEntity<LightningCornerboostController>()?.lightningSolids?.TryGetValue(self, out Solid solid) ?? false) || solid is null)
            return;

        solid.Collidable = solid.Visible = self.InView();
    }

    private static void Lightning_Move(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<Entity>(nameof(Position))))
            return;

        cursor.EmitDup();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(SetSolidPosition);
    }

    private static void SetSolidPosition(Vector2 pos, Lightning lightning)
    {
        if (!(lightning.Scene.Tracker.GetEntity<LightningCornerboostController>()?.lightningSolids?.TryGetValue(lightning, out Solid solid) ?? false) || solid is null)
            return;
        
        solid.Position = pos + new Vector2(3f, 4f);
    }
    
    #endregion
}
