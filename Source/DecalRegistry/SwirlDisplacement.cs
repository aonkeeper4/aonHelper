using System.Xml;
using Celeste.Mod.Registry.DecalRegistryHandlers;

namespace Celeste.Mod.aonHelper.DecalRegistry;

public class SwirlDisplacementHandler : DecalRegistryHandler
{
    public override string Name => "aonHelper_swirlDisplacement";

    public override void Parse(XmlAttributeCollection xml)
        => throw new NotImplementedException();

    public override void ApplyTo(Decal decal)
        => throw new NotImplementedException();
}