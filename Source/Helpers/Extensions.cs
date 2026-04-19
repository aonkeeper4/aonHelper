using System.Globalization;

namespace Celeste.Mod.aonHelper.Helpers;

public static class Extensions
{
    extension(Rectangle rect)
    {
        public Vector2 TopLeft() => new(rect.Left, rect.Top);
        public Vector2 TopRight() => new(rect.Right, rect.Top);
        public Vector2 BottomLeft() => new(rect.Left, rect.Bottom);
        public Vector2 BottomRight() => new(rect.Right, rect.Bottom);
    }

    extension<TSource>(IEnumerable<TSource> source)
    {
        public int LastIndexWhere(Func<TSource, bool> predicate)
        {
            int index = 0;
            int resultIndex = -1;
            foreach (TSource item in source)
            {
                if (predicate.Invoke(item)) resultIndex = index;
                index++;
            }
            return resultIndex;
        }
    }

    extension(EntityData data)
    {
        public Color? NullableHexColor(string key)
        {
            string value = data.Attr(key);
            return string.IsNullOrEmpty(value) ? null : Calc.HexToColor(value);
        }

        public T? Nullable<T>(string key) where T : struct, IParsable<T>
        {
            string value = data.Attr(key);
            return string.IsNullOrEmpty(value) ? null : T.Parse(value, CultureInfo.InvariantCulture);
        }

        public Color[] HexColorArray(string key)
        {
            string value = data.Attr(key);
            if (string.IsNullOrEmpty(value))
                return [];

            return value.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(Calc.HexToColor)
                        .ToArray();
        }
    }

    extension(ILCursor cursor)
    {
        public VariableDefinition AddVariable(Type type)
        {
            VariableDefinition variableDefinition = new(cursor.Context.Import(type));
            cursor.Body.Variables.Add(variableDefinition);
            return variableDefinition;
        }

        public VariableDefinition AddVariable<T>()
            => cursor.AddVariable(typeof(T));
        
        /// <summary>
        /// Go to the next match of a given IL sequence, allowing up to <paramref name="maxInstructionSpread"/> instructions of tolerance if the instructions are not sequential (i.e. if something else hooks the same sequence).
        /// </summary>
        /// <param name="cursor">The IL cursor to look for a match in.</param>
        /// <param name="moveType">The move type to use.</param>
        /// <param name="maxInstructionSpread">The amount of instructions between predicate matches to still consider as a successful match.</param>
        /// <param name="predicates">The IL instructions to match against.</param>
        /// <remarks>
        /// This function picks the first match, which might not have the least possible instruction spread.<br/>
        /// For that, see <see cref="ILCursorExtensions.TryGotoNextBestFit(ILCursor,MoveType,int,Func&lt;Instruction,bool&gt;[])"/>.
        /// </remarks>
        /// <returns>Whether a match has been found, and the cursor has been moved.</returns>
        public bool TryGotoNextFirstFit(MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
        {
            if (predicates.Length == 0)
                throw new ArgumentException("No predicates given.");
    
            if (predicates.Length == 1)
                return cursor.TryGotoNext(moveType, predicates[0]);
    
            int matchFrom = -1, matchTo = -1;
            while (cursor.TryGotoNext(MoveType.Before, predicates[0]))
            {
                matchFrom = cursor.Index++;
                bool flag = true;
                for (int i = 1; i < predicates.Length; i++)
                {
                    Func<Instruction, bool> func = predicates[i];
                    int index = cursor.Index;
                    if (!cursor.TryGotoNext(MoveType.After, func))
                    {
                        flag = false;
                        break;
                    }
    
                    int instructionSpread = cursor.Index - index;
                    if (instructionSpread > maxInstructionSpread)
                    {
                        flag = false;
                        break;
                    }
                }
    
                if (flag)
                {
                    matchTo = cursor.Index;
                    break;
                }
    
                cursor.Index = matchFrom + 1;
            }
    
            if (matchFrom == -1 || matchTo == -1)
                return false;
    
            cursor.Index = moveType != MoveType.After ? matchFrom : matchTo;
            if (moveType == MoveType.AfterLabel)
                cursor.MoveAfterLabels();
    
            return true;
        }
        
        /// <summary>
        /// Go to the previous match of a given IL sequence, allowing up to <paramref name="maxInstructionSpread"/> instructions of tolerance if the instructions are not sequential (i.e. if something else hooks the same sequence).
        /// </summary>
        /// <param name="cursor">The IL cursor to look for a match in.</param>
        /// <param name="moveType">The move type to use.</param>
        /// <param name="maxInstructionSpread">The amount of instructions between predicate matches to still consider as a successful match.</param>
        /// <param name="predicates">The IL instructions to match against.</param>
        /// <remarks>
        /// This function picks the first match, which might not have the least possible instruction spread.<br/>
        /// For that, see <see cref="ILCursorExtensions.TryGotoPrevBestFit(ILCursor,MoveType,int,Func&lt;Instruction,bool&gt;[])"/>.
        /// </remarks>
        /// <returns>Whether a match has been found, and the cursor has been moved.</returns>
        public bool TryGotoPrevFirstFit(MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
        {
            if (predicates.Length == 0)
                throw new ArgumentException("No predicates given.");
    
            if (predicates.Length == 1)
                return cursor.TryGotoPrev(moveType, predicates[0]);
    
            int matchFrom = -1, matchTo = -1;
            while (cursor.TryGotoPrev(MoveType.Before, predicates[0]))
            {
                matchFrom = cursor.Index++;
                bool flag = true;
                for (int i = 1; i < predicates.Length; i++)
                {
                    Func<Instruction, bool> func = predicates[i];
                    int index = cursor.Index;
                    if (!cursor.TryGotoNext(MoveType.After, func))
                    {
                        flag = false;
                        break;
                    }
    
                    int instructionSpread = cursor.Index - index;
                    if (instructionSpread > maxInstructionSpread)
                    {
                        flag = false;
                        break;
                    }
                }
    
                if (flag)
                {
                    matchTo = cursor.Index;
                    break;
                }
    
                cursor.Index = matchFrom;
            }
    
            if (matchFrom == -1 || matchTo == -1)
                return false;
    
            cursor.Index = moveType != MoveType.After ? matchFrom : matchTo;
            if (moveType == MoveType.AfterLabel)
                cursor.MoveAfterLabels();
    
            return true;
        }
    
        /// <summary>
        /// Go to the next match of a given IL sequence, allowing up to <paramref name="maxInstructionSpread"/> instructions of tolerance if the instructions are not sequential (i.e. if something else hooks the same sequence), checking the match in reverse order.
        /// </summary>
        /// <param name="cursor">The IL cursor to look for a match in.</param>
        /// <param name="moveType">The move type to use.</param>
        /// <param name="maxInstructionSpread">The amount of instructions between predicate matches to still consider as a successful match.</param>
        /// <param name="predicates">The IL instructions to match against.</param>
        /// <remarks>
        /// This function picks the first match, which might not have the least possible instruction spread.<br/>
        /// For that, see <see cref="ILCursorExtensions.TryGotoNextBestFit(ILCursor,MoveType,int,Func&lt;Instruction,bool&gt;[])"/>.<br/>
        /// This function also checks its match predicates in reverse order, starting from the last.<br/>
        /// If you do not want this behavior, see <see cref="TryGotoNextFirstFit"/>.
        /// </remarks>
        /// <returns>Whether a match has been found, and the cursor has been moved.</returns>
        public bool TryGotoNextFirstFitReversed(MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
        {
            if (predicates.Length == 0)
                throw new ArgumentException("No predicates given.");
    
            if (predicates.Length == 1)
                return cursor.TryGotoNext(moveType, predicates[0]);
    
            int matchFrom = -1, matchTo = -1;
            while (cursor.TryGotoNext(MoveType.Before, predicates[^1]))
            {
                matchTo = cursor.Index + 1;
                bool flag = true;
                for (int i = predicates.Length - 2; i >= 0; i--)
                {
                    Func<Instruction, bool> func = predicates[i];
                    int index = cursor.Index;
                    if (!cursor.TryGotoPrev(MoveType.Before, func))
                    {
                        flag = false;
                        break;
                    }
    
                    int instructionSpread = index - cursor.Index;
                    if (instructionSpread > maxInstructionSpread)
                    {
                        flag = false;
                        break;
                    }
                }
    
                if (flag)
                {
                    matchFrom = cursor.Index;
                    break;
                }
    
                cursor.Index = matchTo + 1;
            }
    
            if (matchFrom == -1 || matchTo == -1)
                return false;
    
            cursor.Index = moveType != MoveType.After ? matchFrom : matchTo;
            if (moveType == MoveType.AfterLabel)
                cursor.MoveAfterLabels();
    
            return true;
        }
    
        /// <summary>
        /// Go to the previous match of a given IL sequence, allowing up to <paramref name="maxInstructionSpread"/> instructions of tolerance if the instructions are not sequential (i.e. if something else hooks the same sequence), checking the match in reverse order.
        /// </summary>
        /// <param name="cursor">The IL cursor to look for a match in.</param>
        /// <param name="moveType">The move type to use.</param>
        /// <param name="maxInstructionSpread">The amount of instructions between predicate matches to still consider as a successful match.</param>
        /// <param name="predicates">The IL instructions to match against.</param>
        /// <remarks>
        /// This function picks the first match, which might not have the least possible instruction spread.<br/>
        /// For that, see <see cref="ILCursorExtensions.TryGotoPrevBestFit(ILCursor,MoveType,int,Func&lt;Instruction,bool&gt;[])"/>.<br/>
        /// This function also checks its match predicates in reverse order, starting from the last.<br/>
        /// If you do not want this behavior, see <see cref="TryGotoNextFirstFit"/>.
        /// </remarks>
        /// <returns>Whether a match has been found, and the cursor has been moved.</returns>
        public bool TryGotoPrevFirstFitReversed(MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
        {
            if (predicates.Length == 0)
                throw new ArgumentException("No predicates given.");
    
            if (predicates.Length == 1)
                return cursor.TryGotoPrev(moveType, predicates[0]);
    
            int matchFrom = -1, matchTo = -1;
            while (cursor.TryGotoPrev(MoveType.Before, predicates[^1]))
            {
                matchTo = cursor.Index + 1;
                bool flag = true;
                for (int i = predicates.Length - 2; i >= 0; i--)
                {
                    Func<Instruction, bool> func = predicates[i];
                    int index = cursor.Index;
                    if (!cursor.TryGotoPrev(MoveType.Before, func))
                    {
                        flag = false;
                        break;
                    }
    
                    int instructionSpread = index - cursor.Index;
                    if (instructionSpread > maxInstructionSpread)
                    {
                        flag = false;
                        break;
                    }
                }
    
                if (flag)
                {
                    matchFrom = cursor.Index;
                    break;
                }
    
                cursor.Index = matchFrom;
            }
    
            if (matchFrom == -1 || matchTo == -1)
                return false;
    
            cursor.Index = moveType != MoveType.After ? matchFrom : matchTo;
            if (moveType == MoveType.AfterLabel)
                cursor.MoveAfterLabels();
    
            return true;
        }
    }
}
