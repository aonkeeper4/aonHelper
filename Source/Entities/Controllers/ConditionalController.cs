namespace Celeste.Mod.aonHelper.Entities.Controllers;

// yess generics jank
public class ConditionalController<T>(Vector2 position, string condition) : Controller(position) where T : ConditionalController<T>
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(T)}";
    
    #region Conditions
    
    private abstract class Condition
    {
        public abstract bool Check(Level level);
    }

    private class True : Condition
    {
        public override bool Check(Level _) => true;
    }

    private class Flag : Condition
    {
        // see https://github.com/JaThePlayer/FrostHelper/blob/5d9a39bb2f84c98bf4a31692e6b181847fb988a4/Code/FrostHelper/Helpers/AbstractExpressionTokenizer.cs#L225
        private static readonly char[] IllegalCharacters = "+-*/&|#@$(),\"!=<> ".ToCharArray();
        
        private readonly string flag;
        private readonly bool inverted;

        public Flag(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                Logger.Warn(LogID, $"Received empty {nameof(Flag)} condition!");
                return;
            }

            inverted = condition.StartsWith('!');
            string flagName = inverted ? condition[1..] : condition;
            if (flagName.Length <= 0 || IllegalCharacters.Any(c => flagName.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Warn(LogID, $"Received illegal {nameof(Flag)} condition: '{condition}'!");
                return;
            }
            
            flag = flagName;
        }
        
        public override bool Check(Level level)
            => flag is null || level.Session.GetFlag(flag) ^ inverted;
    }

    private class SessionExpression : Condition
    {
        private readonly object expression;
        
        public SessionExpression(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                Logger.Warn(LogID, $"Received empty {nameof(SessionExpression)} condition!");
                return;
            }

            if (!FrostHelper.TryCreateSessionExpression(condition, out object expr))
            {
                Logger.Warn(LogID, $"Received illegal {nameof(SessionExpression)} condition: '{condition}'!");
                return;
            }

            expression = expr;
        }

        public override bool Check(Level level)
            => expression is null || FrostHelper.GetBoolSessionExpressionValue(expression, level.Session);
    }
    
    #endregion
    
    protected new bool Active => SceneAs<Level>() is { } level && condition.Check(level);

    private readonly Condition condition = string.IsNullOrEmpty(condition)
        ? new True()
        : FrostHelper.IsImported
            ? new SessionExpression(condition)
            : new Flag(condition);

    public override void Added(Scene scene)
    {
        base.Added(scene);

        // no way to automatically track all instantiations of a generic type and no way to get trackedness information ahead of time
        if (!Tracker.StoredEntityTypes.Contains(typeof(T)))
            throw new InvalidOperationException($"{nameof(ConditionalController<T>)} added while {nameof(T)} is untracked!");
    }

    protected static bool TryGetActiveController(Level level, out T controller, bool checkNewlyAdded = false)
    {
        controller = null;
        if (level is null)
            return false;
        
        T controllerEntity = checkNewlyAdded
            ? level.Tracker.GetEntities<T>()
                           .Concat(level.Entities.ToAdd)
                           .OfType<T>()
                           .FirstOrDefault()
            : level.Tracker.GetEntity<T>();
        if (controllerEntity is null || !controllerEntity.Active)
            return false;
        
        controller = controllerEntity;
        return true;
    }
}
