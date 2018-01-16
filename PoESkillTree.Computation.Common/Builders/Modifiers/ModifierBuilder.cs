using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <inheritdoc cref="IModifierBuilder" />
    public class ModifierBuilder : IModifierBuilder, IIntermediateModifier
    {
        public IReadOnlyList<IntermediateModifierEntry> Entries { get; }
        public StatConverter StatConverter { get; } = s => s;
        public ValueConverter ValueConverter { get; } = v => v;

        public ModifierBuilder()
        {
            Entries = new IntermediateModifierEntry[0];
        }

        private ModifierBuilder(IEnumerable<IntermediateModifierEntry> entries, 
            StatConverter statConverter, 
            ValueConverter valueConverter)
        {
            Entries = entries.ToList();
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }

        private IModifierBuilder WithSingle<T>(T element, 
            Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector, 
            Func<IntermediateModifierEntry, T> entryElementSelector, string elementName)
        {
            IEnumerable<IntermediateModifierEntry> entries;
            if (Entries.IsEmpty())
            {
                entries = new[] { entrySelector(new IntermediateModifierEntry(), element) };
            }
            else if (Entries.Select(entryElementSelector).Any(t => t != null))
            {
                throw new InvalidOperationException(elementName + " may not be set multiple times");
            }
            else
            {
                entries = Entries.Select(e => entrySelector(e, element));
            }
            return new ModifierBuilder(entries, StatConverter, ValueConverter);
        }

        private IModifierBuilder WithEnumerable<T>(IEnumerable<T> elements, 
            Func<IntermediateModifierEntry, T, IntermediateModifierEntry> entrySelector,
            Func<IntermediateModifierEntry, T> entryElementSelector, string elementName)
        {
            IEnumerable<IntermediateModifierEntry> entries;
            var elementList = elements.ToList();
            if (Entries.IsEmpty())
            {
                entries = elementList.Select(e => entrySelector(new IntermediateModifierEntry(), e));
            }
            else if (Entries.Select(entryElementSelector).Any(t => t != null))
            {
                throw new InvalidOperationException(elementName + " may not be set multiple times");
            }
            else if (Entries.Count == 1)
            {
                var entry = Entries[0];
                entries = elementList.Select(e => entrySelector(entry, e));
            }
            else if (Entries.Count != elementList.Count)
            {
                throw new ArgumentException(
                    "All calls to WithXs methods must be made with parameters with the same amount of elements", 
                    nameof(elements));
            }
            else
            {
                entries = Entries.Zip(elementList, entrySelector);
            }
            return new ModifierBuilder(entries, StatConverter, ValueConverter);
        }

        public IModifierBuilder WithForm(IFormBuilder form)
        {
            return WithSingle(form, (e, f) => e.WithForm(f), e => e.Form, "Form");
        }

        public IModifierBuilder WithForms(IEnumerable<IFormBuilder> forms)
        {
            return WithEnumerable(forms, (e, f) => e.WithForm(f), e => e.Form, "Form");
        }

        public IModifierBuilder WithStat(IStatBuilder stat)
        {
            return WithSingle(stat, (e, s) => e.WithStat(s), e => e.Stat, "Stat");
        }

        public IModifierBuilder WithStats(IEnumerable<IStatBuilder> stats)
        {
            return WithEnumerable(stats, (e, s) => e.WithStat(s), e => e.Stat, "Stat");
        }

        public IModifierBuilder WithStatConverter(StatConverter converter)
        {
            return new ModifierBuilder(Entries, converter, ValueConverter);
        }

        public IModifierBuilder WithValue(IValueBuilder value)
        {
            return WithSingle(value, (e, v) => e.WithValue(v), e => e.Value, "Value");
        }

        public IModifierBuilder WithValues(IEnumerable<IValueBuilder> values)
        {
            return WithEnumerable(values, (e, v) => e.WithValue(v), e => e.Value, "Value");
        }

        public IModifierBuilder WithValueConverter(ValueConverter converter)
        {
            return new ModifierBuilder(Entries, StatConverter, converter);
        }

        public IModifierBuilder WithCondition(IConditionBuilder condition)
        {
            return WithSingle(condition, (e, c) => e.WithCondition(c), e => e.Condition, "Condition");
        }

        public IModifierBuilder WithConditions(IEnumerable<IConditionBuilder> conditions)
        {
            return WithEnumerable(conditions, (e, c) => e.WithCondition(c), e => e.Condition, "Condition");
        }

        public IIntermediateModifier Build()
        {
            return this;
        }
    }
}