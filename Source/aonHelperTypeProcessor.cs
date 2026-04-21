namespace Celeste.Mod.aonHelper;

public static class aonHelperTypeProcessor
{
	private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperTypeProcessor)}";

	public delegate void TypeProcessor(Type type, ref int attributesProcessed);
	public static readonly TypeProcessor[] TypeProcessors = [
		GlobalHelper.ProcessGlobalEntityAttributes
		// add more here
	];
	
    private static void Process(Assembly assembly)
    {
	    Logger.Info(LogID, $"Processing types for assembly {assembly.FullName}.");
	    int attributesProcessed = 0;
	    
	    foreach (Type type in assembly.GetTypesSafe())
	    foreach (TypeProcessor processor in TypeProcessors)
		    processor.Invoke(type, ref attributesProcessed);
	    
	    Logger.Info(LogID, $"Finished processing assembly with {attributesProcessed} attributes processed.");
    }
    
    internal static void Load()
    {
	    // i think this is how ur supposed to get them ?
	    foreach (Assembly assembly in aonHelperModule.Instance.Metadata.AssemblyContext.Assemblies)
		    Process(assembly);
    }
}
