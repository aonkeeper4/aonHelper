namespace Celeste.Mod.aonHelper;

public static class aonHelperTypeProcessor
{
	public delegate void TypeProcessor(Type type);
    public static readonly TypeProcessor[] TypeProcessors = [
		GlobalHelper.ProcessGlobalEntityAttributes
		// add more here
	];
	
    private static void Process(Assembly assembly)
    {
	    foreach (Type type in assembly.GetTypesSafe())
	    foreach (TypeProcessor processor in TypeProcessors)
		    processor.Invoke(type);
    }
    
    internal static void Load()
    {
	    // only process this assembly
	    Process(typeof(aonHelperModule).Assembly);
    }
}
