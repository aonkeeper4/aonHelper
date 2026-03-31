using ModInteropImportGenerator;
using System.Collections.Immutable;

namespace Celeste.Mod.aonHelper.Imports;

// growls
[GenerateImports("ReverseHelper.DreamBlock")]
public static partial class ReverseHelper
{
    public static partial void RegisterDreamBlockLike(Type type, Action<Entity> activate, Action<Entity> deactivate);
    public static partial void RegisterDreamBlockDummy(Type type, Func<Entity, Entity> getDummy);
    
    public static partial bool PlayerHasDreamDash(Entity dreamBlockLike);
    
    public static partial long ConfigureGetEnum(string enumVariant);
    public static partial bool? ConfigureGetFromEnum(Entity dreamBlockLike, long enumValue);
    public static partial void ConfigureSetFromEnum(Entity dreamBlockLike, long enumValue, bool? value);
    
    public static partial ImmutableArray<List<Entity>> GetDreamBlockTrackers(Scene scene);
}