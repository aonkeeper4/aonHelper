using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FgStylegroundBloomController")]
[Tracked]
public class FgStylegroundBloomController : Entity
{
    private readonly string bloomTag;

    public FgStylegroundBloomController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        bloomTag = data.Attr("bloomTag");
    }

    public static void Load()
    {
        IL.Celeste.Level.Render += mod_LevelRender;
    }

    public static void Unload()
    {
        IL.Celeste.Level.Render -= mod_LevelRender;
    }

    private static void mod_LevelRender(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel normalBehavior = cursor.DefineLabel();
        ILLabel pastNormalBehavior = cursor.DefineLabel();
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt(typeof(BloomRenderer), "Apply"));
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Level), "Bloom"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(determineFgBloom);
        cursor.Emit(OpCodes.Brfalse, normalBehavior);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(customBloomBehavior);
        cursor.Emit(OpCodes.Br, pastNormalBehavior);
        cursor.GotoNext(instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Level), "Bloom"));
        cursor.MarkLabel(normalBehavior);
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld(typeof(Level), "Foreground"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt(typeof(Renderer), "Render"));
        cursor.MarkLabel(pastNormalBehavior);
    }

    private static bool determineFgBloom(Level level)
    {
        return level.Tracker.GetEntity<FgStylegroundBloomController>() is not null;
    }

    private static void customBloomBehavior(Level level)
    {
        if (level.Tracker.GetEntity<FgStylegroundBloomController>() is not FgStylegroundBloomController controller)
            return;

        if (controller.bloomTag != "")
        {
            // i really don't like assigning to level.Foreground.Backdrops every 2 seconds but  ehh
            List<Backdrop> all = level.Foreground.Backdrops;
            List<Backdrop> affected = level.Foreground.GetEach<Backdrop>(controller.bloomTag).ToList();
            List<Backdrop> unaffected = level.Foreground.Backdrops.Except(affected).ToList();
            level.Foreground.Backdrops = affected;
            level.Foreground.Render(level);
            level.Bloom.Apply(GameplayBuffers.Level, level);
            level.Foreground.Backdrops = unaffected;
            level.Foreground.Render(level);
            level.Foreground.Backdrops = all;
        }
        else
        {
            level.Foreground.Render(level);
            level.Bloom.Apply(GameplayBuffers.Level, level);
        }
    }
}