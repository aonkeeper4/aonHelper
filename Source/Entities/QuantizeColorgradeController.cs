using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/QuantizeColorgradeController")]
[Tracked]
public class QuantizeColorgradeController(Vector2 position, string affectedColorgrades, QuantizeColorgradeController.Modes mode) : Entity(position)
{
    private readonly string[] affectedColorgrades = affectedColorgrades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private readonly bool affectAll = affectedColorgrades.Contains('*');

    public enum Modes
    {
        Quantize,
        Normalize,
        Both
    }
    private readonly Modes mode = mode;
    
    public QuantizeColorgradeController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("affectedColorgrades", "*"), data.Enum("mode", Modes.Quantize))
    { }

    private static Modes? ModeFor(MTexture colorgrade)
        => (Engine.Scene as Level)?.Tracker.GetEntities<QuantizeColorgradeController>()
                                           .Cast<QuantizeColorgradeController>()
                                           .FirstOrDefault(c => c.affectAll || c.affectedColorgrades.Contains(colorgrade.AtlasPath))?
                                           .mode;
    
    #region Hooks

    private static Hook hook_ColorGrade_get_Effect;
    
    internal static void Load()
    {
        IL.Celeste.ColorGrade.Set_MTexture_MTexture_float += ColorGrade_Set;
        
        hook_ColorGrade_get_Effect = new Hook(typeof(ColorGrade).GetMethod("get_Effect", HookHelper.Bind.PublicStatic)!, ColorGrade_get_Effect);
    }

    internal static void Unload()
    {
        IL.Celeste.ColorGrade.Set_MTexture_MTexture_float -= ColorGrade_Set;
        
        HookHelper.DisposeAndSetNull(ref hook_ColorGrade_get_Effect);
    }

    private static FieldInfo f_ColorGrade_from = typeof(ColorGrade).GetField("from", HookHelper.Bind.NonPublicStatic)!;
    private static FieldInfo f_ColorGrade_to = typeof(ColorGrade).GetField("to", HookHelper.Bind.NonPublicStatic)!;

    private static void ColorGrade_Set(ILContext il)
    {
        ILCursor cursor = new(il);
        
        GotoNextSetCurrentTechnique();
        cursor.EmitLdsfld(f_ColorGrade_from);
        cursor.EmitLdsfld(f_ColorGrade_from);
        cursor.EmitDelegate(SetCustomParameters);
        
        GotoNextSetCurrentTechnique();
        cursor.EmitLdsfld(f_ColorGrade_to);
        cursor.EmitLdsfld(f_ColorGrade_to);
        cursor.EmitDelegate(SetCustomParameters);
        
        GotoNextSetCurrentTechnique();
        cursor.EmitLdsfld(f_ColorGrade_from);
        cursor.EmitLdsfld(f_ColorGrade_to);
        cursor.EmitDelegate(SetCustomParameters);
        
        return;

        void GotoNextSetCurrentTechnique()
        {
            // IL_0089: callvirt instance void [FNA]Microsoft.Xna.Framework.Graphics.Effect::set_CurrentTechnique(...)
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Effect>("set_CurrentTechnique")))
                throw new HookHelper.HookException(il, "Unable to find effect technique assignment to insert custom parameter logic after.");
        }
        
        static void SetCustomParameters(MTexture from, MTexture to)
        {
            Modes? fromMode = ModeFor(from);
            Modes? toMode = ModeFor(to);
            
            ColorGrade.Effect.Parameters["from_quantization"]?.SetValue(fromMode is Modes.Quantize or Modes.Both ? 1f : 0f);
            ColorGrade.Effect.Parameters["to_quantization"]?.SetValue(toMode is Modes.Quantize or Modes.Both ? 1f : 0f);
            ColorGrade.Effect.Parameters["from_normalization"]?.SetValue(fromMode is Modes.Normalize or Modes.Both ? 1f : 0f);
            ColorGrade.Effect.Parameters["to_normalization"]?.SetValue(toMode is Modes.Normalize or Modes.Both ? 1f : 0f);
        }
    }

    private static Effect ColorGrade_get_Effect(Func<Effect> orig)
    {
        if (Engine.Scene is not Level level
            || level.Tracker.GetEntity<QuantizeColorgradeController>() is null
            || aonHelperGFX.FxQuantizedColorgrade is not { IsDisposed: false })
            return orig();

        return aonHelperGFX.FxQuantizedColorgrade;
    }
    
    #endregion
}