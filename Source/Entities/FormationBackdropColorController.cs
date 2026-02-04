using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FormationBackdropColorController")]
[Tracked]
public class FormationBackdropColorController(Vector2 position, Color color, float alpha) : Entity(position)
{
    private readonly Color color = color;
    private readonly float alpha = alpha;
    
    public FormationBackdropColorController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.HexColor("color", Color.Black), data.Float("alpha", 0.85f))
    { }
    
    #region Hooks

    internal static void Load()
    {
        IL.Celeste.FormationBackdrop.Render += FormationBackdrop_Render;
    }

    internal static void Unload()
    {
        IL.Celeste.FormationBackdrop.Render -= FormationBackdrop_Render;
    }

    private static void FormationBackdrop_Render(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCall<Color>("get_Black")))
            throw new HookHelper.HookException(il, "Unable to find reference to `Color.Black` to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ChangeFormationBackdropColor);

        if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdcR4(0.85f)))
            throw new HookHelper.HookException(il, "Unable to find reference to `0.85f` to modify.");
        
        cursor.EmitLdarg0();
        cursor.EmitDelegate(ChangeFormationBackdropAlpha);
        
        return;

        static Color ChangeFormationBackdropColor(Color orig, FormationBackdrop backdrop)
            => backdrop.Scene.Tracker.GetEntity<FormationBackdropColorController>() is { color: var c } ? c : orig;
        
        static float ChangeFormationBackdropAlpha(float orig, FormationBackdrop backdrop)
            => backdrop.Scene.Tracker.GetEntity<FormationBackdropColorController>() is { alpha: var a } ? a : orig;
    }
    
    #endregion
}
