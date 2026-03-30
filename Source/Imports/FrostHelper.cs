using ModInteropImportGenerator;

namespace Celeste.Mod.aonHelper.Imports;

[GenerateImports("FrostHelper", RequiredDependency = false)]
public static partial class FrostHelper
{
    public static partial bool TryCreateSessionExpression(string str, out object expression);
    public static partial bool TryCreateSessionExpression(string str, object context, out object expression);

    public static partial object GetSessionExpressionValue(object expression, Session session);
    public static partial object GetSessionExpressionValue(object expression, Session session, object userdata);
    
    public static partial Type GetSessionExpressionReturnedType(object expression);

    public static partial int GetIntSessionExpressionValue(object expression, Session session);
    public static partial int GetIntSessionExpressionValue(object expression, Session session, object userdata);
    public static partial float GetFloatSessionExpressionValue(object expression, Session session);
    public static partial float GetFloatSessionExpressionValue(object expression, Session session, object userdata);
    public static partial bool GetBoolSessionExpressionValue(object expression, Session session);
    public static partial bool GetBoolSessionExpressionValue(object expression, Session session, object userdata);

    public static partial void RegisterSimpleSessionExpressionCommand(string modName, string cmdName, Func<Session, object> func);
    public static partial void RegisterFunctionSessionExpressionCommand(string modName, string cmdName, Func<Session, IReadOnlyList<object>, object> func);

    public static partial object CreateSessionExpressionContext(
        Dictionary<string, Func<Session, object, object>> simpleCommands,
        Dictionary<string, Func<Session, object, IReadOnlyList<object>, object>> functionCommands);
}