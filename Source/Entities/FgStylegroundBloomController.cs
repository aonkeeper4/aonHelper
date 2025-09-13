using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FgStylegroundBloomController")]
[Tracked]
public class FgStylegroundBloomController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    private readonly string bloomTag = data.Attr("bloomTag");

    #region Hooks
    
    private static readonly List<Action<bool>> BeforeForegroundRenderActions = [], AfterForegroundRenderActions = [];
    
    internal static void AddBeforeForegroundRenderAction(Action<bool> action)
        => BeforeForegroundRenderActions.Add(action);
    internal static void RemoveBeforeForegroundRenderAction(Action<bool> action)
        => BeforeForegroundRenderActions.Remove(action);
    
    internal static void AddAfterForegroundRenderAction(Action<bool> action)
        => AfterForegroundRenderActions.Add(action);
    internal static void RemoveAfterForegroundRenderAction(Action<bool> action)
        => AfterForegroundRenderActions.Remove(action);

    internal static string GetCurrentBloomTag(Level level)
        => level.Tracker.GetEntity<FgStylegroundBloomController>() is { } controller
            && !string.IsNullOrEmpty(controller.bloomTag)
                ? controller.bloomTag
                : null;

    private static void RenderForeground(Level level, bool applyBloom)
    {
        BeforeForegroundRenderActions.ForEach(action => action(applyBloom));
        level.Foreground.Render(level);
        AfterForegroundRenderActions.ForEach(action => action(applyBloom));
    }

    private const string StyleMaskHelperDetourConfigName = "StyleMaskHelper";
    
    private static FieldInfo f_GameplayBuffers_Level = typeof(GameplayBuffers).GetField("Level", HookHelper.Bind.PublicStatic)!;
    private static MethodInfo m_Level_Render = typeof(Level).GetMethod("Render", HookHelper.Bind.PublicInstance)!;

    private static ILHook ilHook_Level_Render;
    
    internal static void Load()
    {
        ilHook_Level_Render = new ILHook(m_Level_Render, Level_Render, HookHelper.GetDetourConfig(before: [StyleMaskHelperDetourConfigName]));
    }

    internal static void Unload()
    {
        HookHelper.DisposeAndSetNull(ref ilHook_Level_Render);
    }

    private static void Level_Render(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_00a4: ldarg.0
         * IL_00a5: ldfld class Celeste.BloomRenderer Celeste.Level::Bloom
         * IL_00aa: ldsfld class Monocle.VirtualRenderTarget Celeste.GameplayBuffers::Level
         * IL_00af: ldarg.0
         * IL_00b0: callvirt instance void Celeste.BloomRenderer::Apply(class Monocle.VirtualRenderTarget, class Monocle.Scene)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Level>("Bloom"),
            instr => instr.MatchLdsfld(f_GameplayBuffers_Level),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<BloomRenderer>("Apply")))
            throw new HookHelper.HookException(il, "Unable to find bloom application to modify.");
        
        ILLabel skipBloomRendering = cursor.DefineLabel();
        cursor.EmitLdarg0();
        cursor.EmitDelegate(CustomBloomRendering);
        cursor.EmitBrtrue(skipBloomRendering);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<BloomRenderer>("Apply"));
        cursor.MarkLabel(skipBloomRendering);
        cursor.EmitNop(); // to ensure our label is inserted before other people's additions
        
        /*
         * IL_00b5: ldarg.0
         * IL_00b6: ldfld class Celeste.BackdropRenderer Celeste.Level::Foreground
         * IL_00bb: ldarg.0
         * IL_00bc: callvirt instance void Monocle.Renderer::Render(class Monocle.Scene)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Level>("Foreground"),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Renderer>("Render")))
            throw new HookHelper.HookException(il, "Unable to find foreground styleground rendering to modify.");
        
        ILLabel skipForegroundRendering = cursor.DefineLabel();
        cursor.EmitLdarg0();
        cursor.EmitDelegate(CustomForegroundRendering);
        cursor.EmitBrtrue(skipForegroundRendering);
        
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Renderer>("Render"));
        cursor.MarkLabel(skipForegroundRendering);
        cursor.EmitNop(); // to ensure our label is inserted before other people's additions

        return;
        
        static bool CustomBloomRendering(Level level)
            => level.Tracker.GetEntity<FgStylegroundBloomController>() is not null;
        
        static bool CustomForegroundRendering(Level level)
        {
            if (level.Tracker.GetEntity<FgStylegroundBloomController>() is not { } controller)
                return false;

            if (string.IsNullOrEmpty(controller.bloomTag))
            {
                RenderForeground(level, true);
                level.Bloom.Apply(GameplayBuffers.Level, level);
            }
            else
            {
                List<Backdrop> all = level.Foreground.Backdrops;
                List<Backdrop> affected = level.Foreground.GetEach<Backdrop>(controller.bloomTag).ToList();
                List<Backdrop> unaffected = level.Foreground.Backdrops.Except(affected).ToList();
                
                // i really don't like assigning to level.Foreground.Backdrops every 2 seconds but  ehh
                level.Foreground.Backdrops = affected;
                RenderForeground(level, true);
                level.Bloom.Apply(GameplayBuffers.Level, level);
                
                level.Foreground.Backdrops = unaffected;
                RenderForeground(level, false);
                
                level.Foreground.Backdrops = all;
            }

            return true;
        }
    }
    
    #endregion
}