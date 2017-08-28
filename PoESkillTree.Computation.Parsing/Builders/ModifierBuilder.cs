using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders
{
    public class ModifierBuilder : IModifierBuilder
    {
        public IReadOnlyList<Entry> Entries { get; }
        public Func<IStatBuilder, IStatBuilder> StatConverter { get; }
        public ValueFunc ValueConverter { get; }

        public ModifierBuilder()
        {
            Entries = new Entry[0];
        }

        private ModifierBuilder(IEnumerable<Entry> entries, 
            Func<IStatBuilder, IStatBuilder> statConverter, ValueFunc valueConverter)
        {
            Entries = entries.ToList();
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }

        private IModifierBuilder WithSingle<T>(T element, Func<Entry, T, Entry> entrySelector, 
            Func<Entry, T> entryElementSelector, string elementName)
        {
            IEnumerable<Entry> entries;
            if (Entries.IsEmpty())
            {
                entries = new[] { new Entry() }.Select(e => entrySelector(e, element));
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
            Func<Entry, T, Entry> entrySelector,
            Func<Entry, T> entryElementSelector, string elementName)
        {
            IEnumerable<Entry> entries;
            var elementList = elements.ToList();
            if (Entries.IsEmpty())
            {
                entries = elementList.Select(e => entrySelector(new Entry(), e));
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
                    "All calls to WithXs methods must be made with parameters with the " +
                    "same amount of elements", nameof(elements));
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

        public IModifierBuilder WithStatConverter(Func<IStatBuilder, IStatBuilder> converter)
        {
            return new ModifierBuilder(Entries, converter, ValueConverter);
        }

        public IModifierBuilder WithValue(ValueBuilder value)
        {
            return WithSingle(value, (e, v) => e.WithValue(v), e => e.Value, "Value");
        }

        public IModifierBuilder WithValues(IEnumerable<ValueBuilder> values)
        {
            return WithEnumerable(values, (e, v) => e.WithValue(v), e => e.Value, "Value");
        }

        public IModifierBuilder WithValueConverter(ValueFunc converter)
        {
            return new ModifierBuilder(Entries, StatConverter, converter);
        }

        public IModifierBuilder WithCondition(IConditionBuilder condition)
        {
            return WithSingle(condition, (e, c) => e.WithCondition(c), e => e.Condition,
                "Condition");
        }

        public IModifierBuilder WithConditions(IEnumerable<IConditionBuilder> conditions)
        {
            return WithEnumerable(conditions, (e, c) => e.WithCondition(c), e => e.Condition,
                "Condition");
        }


        public class Entry
        {
            [CanBeNull]
            public IFormBuilder Form { get; }

            [CanBeNull]
            public IStatBuilder Stat { get; }

            [CanBeNull]
            public ValueBuilder Value { get; }

            [CanBeNull]
            public IConditionBuilder Condition { get; }

            public Entry()
            {
            }

            private Entry(IFormBuilder form, IStatBuilder stat, ValueBuilder value, 
                IConditionBuilder condition)
            {
                Form = form;
                Stat = stat;
                Value = value;
                Condition = condition;
            }

            public Entry WithForm(IFormBuilder form)
            {
                return new Entry(form, Stat, Value, Condition);
            }

            public Entry WithStat(IStatBuilder stat)
            {
                return new Entry(Form, stat, Value, Condition);
            }

            public Entry WithValue(ValueBuilder value)
            {
                return new Entry(Form, Stat, value, Condition);
            }

            public Entry WithCondition(IConditionBuilder condition)
            {
                return new Entry(Form, Stat, Value, condition);
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                if (!(obj is Entry other))
                    return false;

                return Equals(Form, other.Form)
                    && Equals(Stat, other.Stat)
                    && Equals(Value, other.Value)
                    && Equals(Condition, other.Condition);
            }

            public override int GetHashCode()
            {
                return (Form != null ? Form.GetHashCode() : 0) ^
                       (Stat != null ? Stat.GetHashCode() : 0) ^
                       (!ReferenceEquals(Value, null) ? Value.GetHashCode() : 0) ^
                       (Condition != null ? Condition.GetHashCode() : 0);
            }
        }
    }
}