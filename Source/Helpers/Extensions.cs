using Celeste.Mod.Helpers;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.aonHelper.Helpers;

public static class ILCursorExtensions
{
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
    public static bool TryGotoNextFirstFit(this ILCursor cursor, MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
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
    public static bool TryGotoPrevFirstFit(this ILCursor cursor, MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
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
    public static bool TryGotoNextFirstFitReversed(this ILCursor cursor, MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
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
    public static bool TryGotoPrevFirstFitReversed(this ILCursor cursor, MoveType moveType, int maxInstructionSpread, params Func<Instruction, bool>[] predicates)
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