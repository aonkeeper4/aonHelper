using Celeste.Mod.Registry;

namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[GlobalHelper.GlobalEntity("aonHelper/TaikoDrumController", "global")]
[Tracked]
public class TaikoDrumController(Vector2 position,
    float soundWaveSpeed, int soundWaveDepth, Color soundWaveColor,
    string affectedEntities)
    : Entity(position)
{
    public const float DefaultSoundWaveSpeed = 200f;
    public const int DefaultSoundWaveDepth = Depths.FGDecals - 1;
    public static readonly Color DefaultSoundWaveColor = Calc.HexToColor("f3dbc5");

    public readonly float SoundWaveSpeed = soundWaveSpeed;
    public readonly int SoundWaveDepth = soundWaveDepth;
    public readonly Color SoundWaveColor = soundWaveColor;
    
    public readonly Type[] AffectedTypes = affectedEntities != "*" ? GetTypes(affectedEntities) : [];
    public readonly bool AffectAll = affectedEntities == "*";

    private static Type[] GetTypes(string entitySIDs)
        => entitySIDs.Split(",", StringSplitOptions.RemoveEmptyEntries)
                     .SelectMany(sid => EntityRegistry.GetKnownTypesFromSid(sid).Where(t => t.IsOrIsSubclassOf(typeof(Solid))))
                     .ToArray();

    public TaikoDrumController(EntityData data, Vector2 offset)
        : this(data.Position + offset,
            data.Float("soundWaveSpeed", DefaultSoundWaveSpeed), data.Int("soundWaveDepth", DefaultSoundWaveDepth), data.HexColor("soundWaveColor", DefaultSoundWaveColor),
            data.Attr("affectedEntities"))
    { }
}
