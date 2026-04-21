namespace Celeste.Mod.aonHelper.Helpers;

public static class GlobalHelper
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GlobalEntityAttribute(string entitySID, string onlyGlobalIf = null) : Attribute
    {
        public string EntitySID = entitySID;
        
        public string OnlyGlobalIf = onlyGlobalIf;
    }
    
    public static void ProcessGlobalEntityAttributes(Type type, ref int attributesProcessed)
    {
        
    }
}
