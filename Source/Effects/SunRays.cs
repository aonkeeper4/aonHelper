using Microsoft.Xna.Framework;
using Celeste.Mod.Backdrops;

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

            private Vector2 start, end, mid;

            private float alpha;
        }
    }
}