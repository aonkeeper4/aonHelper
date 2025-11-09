using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/QuantizeColorgradeController")]
[Tracked]
public class QuantizeColorgradeController(Vector2 position) : Entity(position)
{
    public QuantizeColorgradeController(EntityData data, Vector2 offset)
        : this(data.Position + offset)
    { }
    
    #region Hooks
    
    internal static void Load()
    {
        // guarantee hook order
        using (new DetourConfigContext(HookHelper.BeforeStyleMaskHelperDetourConfig).Use())
            IL.Celeste.Level.Render += Level_Render;
    }

    internal static void Unload()
    {
        IL.Celeste.Level.Render -= Level_Render;
    }

    private static void Level_Render(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0383: call class [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch Monocle.Draw::get_SpriteBatch()
         * IL_0388: ldc.i4.0
         * IL_0389: ldsfld class [FNA]Microsoft.Xna.Framework.Graphics.BlendState [FNA]Microsoft.Xna.Framework.Graphics.BlendState::AlphaBlend
         * IL_038e: ldsfld class [FNA]Microsoft.Xna.Framework.Graphics.SamplerState [FNA]Microsoft.Xna.Framework.Graphics.SamplerState::PointClamp
         * IL_0393: ldsfld class [FNA]Microsoft.Xna.Framework.Graphics.DepthStencilState [FNA]Microsoft.Xna.Framework.Graphics.DepthStencilState::Default
         * IL_0398: ldsfld class [FNA]Microsoft.Xna.Framework.Graphics.RasterizerState [FNA]Microsoft.Xna.Framework.Graphics.RasterizerState::CullNone
         * IL_039d: call class [FNA]Microsoft.Xna.Framework.Graphics.Effect Celeste.ColorGrade::get_Effect()
         * IL_03a2: ldloc.2
         * IL_03a3: callvirt instance void [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch::Begin(...)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchCall(typeof(Draw), "get_SpriteBatch"),
            instr => instr.MatchLdcI4(0),
            instr => instr.MatchLdsfld<BlendState>("AlphaBlend"),
            instr => instr.MatchLdsfld<SamplerState>("PointClamp"),
            instr => instr.MatchLdsfld<DepthStencilState>("Default"),
            instr => instr.MatchLdsfld<RasterizerState>("CullNone"),
            instr => instr.MatchCall(typeof(ColorGrade), "get_Effect"),
            instr => instr.MatchLdloc2(),
            instr => instr.MatchCallvirt<SpriteBatch>("Begin")))
            throw new HookHelper.HookException(il, "Unable to find `SpriteBatch.Begin` for colorgrade application to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(OverrideSamplerState);
        
        // no idea why stylemaskhelper matches on this part instead of just  after SpriteBatch.End but ok whatever
        /*
         * IL_0406: ldarg.0
         * IL_0407: ldfld class Celeste.Pathfinder Celeste.Level::Pathfinder
         * IL_040c: brfalse.s IL_0461
         */
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Level>("Pathfinder"),
            instr => instr.MatchBrfalse(out _)))
            throw new HookHelper.HookException(il, "Unable to find `SpriteBatch.End` for colorgrade application to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ResetSamplerState);

        return;
        
        static void OverrideSamplerState(Level level)
        {
            if (level.Tracker.GetEntity<QuantizeColorgradeController>() is null)
                return;
            
            Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            Engine.Graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
        }

        static void ResetSamplerState(Level level)
        {
            if (level.Tracker.GetEntity<QuantizeColorgradeController>() is null)
                return;
            
            Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Engine.Graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
        }
    }
    
    #endregion
}