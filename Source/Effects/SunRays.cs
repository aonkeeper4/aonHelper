using Microsoft.Xna.Framework;
using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Celeste.Mod.aonHelper.Effects
{
    [CustomBackdrop("aonHelper/SunRays")]
    public class SunRays : Backdrop
    {
        private class SegmentLoop(Vector2[] points)
        {
            private Vector2[] points = points;

            private float[] segmentLengths;

            private bool looping;
        }

        private class SunRay
        {
            private SunRays parent;

            private SegmentLoop current;

            private float speed;

            private Vector2 start, end;
            private List<Vector2> corners;

            private VertexPositionColorTexture[] verts;

            private float alpha;
        }

        // in LevelLoadingThread we store all segment loops that rays can travel on depending on if idk useBgTileHoles, useFgTopHoles, others, user-defined segment loop entity?
        // for rays from the top, we place a corner at every place where the tiletype toggles from air to a solid + some y offset so they start in the ceiling
        // for rays coming from background tile holes, we use like a floodfill algorithm? to determine all the corners of the bg tile holes (im sure this is doable), then order them clockwise somehow?? todo: research how to do this
        // rays can travel along segment loops:
        // rays have start and end of a line segment defining where the ray shines out of, however if the start and end points are on different segments in the segment loop we add the corners the ray passes through to a list to keep track of the quads we need to draw
        // every Update, we move the start and end points along by the speed of the ray (do we want the rays to change length?) and check if start and end ar eon different segment indices - if so, we add the corners to the corner list
        // we also update the ray origin position by Scroll and the camera pos like parallax stylegrounds and wiggle the alpha and length of the rays somehow
        // render the rays:
        // loop over each consecutive pair of points formed by start, corners, end
        // find the other two corners of the quad by casting a ray out in the direction from the origin to the starting point up to a user-specified distance from the two origin points + record distance ray travels
        // interpolate texture coords to go from the very end of the ray texture when the distance the ray travelled is the maximum to the beginning when its zero - this should give us uniform texture behaviour when colliding with solids
        // draw quad using specified lightbeam texture + VertexPositionColorTexture[] to a texture (? that might be   slow as fuck) then mask out decals (limiting this to decals only) that occlude the rays determined by probably a custom decal registry attribute (luckily the masking part shouldnt be too expensive)
        // we should probably also precompute the occluding decal mask thing per room like with the segment loops
        // draw the texture to both main target (see how cloudscape does it)
        // blur + grayscale (does grayscaling type matter here at all? probably not) the texture before drawing it to bloom target (GameplayBuffers.TempA i think) 
        // might need to implement perspective correction on the texture (https://gamedev.stackexchange.com/questions/68021/how-can-i-draw-a-perspective-correct-quad)
    }
}