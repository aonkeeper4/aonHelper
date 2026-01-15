using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/QuantizeColorgradeController")]
[Tracked]
public class QuantizeColorgradeController(Vector2 position, string affectedColorGrades) : Entity(position)
{
    private readonly string[] affectedColorgrades = affectedColorGrades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private readonly bool affectAll = affectedColorGrades.Contains('*');
    
    public QuantizeColorgradeController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("affectedColorgrades", "*"))
    { }

    private bool ColorgradeAffected(MTexture texture)
        => affectAll || affectedColorgrades.Contains(texture.AtlasPath);
    
    #region Hooks

    private static Hook hook_ColorGrade_get_Effect;
    
    internal static void Load()
    {
        On.Celeste.ColorGrade.Set_MTexture_MTexture_float += ColorGrade_Set;
        
        hook_ColorGrade_get_Effect = new Hook(typeof(ColorGrade).GetMethod("get_Effect", HookHelper.Bind.PublicStatic)!, ColorGrade_get_Effect);
    }

    internal static void Unload()
    {
        On.Celeste.ColorGrade.Set_MTexture_MTexture_float -= ColorGrade_Set;
        
        HookHelper.DisposeAndSetNull(ref hook_ColorGrade_get_Effect);
    }

    private static void ColorGrade_Set(On.Celeste.ColorGrade.orig_Set_MTexture_MTexture_float orig, MTexture fromTex, MTexture toTex, float p)
    {
        orig(fromTex, toTex, p);
        
        if (Engine.Scene is not Level level
            || level.Tracker.GetEntity<QuantizeColorgradeController>() is not { } controller
            || aonHelperGFX.FxQuantizedColorgrade is null
            || aonHelperGFX.FxQuantizedColorgrade.IsDisposed)
            return;

        aonHelperGFX.FxQuantizedColorgrade.Parameters["from_filter"].SetValue(controller.ColorgradeAffected(fromTex) ? 1f : 0f);
        aonHelperGFX.FxQuantizedColorgrade.Parameters["to_filter"].SetValue(controller.ColorgradeAffected(toTex) ? 1f : 0f);
    }

    private static Effect ColorGrade_get_Effect(Func<Effect> orig)
    {
        if (Engine.Scene is not Level level
            || level.Tracker.GetEntity<QuantizeColorgradeController>() is null
            || aonHelperGFX.FxQuantizedColorgrade is null
            || aonHelperGFX.FxQuantizedColorgrade.IsDisposed)
            return orig();

        return aonHelperGFX.FxQuantizedColorgrade;
    }
    
    #endregion
}