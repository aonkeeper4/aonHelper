using Celeste.Mod.Registry;

namespace Celeste.Mod.aonHelper.Helpers;

public static class GlobalHelper
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(GlobalHelper)}";
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GlobalEntityAttribute(string entitySIDs, string globalAttributeName = null) : Attribute
    {
        public readonly string EntitySIDs = entitySIDs;
        
        public readonly string GlobalAttributeName = globalAttributeName;
    }
    
    #region Loader Processing
    
    private struct GlobalEntityLoader
    {
        public delegate Entity GlobalEntityLoadingHandler(Level level, LevelData levelData, Vector2 offset, EntityData entityData);
        public GlobalEntityLoadingHandler Handler;
        
        public string Attribute;
    }
    private static readonly Dictionary<string, GlobalEntityLoader> GlobalEntityLoaders = new();
    
    private static readonly Type[] globalEntityLoaderSignature = [typeof(Level), typeof(LevelData), typeof(Vector2), typeof(EntityData)];
    private static readonly Type[] globalEntityConstructorSignature1 = [typeof(EntityData), typeof(Vector2), typeof(EntityID)];
    private static readonly Type[] globalEntityConstructorSignature2 = [typeof(EntityData), typeof(Vector2)];
    private static readonly Type[] globalEntityConstructorSignature3 = [typeof(Vector2)];
    private static readonly Type[] globalEntityConstructorSignature4 = Type.EmptyTypes;
    
    public static void ProcessGlobalEntityAttributes(Type type, ref int attributesProcessed)
    {
        foreach (GlobalEntityAttribute attribute in type.GetCustomAttributes<GlobalEntityAttribute>())
        {
            string[] ids = attribute.EntitySIDs.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string globalAttributeName = string.IsNullOrEmpty(attribute.GlobalAttributeName) ? null : attribute.GlobalAttributeName;
            
            foreach (string id in ids)
            {
                string[] parts = id.Split('=');
                (string entitySID, string loaderMethodName) = parts.Length switch
                {
                    1 => (parts[0], "Load"),
                    2 => (parts[0], parts[1]),
                    _ => (null, null)
                };
                if (entitySID is null || loaderMethodName is null)
                {
                    Logger.Warn(LogID, $"Found invalid global entity loader string ('{id}') in type `{type.FullName}`!");
                    continue;
                }
                
                entitySID = entitySID.Trim();
                loaderMethodName = loaderMethodName.Trim();
                GlobalEntityLoader entityLoader = new() { Attribute = globalAttributeName };
                bool fromConstructor = false;
                
                if (type.GetMethod(loaderMethodName, globalEntityLoaderSignature) is { IsStatic: true} loaderMethod
                    && loaderMethod.ReturnType.IsCompatible(typeof(Entity)))
                {
                    // loader method
                    entityLoader.Handler = (level, levelData, offset, entityData) =>
                    {
                        EntityID sourceID = CreateEntityID(levelData, entityData);
                        Entity entity = (Entity) loaderMethod.Invoke(null, [level, levelData, offset, entityData]);
                        if (entity is null)
                            return null;
                        
                        entity.SourceData = entityData;
                        entity.SourceId = sourceID;
                        return entity;
                    };
                }
                else if (type.IsCompatible(typeof(Entity)))
                {
                    // constructor
                    if (type.GetConstructor(globalEntityConstructorSignature1) is { } constructor1)
                    {
                        entityLoader.Handler = (_, levelData, offset, entityData) =>
                        {
                            EntityID sourceID = CreateEntityID(levelData, entityData);
                            Entity entity = (Entity) constructor1.Invoke([entityData, offset, sourceID]);

                            entity.SourceData = entityData;
                            entity.SourceId = sourceID;
                            return entity;
                        };
                        fromConstructor = true;
                    }
                    else if (type.GetConstructor(globalEntityConstructorSignature2) is { } constructor2)
                    {
                        entityLoader.Handler = (_, levelData, offset, entityData) =>
                        {
                            Entity entity = (Entity) constructor2.Invoke([entityData, offset]);

                            entity.SourceData = entityData;
                            entity.SourceId = CreateEntityID(levelData, entityData);
                            return entity;
                        };
                        fromConstructor = true;
                    }
                    else if (type.GetConstructor(globalEntityConstructorSignature3) is { } constructor3)
                    {
                        entityLoader.Handler = (_, levelData, offset, entityData) =>
                        {
                            Entity entity = (Entity) constructor3.Invoke([offset]);

                            entity.SourceData = entityData;
                            entity.SourceId = CreateEntityID(levelData, entityData);
                            return entity;
                        };
                        fromConstructor = true;
                    }
                    else if (type.GetConstructor(globalEntityConstructorSignature4) is { } constructor4)
                    {
                        entityLoader.Handler = (_, levelData, offset, entityData) =>
                        {
                            Entity entity = (Entity) constructor4.Invoke(null);

                            entity.SourceData = entityData;
                            entity.SourceId = CreateEntityID(levelData, entityData);
                            return entity;
                        };
                        fromConstructor = true;
                    }
                }

                if (entityLoader.Handler is null)
                {
                    Logger.Warn(LogID, $"Found global entity with SID '{entitySID}' without a suitable constructor/loader method in type `{type.FullName}`!");
                    continue;
                }

                if (fromConstructor)
                    RegisterSidToTypeConnection(entitySID, type);
                GlobalEntityLoaders[entitySID] = entityLoader;
                
                Logger.Info(LogID, $"Registered global entity with SID '{entitySID}' in type `{type.FullName}`.");
                attributesProcessed++;
            }
        }
    }

    private static EntityID CreateEntityID(LevelData levelData, EntityData entityData, bool isTrigger = false)
        => new(levelData.Name, entityData.ID + (isTrigger ? 10_000_000 : 0));

    // jaaaa why is this internal
    private static readonly MethodInfo m_EntityRegistry_RegisterSidToTypeConnection
        = typeof(EntityRegistry).GetMethod("RegisterSidToTypeConnection", HookHelper.Bind.NonPublicStatic)!;
    private static void RegisterSidToTypeConnection(string entitySID, Type type)
        => m_EntityRegistry_RegisterSidToTypeConnection.Invoke(null, [entitySID, type]);
    
    #endregion
    
    #region Hooks

    [OnLoad]
    internal static void Load()
    {
        Everest.Events.LevelLoader.OnLoadingThread += Event_LevelLoader_OnLoadingThread;
        Everest.Events.Level.OnLoadEntity += Event_Level_OnLoadEntity;
    }

    [OnUnload]
    internal static void Unload()
    {
        Everest.Events.LevelLoader.OnLoadingThread -= Event_LevelLoader_OnLoadingThread;
        Everest.Events.Level.OnLoadEntity -= Event_Level_OnLoadEntity;
    }
    
    private static void Event_LevelLoader_OnLoadingThread(Level level)
    {
        foreach (LevelData levelData in level.Session.MapData.Levels)
        foreach (EntityData entityData in levelData.Entities)
        {
            if (!GlobalEntityLoaders.TryGetValue(entityData.Name, out GlobalEntityLoader entityLoader)
                || entityLoader.Attribute is { } attribute && !entityData.Bool(attribute))
                continue;

            Entity entity = entityLoader.Handler(level, levelData, new Vector2(levelData.Bounds.Left, levelData.Bounds.Top), entityData);
            entity.AddTag(Tags.Global);
            level.Add(entity);
            
            Logger.Info(LogID, $"Eagerly added global entity with SID '{entityData.Name}' and position {entityData.Position} in room '{levelData.Name}'.");
        }
    }
    
    private static bool Event_Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        if (!GlobalEntityLoaders.TryGetValue(entityData.Name, out GlobalEntityLoader entityLoader))
            return false;

        // return true even if we didn't add anything to make everest shut up
        if (entityLoader.Attribute is not { } attribute || entityData.Bool(attribute))
            return true;
        
        level.Add(entityLoader.Handler(level, levelData, offset, entityData));
        return true;
    }
    
    #endregion
}
