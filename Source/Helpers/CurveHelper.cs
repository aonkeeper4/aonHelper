namespace Celeste.Mod.aonHelper.Helpers;

// mostly copied from https://github.com/CommunalHelper/CommunalHelper/blob/dev/src/Utils/BakedCurve.cs
public static class CurveHelper
{
    // quadratic bezier
    public static Vector2 GetCurvePoint(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float t2 = t * t;
        float mt = 1f - t;
        float mt2 = mt * mt;

        return mt2 * a + 2f * mt * t * b + t2 * c;
    }
    public static Vector2 GetCurveDerivative(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float fa = 2f * t - 2f;
        float fb = -4f * t + 2f;
        float fc = 2f * t;

        return fa * a + fb * b + fc * c;
    }

    // cubic bezier
    public static Vector2 GetCurvePoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float mt = 1f - t;
        float mt2 = mt * mt;
        float mt3 = mt2 * mt;

        return mt3 * a + 3f * mt2 * t * b + 3f * mt * t2 * c + t3 * d;
    }
    public static Vector2 GetCurveDerivative(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        float tm = t - 1f;
        float threetm1 = 3f * t - 1f;

        float fa = -3f * tm * tm;
        float fb = 3f * tm * threetm1;
        float fc = 1f - threetm1 * threetm1;
        float fd = 3f * t * t;

        return fa * a + fb * b + fc * c + fd * d;
    }

    public class BakedCurve
    {
        private readonly Vector2[] points;

        // t -> distance
        private readonly float[] lut;
        private readonly int resolution;

        public enum CurveType {
            Quadratic,
            Cubic
        }
        private readonly CurveType mode;

        public readonly int CurveCount;
        public readonly float Length;

        public BakedCurve(Vector2[] nodes, CurveType mode, int resolution)
        {
            this.mode = mode;
            this.resolution = resolution;

            int m = (int) mode;
            int l = nodes.Length - 1;
            int max = l - l % m;
            CurveCount = max / m;

            points = new Vector2[max + 1];
            for (int i = 0; i <= max; i++)
                points[i] = nodes[i];

            lut = new float[CurveCount * resolution + 1];
            lut[0] = 0f;
            float step = 1f / resolution;
            Vector2 prev = points[0];
            int index = 0;
            switch (mode)
            {
                default:
                case CurveType.Quadratic:
                    for (float t = step; t < CurveCount; t += step)
                    {
                        float percent = t % 1f;
                        int i = (int) MathF.Floor(t) * 2;

                        Vector2 p = GetCurvePoint(points[i], points[i + 1], points[i + 2], percent);
                        lut[++index] = Length += Vector2.Distance(prev, p);
                        prev = p;
                    }
                    break;

                case CurveType.Cubic:
                    for (float t = step; t < CurveCount; t += step)
                    {
                        float percent = t % 1f;
                        int i = (int) MathF.Floor(t) * 3;

                        Vector2 p = GetCurvePoint(points[i], points[i + 1], points[i + 2], points[i + 3], percent);
                        lut[++index] = Length += Vector2.Distance(prev, p);
                        prev = p;
                    }
                    break;
            }
            Length += Vector2.Distance(prev, points[^1]);
            lut[CurveCount * resolution] = Length;
        }

        public Vector2 GetPoint(float t)
        {
            int m = (int) mode;
            float percent = t % 1f;
            int i = Math.Max(0, (int) MathF.Floor(t) * m);
            if (t >= CurveCount)
            {
                percent++;
                i -= m;
            }

            return mode == CurveType.Cubic ?
                GetCurvePoint(points[i], points[i + 1], points[i + 2], points[i + 3], percent) :
                GetCurvePoint(points[i], points[i + 1], points[i + 2], percent);
        }
        public Vector2 GetDerivative(float t)
        {
            int m = (int) mode;
            float percent = t % 1f;
            int i = Math.Max(0, (int) MathF.Floor(t) * m);
            if (t >= CurveCount)
            {
                percent++;
                i -= m;
            }

            return mode == CurveType.Cubic ?
                GetCurveDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], percent) :
                GetCurveDerivative(points[i], points[i + 1], points[i + 2], percent);
        }
        public void GetAll(float t, out Vector2 point, out Vector2 derivative)
        {
            int m = (int) mode;
            float percent = t % 1f;
            int i = Math.Max(0, (int) MathF.Floor(t) * m);
            if (t >= CurveCount)
            {
                percent++;
                i -= m;
            }

            if (mode == CurveType.Cubic)
            {
                point = GetCurvePoint(points[i], points[i + 1], points[i + 2], points[i + 3], percent);
                derivative = GetCurveDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], percent);
            }
            else
            {
                point = GetCurvePoint(points[i], points[i + 1], points[i + 2], percent);
                derivative = GetCurveDerivative(points[i], points[i + 1], points[i + 2], percent);
            }
        }

        public Vector2 GetPointByDistance(float distance)
            => GetPoint(SolveForTGivenDistance(distance));
        public Vector2 GetDerivativeByDistance(float distance)
            => GetDerivative(SolveForTGivenDistance(distance));
        public void GetAllByDistance(float distance, out Vector2 point, out Vector2 derivative)
            => GetAll(SolveForTGivenDistance(distance), out point, out derivative);

        public float SolveForTGivenDistance(float distance)
        {
            distance = Calc.Clamp(distance, 0f, Length);
            float t = 0f;
            for (int i = 0; i < lut.Length; i++)
            {
                if (lut[i] == distance)
                {
                    t = i;
                    break;
                }

                if (lut[i] > distance)
                {
                    int j = i - 1;
                    float dA = lut[i - 1],
                        dJ = lut[i];
                    t = j + (i - j) * (distance - dA) / (dJ - dA);
                    break;
                }
            }

            return t / resolution;
        }
    }
}
