namespace Celeste.Mod.aonHelper.Helpers;

public static class ConditionHelper
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(ConditionHelper)}";
    
    public abstract class Condition
    {
        public abstract bool Check(Level level);
    }

    private class True : Condition
    {
        public override bool Check(Level level) => level is not null;
    }

    private class Flag : Condition
    {
        // see https://github.com/JaThePlayer/FrostHelper/blob/master/Code/FrostHelper/Helpers/AbstractExpressionTokenizer.cs#L226
        private static readonly char[] IllegalCharacters = "+-*/&|#@$(),\"!=<> ".ToCharArray();
        
        private readonly string flag;
        private readonly bool inverted;

        public Flag(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                Logger.Warn(LogID, $"Attempted to parse empty {nameof(Flag)} condition!");
                return;
            }

            inverted = condition.StartsWith('!');
            string flagName = inverted ? condition[1..] : condition;
            if (flagName.Length <= 0 || IllegalCharacters.Any(c => flagName.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Warn(LogID, $"Attempted to parse illegal {nameof(Flag)} condition: '{condition}'!");
                return;
            }
            
            flag = flagName;
        }
        
        public override bool Check(Level level)
            => level is not null && (flag is null || level.Session.GetFlag(flag) ^ inverted);
    }

    private class SessionExpression : Condition
    {
        private readonly object expression;
        
        public SessionExpression(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                Logger.Warn(LogID, $"Attempted to parse empty {nameof(SessionExpression)} condition!");
                return;
            }

            if (!FrostHelper.TryCreateSessionExpression(condition, out object expr))
            {
                Logger.Warn(LogID, $"Attempted to parse illegal {nameof(SessionExpression)} condition: '{condition}'!");
                return;
            }

            expression = expr;
        }

        public override bool Check(Level level)
            => level is not null && (expression is null || FrostHelper.GetBoolSessionExpressionValue(expression, level.Session));
    }
    
    public static Condition Create(string condition)
        => string.IsNullOrEmpty(condition)
            ? new True()
            : FrostHelper.IsImported
                ? new SessionExpression(condition)
                : new Flag(condition);
}
