namespace Celeste.Mod.aonHelper.Entities.Misc;

[CustomEntity("aonHelper/FlagCustomCoreMessage")]
[Tracked]
public class FlagCustomCoreMessage : Entity
{
    private readonly string text;

    private readonly float startFadeRadius, endFadeRadius;
    private readonly string appearFlag, stayFlag;
    private readonly float flagFadeTime;
    private readonly bool useRawDeltaTime;
    private float appearAlpha, playerAlpha, stayAlpha;
    private float alpha;

    private readonly Color textColor, outlineColor;
    private readonly bool hasOutline;
    private readonly float outlineThickness;

    private readonly float scale;
    private readonly Vector2 parallax;

    private readonly bool hideOnHeartCollect;
    
    public FlagCustomCoreMessage(Vector2 position,
        string dialogID, int lineNumber,
        float startFadeRadius, float endFadeRadius,
        string appearFlag, string stayFlag, float flagFadeTime, bool useRawDeltaTime,
        Color textColor, bool hasOutline, Color outlineColor, float outlineThickness,
        float scale, Vector2 parallax,
        bool hideOnHeartCollect)
        : base(position)
    {
        Tag = TagsExt.SubHUD;

        text = Dialog.Clean(dialogID)
                     .Split((char[])['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                     .ElementAtOrDefault(lineNumber)
            ?? $"{{{dialogID}, line {lineNumber}}}";

        this.startFadeRadius = startFadeRadius;
        this.endFadeRadius = endFadeRadius;

        this.appearFlag = appearFlag;
        this.stayFlag = stayFlag;
        this.flagFadeTime = flagFadeTime;
        this.useRawDeltaTime = useRawDeltaTime;

        this.textColor = textColor;
        this.hasOutline = hasOutline;
        this.outlineColor = outlineColor;
        this.outlineThickness = outlineThickness;

        this.scale = scale;
        this.parallax = parallax;

        this.hideOnHeartCollect = hideOnHeartCollect;
    }
    
    public FlagCustomCoreMessage(EntityData data, Vector2 offset)
        : this(data.Position + offset,
            data.Attr("dialogID"), data.Int("lineNumber"),
            data.Float("startFadeRadius", 96f), data.Float("endFadeRadius", 128f),
            data.Attr("appearFlag"), data.Attr("stayFlag"), data.Float("flagFadeTime", 0.4f), data.Bool("useRawDeltaTime"),
            data.HexColor("textColor", Color.White), data.Bool("hasOutline", true), data.HexColor("outlineColor", Color.Black), data.Float("outlineThickness", 2f),
            data.Float("scale", 1.25f), new Vector2(data.Float("parallaxX", 0.2f), data.Float("parallaxY", 0.2f)),
            data.Bool("hideOnHeartCollect", true))
    { }

    public override void Update()
    {
        base.Update();

        if (Scene.Tracker.GetEntity<Player>() is not { } player)
            return;

        bool appear = string.IsNullOrEmpty(appearFlag) || SceneAs<Level>().Session.GetFlag(appearFlag);
        bool stay = !string.IsNullOrEmpty(stayFlag) && SceneAs<Level>().Session.GetFlag(stayFlag);
        
        appearAlpha = Calc.Approach(appearAlpha, appear ? 1f : 0f, (useRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / flagFadeTime);
        stayAlpha = Calc.Approach(stayAlpha, stay ? 1f : 0f, (useRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / flagFadeTime);
        playerAlpha = Calc.ClampedMap(Vector2.Distance(player.Position, Position), startFadeRadius, endFadeRadius, 1f, 0f);
        alpha = Ease.CubeInOut(appearAlpha * MathF.Max(stayAlpha, playerAlpha));
    }

    public override void Render()
    {
        base.Render();

        Camera camera = SceneAs<Level>().Camera;
        Vector2 cameraCenter = new Vector2(camera.Left + camera.Right, camera.Top + camera.Bottom) / 2f;
        Vector2 position = (Position - camera.Position + (Position - cameraCenter) * parallax) * 6f;
        if (SaveData.Instance?.Assists.MirrorMode ?? false)
            position.X = Engine.Width - position.X;
        
        if (hasOutline)
            ActiveFont.DrawOutline(text, position, Vector2.One * 0.5f, Vector2.One * scale, textColor * alpha, outlineThickness, outlineColor * alpha);
        else
            ActiveFont.Draw(text, position, Vector2.One * 0.5f, Vector2.One * scale, textColor * alpha);
    }

    #region Hooks

    private static ILHook il_HeartGem_orig_CollectRoutine;
    
    [OnLoad]
    internal static void Load()
    {
        il_HeartGem_orig_CollectRoutine = new ILHook(typeof(HeartGem).GetMethod("orig_CollectRoutine", HookHelper.Bind.NonPublicInstance)!.GetStateMachineTarget()!, IL_HeartGem_orig_CollectRoutine);
    }

    [OnUnload]
    internal static void Unload()
    {
        HookHelper.DisposeAndSetNull(ref il_HeartGem_orig_CollectRoutine);
    }

    private static void IL_HeartGem_orig_CollectRoutine(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdcR4(1f),
            instr => instr.MatchStfld<FormationBackdrop>("Alpha")))
            throw new HookHelper.HookException(il, "Unable to find assignment to `level.FormationBackdrop.Alpha` to insert Flag Custom Core Message disabling after.");

        cursor.EmitLdloc1();
        cursor.EmitDelegate(HideFlagCustomCoreMessages);

        return;

        static void HideFlagCustomCoreMessages(HeartGem heart)
        {
            foreach (FlagCustomCoreMessage coreMessage in heart.Scene.Tracker
                    .GetEntities<FlagCustomCoreMessage>()
                    .Cast<FlagCustomCoreMessage>()
                    .Where(cm => cm.hideOnHeartCollect))
                coreMessage.Visible = false;
        }
    }

    #endregion
}
