using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <inheritdoc cref="IModifierBuilder" />
    public class ModifierBuilder : IModifierBuilder, IIntermediateModifier
    {
        public IReadOnlyList<IntermediateModifierEntry> Entries { get; }
        public StatConverter StatConverter { get; }
        public ValueConverter ValueConverter { get; }

        public static readonly ModifierBuilder Empty
            = new ModifierBuilder(new IntermediateModifierEntry[0], Funcs.Identity, Funcs.Identity);

        private ModifierBuilder(IReadOnlyList<IntermediateModifierEntry> entries, 
            StatConverter statConverter, 
            ValueConverter valueConverter)
        {
            Entries = entries;
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }

        public IModifierBuilder WithForm(IFormBuilder form)
            => WithSingle(form, (e, f) => e.WithForm(f));

        public IModifierBuilder WithForms(IReadOnlyList<IFormBuilder> forms)
            => WithEnumerable(forms, (e, f) => e.WithForm(f));

        public IModifierBuilder WithStat(IStatBuilder stat)
            => WithSingle(stat, (e, s) => e.WithStat(s));

        public IModifierBuilder WithStats(IReadOnlyList<IStatBuilder> stats)
            => WithEnumerable(stats, (e, s) => e.WithStat(s));

        public IModifierBuilder WithStatConverter(StatConverter converter)
            => new ModifierBuilder(Entries, converter, ValueConverter);

        public IModifierBuilder WithValue(IValueBuilder value)
            => WithSingle(value, (e, v) => e.WithValue(v));

        public IModifierBuilder WithValues(IReadOnlyList<IValueBuilder> values)
            => WithEnumerable(values, (e, v) => e.WithValue(v));

        public IModifierBuilder WithValueConverter(ValueConverter converter)
            => new ModifierBuilder(Entries, StatConverter, converter);

        public IModifierBuilder WithCondition(IConditionBuilder condition)
            => WithSingle(condition, (e, c) => e.WithCondition(c));

        public IModifierBuilder WithConditions(IReadOnlyList<IConditionBuilder> conditions)
            => WithEnumerable(conditions, (e, c) => e.WithCondition(c));

        public IIntermediateModifier Build() => this;

        private IModifierBuilder WithSingle<T>(T element, 
            Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector)
        {
            if (Entries.IsEmpty())
                return WithEntries(new[] { entrySelector(new IntermediateModifierEntry(), element) });

            var entries = new List<IntermediateModifierEntry>(Entries.Count);
            foreach (var entry in Entries)
            {
                entries.Add(entrySelector(entry, element));
            }
            return WithEntries(entries);
        }

        private IModifierBuilder WithEnumerable<T>(IReadOnlyList<T> elements, 
            Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector)
        {
            if (Entries.IsEmpty())
                return WithEnumerableNoEntries(elements, entrySelector);
            if (Entries.Count == 1)
                return WithEnumerableSingleEntry(elements, entrySelector);
            return WithEnumerableManyEntries(elements, entrySelector);
        }

        private IModifierBuilder WithEnumerableNoEntries<T>(
            IReadOnlyList<T> elements, Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector)
        {
            var entries = new List<IntermediateModifierEntry>(elements.Count);
            foreach (var element in elements)
            {
                entries.Add(entrySelector(new IntermediateModifierEntry(), element));
            }
            return WithEntries(entries);
        }

        private IModifierBuilder WithEnumerableSingleEntry<T>(
            IReadOnlyList<T> elements, Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector)
        {
            var entries = new List<IntermediateModifierEntry>(elements.Count);
            foreach (var element in elements)
            {
                entries.Add(entrySelector(Entries[0], element));
            }
            return WithEntries(entries);
        }

        private IModifierBuilder WithEnumerableManyEntries<T>(
            IReadOnlyList<T> elements, Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector)
        {
            if (Entries.Count != elements.Count)
                throw new ArgumentException(
                    "All calls to WithXs methods must be made with parameters with the same amount of elements",
                    nameof(elements));

            var entries = new List<IntermediateModifierEntry>(Entries.Count);
            for (var i = 0; i < Entries.Count; i++)
            {
                entries.Add(entrySelector(Entries[i], elements[i]));
            }
            return WithEntries(entries);
        }

        private ModifierBuilder WithEntries(IReadOnlyList<IntermediateModifierEntry> entries)
            => new ModifierBuilder(entries, StatConverter, ValueConverter);
    }
}