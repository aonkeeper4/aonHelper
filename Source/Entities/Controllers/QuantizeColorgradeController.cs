using System.Reflection;

namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/QuantizeColorgradeController")]
[Tracked]
public class QuantizeColorgradeController(
    Vector2 position,
    string affectedColorgrades,
    bool quantize, bool normalize) : Controller(position)
{
    private readonly string[] affectedColorgrades = affectedColorgrades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private readonly bool affectAll = affectedColorgrades.Contains('*');

    private readonly bool quantize = quantize, normalize = normalize;
    
    public QuantizeColorgradeController(EntityData data, Vector2 offset)
        : this(
            data.Position + offset,
            data.Attr("affectedColorgrades", "*"),
            data.Bool("quantize", data.Int("mode") is 0 or 2), data.Bool("normalize", data.Int("mode") is 1 or 2))
    { }

    private static (bool, bool)? OptionsFor(MTexture colorgrade)
    {
        if ((Engine.Scene as Level)?.Tracker.GetEntities<QuantizeColorgradeController>()
                                            .Cast<QuantizeColorgradeController>()
                                            .FirstOrDefault(c => c.affectAll || c.affectedColorgrades.Contains(colorgrade.AtlasPath))
            is { } controller)
            return (controller.quantize, controller.normalize);
        
        return null;
    }
    
    #region Hooks

    private static Hook on_ColorGrade_get_Effect;
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.ColorGrade.Set_MTexture_MTexture_float += IL_ColorGrade_Set;
        
        on_ColorGrade_get_Effect = new Hook(typeof(ColorGrade).GetMethod("get_Effect", HookHelper.Bind.PublicStatic)!, On_ColorGrade_get_Effect);
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.ColorGrade.Set_MTexture_MTexture_float -= IL_ColorGrade_Set;
        
        HookHelper.DisposeAndSetNull(ref on_ColorGrade_get_Effect);
    }

    private static FieldInfo f_ColorGrade_from = typeof(ColorGrade).GetField("from", HookHelper.Bind.NonPublicStatic)!;
    private static FieldInfo f_ColorGrade_to = typeof(ColorGrade).GetField("to", HookHelper.Bind.NonPublicStatic)!;

    private static void IL_ColorGrade_Set(ILContext il)
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
            (bool quantize, bool normalize)? optionsFrom = OptionsFor(from);
            (bool quantize, bool normalize)? optionsTo = OptionsFor(to);
            
            ColorGrade.Effect.Parameters["from_quantization"]?.SetValue(optionsFrom?.quantize is true ? 1f : 0f);
            ColorGrade.Effect.Parameters["to_quantization"]?.SetValue(optionsTo?.quantize is true ? 1f : 0f);
            ColorGrade.Effect.Parameters["from_normalization"]?.SetValue(optionsFrom?.normalize is true ? 1f : 0f);
            ColorGrade.Effect.Parameters["to_normalization"]?.SetValue(optionsTo?.normalize is true ? 1f : 0f);
        }
    }

    private delegate Effect orig_ColorGrade_get_Effect();
    private static Effect On_ColorGrade_get_Effect(orig_ColorGrade_get_Effect orig)
    {
        if (Engine.Scene is not Level level
            || level.Tracker.GetEntity<QuantizeColorgradeController>() is null
            || aonHelperGFX.FxQuantizedColorgrade is not { IsDisposed: false })
            return orig();

        return aonHelperGFX.FxQuantizedColorgrade;
    }
    
    #endregion
}