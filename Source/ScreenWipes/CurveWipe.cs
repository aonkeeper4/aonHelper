namespace Celeste.Mod.aonHelper.ScreenWipes;

[CustomWipe("aonHelper/CurveWipe")]
public class CurveWipe : ScreenWipe
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(CurveWipe)}";

    public const int DefaultResolution = 16; // 540 verts for the default path
    public static readonly Vector2[] DefaultPoints = [
        new(-300, 540),
        new(150, 750),
        new(650, 750),
        new(960, 540),
        new(1270, 330),
        new(1770, 330),
        new(2220, 540)
    ];
    public const float DefaultFromHeight = 0f, DefaultToHeight = 500f;
    public static readonly aonHelperMetadata.CurveWipeSettingsData DefaultSettings = new() {
        Resolution = DefaultResolution,
        Points = DefaultPoints,
        FromHeight = DefaultFromHeight,
        ToHeight = DefaultToHeight
    };
    private readonly aonHelperMetadata.CurveWipeSettingsData settings;

    private static readonly BlendState SubtractBlendState = new() {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    private const float ExpandHeight = 1200f;
    private const float ExpandThreshold = 0.7f;
    private readonly CurveHelper.BakedCurve path;
    private readonly float lengthFactor;
    private readonly int numQuads;

    private readonly VertexPositionColor[] verts;

    private bool hasDrawn;

    public CurveWipe(Scene scene, bool wipeIn, Action onComplete = null) : this(scene, wipeIn, DeserializeSettingsFromMetadata(scene), onComplete)
    { }

    private static aonHelperMetadata.CurveWipeSettingsData DeserializeSettingsFromMetadata(Scene scene)
    {
        AreaKey? areaKey = scene switch {
            Level level => level.Session.Area,
            Overworld { Current: OuiChapterPanel panel } => panel.Area,
            Overworld { Current: OuiFileSelect } => SaveData.Instance.LastArea_Safe,
            _ => null
        };
        if (areaKey is not { } key)
            return DefaultSettings;

        if (!aonHelperMetadata.TryGetMetadata(key, out aonHelperMetadata meta))
        {
            Logger.Warn(LogID, $"Unable to find Curve Wipe metadata for map with SID {key.SID}, check your '.meta.yaml'!");
            return DefaultSettings;
        }

        return meta.CurveWipeSettings ?? DefaultSettings;
    }

    public CurveWipe(Scene scene, bool wipeIn, aonHelperMetadata.CurveWipeSettingsData settings, Action onComplete = null) : base(scene, wipeIn, onComplete)
    {
        this.settings = settings ?? DefaultSettings;
        if (this.settings.Points.Length <= 1 || (this.settings.Points.Length - 1) % 3 != 0)
            throw new Exception("Received invalid array of points for curve wipe path! Points provided must form a valid cubic Bezier spline.");

        path = new CurveHelper.BakedCurve(this.settings.Points, CurveHelper.BakedCurve.CurveType.Cubic, this.settings.Resolution);
        lengthFactor = path.CurveCount + 1;
        numQuads = (int) lengthFactor * this.settings.Resolution - 1;

        verts = new VertexPositionColor[12 * numQuads];
        for (int i = 0; i < verts.Length; i++)
            verts[i].Color = WipeIn ? Color.Black : Color.White;
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);

        UpdateVerts();
    }

    private void UpdateVerts()
    {
        int index = 0;
        for (int i = 0; i < numQuads; i++)
        {
            float startOffset = lengthFactor * i / numQuads;
            float endOffset = lengthFactor * (i + 1) / numQuads;

            float curvePercent = Ease.QuadIn(Percent) * lengthFactor;
            float startT = Calc.Clamp(curvePercent - startOffset, 0f, path.CurveCount);
            float endT = Calc.Clamp(curvePercent - endOffset, 0f, path.CurveCount);
            Vector2 startPos = path.GetPoint(startT);
            Vector2 endPos = path.GetPoint(endT);
            Vector2 startNormal = path.GetDerivative(startT).SafeNormalize().Perpendicular();
            Vector2 endNormal = path.GetDerivative(endT).SafeNormalize().Perpendicular();

            float startQuadPercent = Ease.Linear(startOffset);
            float endQuadPercent = Ease.Linear(endOffset);
            float startHeight = GetQuadHeight(settings.FromHeight, settings.ToHeight, ExpandHeight, startQuadPercent, Percent);
            float endHeight = GetQuadHeight(settings.FromHeight, settings.ToHeight, ExpandHeight, endQuadPercent, Percent);

            Vector3 a = new(startPos + startNormal * startHeight / 2f, 0f);
            Vector3 b = new(startPos - startNormal * startHeight / 2f, 0f);
            Vector3 c = new(endPos + endNormal * endHeight / 2f, 0f);
            Vector3 d = new(endPos - endNormal * endHeight / 2f, 0f);

            // quad
            verts[index++].Position = a;
            verts[index++].Position = b;
            verts[index++].Position = d;
            verts[index++].Position = a;
            verts[index++].Position = c;
            verts[index++].Position = d;
            // extra quad to ensure no holes when path self-intersects
            verts[index++].Position = a;
            verts[index++].Position = b;
            verts[index++].Position = c;
            verts[index++].Position = b;
            verts[index++].Position = c;
            verts[index++].Position = d;
        }
    }

    private static float GetQuadHeight(float from, float to, float max, float quadPercent, float overallPercent)
    {
        float normalHeight = Calc.LerpClamp(from, to, quadPercent);
        if (overallPercent < ExpandThreshold)
            return normalHeight;

        float expandPercent = (overallPercent - ExpandThreshold) / (1f - ExpandThreshold);
        return Calc.LerpClamp(normalHeight, max, expandPercent);
    }

    public override void BeforeRender(Scene scene)
    {
        hasDrawn = true;

        Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
        Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);

        GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
    }

    public override void Render(Scene scene)
    {
        base.Render(scene);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, SubtractBlendState, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);

        if (WipeIn && Percent <= 0.01f || !WipeIn && Percent >= 0.99f)
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
        else if (hasDrawn)
            Draw.SpriteBatch.Draw(Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);

        Draw.SpriteBatch.End();
    }
}
