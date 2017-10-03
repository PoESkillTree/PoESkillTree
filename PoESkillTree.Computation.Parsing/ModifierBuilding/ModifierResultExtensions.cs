using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public static class ModifierResultExtensions
    {
        public static IModifierResult Merge(IModifierResult left, IModifierResult right)
        {
            IStatBuilder ConvertStat(IStatBuilder s) => 
                right.StatConverter(left.StatConverter(s));

            IValueBuilder ConvertValue(IValueBuilder v) =>
                right.ValueConverter(left.ValueConverter(v));

            if (left.Entries.Count > right.Entries.Count)
            {
                (left, right) = (right, left);
            }
            if (left.Entries.Count > 1)
            {
                throw new ArgumentException(
                    "There may only be one IModifierResult with multiple entries");
            }
            if (left.Entries.IsEmpty() || right.Entries.IsEmpty())
            {
                return new SimpleModifierResult(right.Entries, ConvertStat, ConvertValue);
            }

            var leftEntry = left.Entries.Single();
            IReadOnlyList<ModifierResultEntry> entries;
            if (right.Entries.Count == 1)
            {
                entries = new[] { Merge(leftEntry, right.Entries[0]) };
            }
            else
            {
                entries = right.Entries.Select(r => Merge(leftEntry, r)).ToList();
            }
            return new SimpleModifierResult(entries, ConvertStat, ConvertValue);
        }

        private static ModifierResultEntry Merge(ModifierResultEntry left,
            ModifierResultEntry right)
        {
            if (left.Form != null && right.Form != null)
                throw new ArgumentException("Form may only be set once");
            if (left.Stat != null && right.Stat!= null)
                throw new ArgumentException("Stat may only be set once");
            if (left.Value != null && right.Value!= null)
                throw new ArgumentException("Value may only be set once");

            IConditionBuilder condition;
            if (left.Condition == null)
            {
                condition = right.Condition;
            }
            else if (right.Condition == null)
            {
                condition = left.Condition;
            }
            else
            {
                condition = left.Condition.And(right.Condition);
            }

            return new ModifierResultEntry()
                .WithForm(left.Form ?? right.Form)
                .WithStat(left.Stat ?? right.Stat)
                .WithValue(left.Value ?? right.Value)
                .WithCondition(condition);
        }

        public static IModifierResult Aggregate(this IEnumerable<IModifierResult> results)
        {
            return results.Aggregate(SimpleModifierResult.Empty, Merge);
        }

        public static IReadOnlyList<Modifier> Build(this IModifierResult result)
        {
            return (from entry in result.Entries
                    let stat = result.StatConverter(entry.Stat)
                    let value = result.ValueConverter(entry.Value)
                    select new Modifier(stat, entry.Form, value, entry.Condition))
                .ToList();
        }
    }
}