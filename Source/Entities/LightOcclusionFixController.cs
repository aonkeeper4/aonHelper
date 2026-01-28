using Celeste.Mod.aonHelper.Helpers;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/LightOcclusionFixController")]
[Tracked]
public class LightOcclusionFixController(Vector2 position, char[] noOcclusionTileTypes) : Entity(position)
{
	private readonly char[] noOcclusionTileTypes = noOcclusionTileTypes; 
	
    public LightOcclusionFixController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("noOcclusionTileTypes").ToArray())
    { }
    
    #region Hooks

    internal static void Load()
    {
        IL.Celeste.LightingRenderer.DrawLightOccluders += LightingRenderer_DrawLightOccluders;
    }

    internal static void Unload()
    {
        IL.Celeste.LightingRenderer.DrawLightOccluders -= LightingRenderer_DrawLightOccluders;
    }

    private static void LightingRenderer_DrawLightOccluders(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0346: ldloca.s 8
         * IL_0348: call instance int32 [FNA]Microsoft.Xna.Framework.Rectangle::get_Left()
         * IL_034d: ldc.i4.8
         * IL_034e: div
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdloca(8),
            instr => instr.MatchCall<Rectangle>("get_Left"),
            instr => instr.MatchLdcI4(8),
            instr => instr.MatchDiv()))
            throw new HookHelper.HookException(il, "Unable to find tile occlusion rendering to modify.");

        ILLabel afterTileOcclusionRendering = cursor.DefineLabel();
        
        cursor.EmitLdarg0(); // `this`
        cursor.EmitLdarg2(); // `level`
        cursor.EmitLdloc0(); // `tileBounds`
        cursor.EmitLdloc(7); // `light`
        cursor.EmitLdloc(8); // `rectangle`
        cursor.EmitLdloc(9); // `center`
        cursor.EmitLdloc(10); // `mask`
        cursor.EmitDelegate(ShouldSkipTileOcclusionRendering);
        cursor.EmitBrtrue(afterTileOcclusionRendering);

        /*
         * IL_074c: ldloc.2
         * IL_074d: callvirt instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<class Monocle.Component>::GetEnumerator()
         * IL_0752: stloc.3
         */
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdloc2(),
            instr => instr.MatchCallvirt<List<Component>>("GetEnumerator"),
            instr => instr.MatchStloc3()))
            throw new HookHelper.HookException(il, "Unable to find effect cutout occlusion rendering to skip to.");

        cursor.MarkLabel(afterTileOcclusionRendering);

        return;

        static bool ShouldSkipTileOcclusionRendering(
            LightingRenderer renderer,
            Level level, Rectangle tileBounds,
            Vector2 lightPos, Rectangle lightBounds,
            Vector3 drawnLightCenter, Color maskColor)
        {
            if (level.Tracker.GetEntity<LightOcclusionFixController>() is not { } controller)
                return false;
            
            int leftTileOffset = (int) MathF.Floor(lightBounds.Left / 8f) - tileBounds.Left, topTileOffset = (int) MathF.Floor(lightBounds.Top / 8f) - tileBounds.Top;
			int rightTileOffset = (int) MathF.Ceiling(lightBounds.Right / 8f) - tileBounds.Left, bottomTileOffset = (int) MathF.Ceiling(lightBounds.Bottom / 8f) - tileBounds.Top;
			int tilesWidth = rightTileOffset - leftTileOffset, tilesHeight = bottomTileOffset - topTileOffset;

			// top edges
			for (int tileY = topTileOffset; tileY < topTileOffset + tilesHeight / 2; tileY++)
			{
				for (int tileX = leftTileOffset; tileX < rightTileOffset; tileX++)
				{
					if (!HasOcclusionAt(tileX, tileY) || HasOcclusionAt(tileX, tileY + 1))
						continue;
					
					int startOcclusionTileX = tileX, endOcclusionTileX = tileX;
					do
						endOcclusionTileX++;
					while (endOcclusionTileX < rightTileOffset
						&& HasOcclusionAt(endOcclusionTileX, tileY)
						&& !HasOcclusionAt(endOcclusionTileX, tileY + 1));

					int occlusionTileY = tileBounds.Y + tileY + 1;
					Vector2 occlusionEdgeStart = new Vector2(tileBounds.X + startOcclusionTileX, occlusionTileY) * 8f;
					Vector2 occlusionEdgeEnd = new Vector2(tileBounds.X + endOcclusionTileX, occlusionTileY) * 8f;
					
					renderer.SetOccluder(drawnLightCenter, maskColor, lightPos, occlusionEdgeStart, occlusionEdgeEnd);
				}
			}
			
			// left edges
			for (int tileX = leftTileOffset; tileX < leftTileOffset + tilesWidth / 2; tileX++)
			{
				for (int tileY = topTileOffset; tileY < bottomTileOffset; tileY++)
				{
					if (!HasOcclusionAt(tileX, tileY) || HasOcclusionAt(tileX + 1, tileY))
						continue;
					
					int startOcclusionTileY = tileY, endOcclusionTileY = tileY;
					do
						endOcclusionTileY++;
					while (endOcclusionTileY < bottomTileOffset
						&& HasOcclusionAt(tileX, endOcclusionTileY)
						&& !HasOcclusionAt(tileX + 1, endOcclusionTileY));

					int occlusionTileX = tileBounds.X + tileX + 1;
					Vector2 occlusionEdgeStart = new Vector2(occlusionTileX, tileBounds.Y + startOcclusionTileY) * 8f;
					Vector2 occlusionEdgeEnd = new Vector2(occlusionTileX, tileBounds.Y + endOcclusionTileY) * 8f;
					
					renderer.SetOccluder(drawnLightCenter, maskColor, lightPos, occlusionEdgeStart, occlusionEdgeEnd);
				}
			}
			
			// bottom edges
			for (int tileY = bottomTileOffset - tilesHeight / 2; tileY < bottomTileOffset; tileY++)
			{
				for (int tileX = leftTileOffset; tileX < rightTileOffset; tileX++)
				{
					if (!HasOcclusionAt(tileX, tileY) || HasOcclusionAt(tileX, tileY - 1))
						continue;
					
					int startOcclusionTileX = tileX, endOcclusionTileX = tileX;
					do
						endOcclusionTileX++;
					while (endOcclusionTileX < rightTileOffset
						&& HasOcclusionAt(endOcclusionTileX, tileY)
						&& !HasOcclusionAt(endOcclusionTileX, tileY - 1));

					int occlusionTileY = tileBounds.Y + tileY;
					Vector2 occlusionEdgeStart = new Vector2(tileBounds.X + startOcclusionTileX, occlusionTileY) * 8f;
					Vector2 occlusionEdgeEnd = new Vector2(tileBounds.X + endOcclusionTileX, occlusionTileY) * 8f;
					
					renderer.SetOccluder(drawnLightCenter, maskColor, lightPos, occlusionEdgeStart, occlusionEdgeEnd);
				}
			}
			
			// right edges
			for (int tileX = rightTileOffset - tilesWidth / 2; tileX < rightTileOffset; tileX++)
			{
				for (int tileY = topTileOffset; tileY < bottomTileOffset; tileY++)
				{
					if (!HasOcclusionAt(tileX, tileY) || HasOcclusionAt(tileX - 1, tileY))
						continue;
					
					int startOcclusionTileY = tileY, endOcclusionTileY = tileY;
					do
						endOcclusionTileY++;
					while (endOcclusionTileY < bottomTileOffset
						&& HasOcclusionAt(tileX, endOcclusionTileY)
						&& !HasOcclusionAt(tileX - 1, endOcclusionTileY));

					int occlusionTileX = tileBounds.X + tileX;
					Vector2 occlusionEdgeStart = new Vector2(occlusionTileX, tileBounds.Y + startOcclusionTileY) * 8f;
					Vector2 occlusionEdgeEnd = new Vector2(occlusionTileX, tileBounds.Y + endOcclusionTileY) * 8f;
					
					renderer.SetOccluder(drawnLightCenter, maskColor, lightPos, occlusionEdgeStart, occlusionEdgeEnd);
				}
			}

            return true;

            bool HasOcclusionAt(int x, int y)
            {
	            char tile = level.SolidsData.SafeCheck(x, y);
	            return tile != '0' && !controller.noOcclusionTileTypes.Contains(tile);
            }
        }
    }
    
    #endregion
}
