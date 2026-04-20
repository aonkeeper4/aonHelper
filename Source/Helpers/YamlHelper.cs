using System.ComponentModel.DataAnnotations;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Celeste.Mod.aonHelper.Helpers;

public static class YamlHelper
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(YamlHelper)}";

    public class NoNullItems() : ValidationAttribute("No null items were expected.")
    {
        public override bool IsValid(object value)
            => value switch {
                null => false,
                IDictionary dictionary => dictionary.Keys.Cast<object>().All(key => key is not null && dictionary[key] is not null),
                IEnumerable enumerable => enumerable.Cast<object>().All(item => item is not null),
                _ => throw new NotImplementedException($"Non-null item validation for type {value.GetType()} is not implemented.")
            };
    }

    private class ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer) : INodeDeserializer
    {
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value, ObjectDeserializer rootDeserializer)
        {
            if (!nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value, rootDeserializer))
                return false;

            if (value is null)
                return true;

            // the exception will be wrapped in a YamlException by NodeValueDeserializer.DeserializeValue
            ValidationContext context = new(value, null, null);
            Validator.ValidateObject(value, context, true);
            return true;
        }
    }

    public static readonly IDeserializer ValidatingDeserializer = new DeserializerBuilder()
        .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
        .IgnoreUnmatchedProperties()
        .Build();
    
    extension(ModAsset asset)
    {
        public T ValidatingDeserialize<T>()
        {
            if (asset.Type != typeof(AssetTypeYaml))
                return default;

            using StreamReader input = new(asset.Stream);
            T result = ValidatingDeserializer.Deserialize<T>(input);
            return result;
        }
        
        public bool TryValidatingDeserialize<T>(out T result)
        {
            if (asset.Type != typeof(AssetTypeYaml))
                goto fail;

            try
            {
                using StreamReader input = new(asset.Stream);
                result = ValidatingDeserializer.Deserialize<T>(input);
            }
            catch (YamlException e)
            {
                Logger.Warn(LogID, $"Failed to deserialize mod asset {asset.PathVirtual}.yaml!");
                Logger.LogDetailed(e, LogID);
                goto fail;
            }
            
            return true;
        
        fail:
            result = default;
            return false;
        }
    }
}
