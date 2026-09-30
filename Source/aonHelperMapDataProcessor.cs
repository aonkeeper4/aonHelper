namespace Celeste.Mod.aonHelper;

public class aonHelperMapDataProcessor : EverestMapDataProcessor
{
    public override Dictionary<string, Action<BinaryPacker.Element>> Init()
        => new();

    public override void Reset()
    { }

    public override void End()
    { }
}
