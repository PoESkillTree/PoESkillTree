using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    internal class ModifierBuilderStub : IModifierBuilder
    {
        internal IEnumerable<IConditionBuilder> Conditions { get; private set; }
        internal IEnumerable<IFormBuilder> Forms { get; private set; }
        internal IEnumerable<IStatBuilder> Stats { get; private set; }
        internal Func<IStatBuilder, IStatBuilder> StatConverter { get; private set; }
        internal IEnumerable<ValueBuilder> Values { get; private set; }
        internal ValueFunc ValueConverter { get; private set; }

        public IModifierBuilder WithCondition(IConditionBuilder condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (Conditions != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Conditions = new[] { condition };
            return ret;
        }

        public IModifierBuilder WithConditions(IEnumerable<IConditionBuilder> conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));
            if (Conditions != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Conditions = conditions;
            return ret;
        }

        public IModifierBuilder WithForm(IFormBuilder form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            if (Forms != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Forms = new[] { form };
            return ret;
        }

        public IModifierBuilder WithForms(IEnumerable<IFormBuilder> forms)
        {
            if (forms == null)
                throw new ArgumentNullException(nameof(forms));
            if (Forms != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Forms = forms;
            return ret;
        }

        public IModifierBuilder WithStat(IStatBuilder stat)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));
            if (Stats != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Stats = new[] { stat };
            return ret;
        }

        public IModifierBuilder WithStatConverter(Func<IStatBuilder, IStatBuilder> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
            if (StatConverter != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.StatConverter = converter;
            return ret;
        }

        public IModifierBuilder WithStats(IEnumerable<IStatBuilder> stats)
        {
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));
            if (Stats != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Stats = stats;
            return ret;
        }

        public IModifierBuilder WithValue(ValueBuilder value)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(nameof(value));
            if (Values != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Values = new[] { value };
            return ret;
        }

        public IModifierBuilder WithValueConverter(ValueFunc converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
            if (ValueConverter != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.ValueConverter = converter;
            return ret;
        }

        public IModifierBuilder WithValues(IEnumerable<ValueBuilder> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (Values != null)
                throw new InvalidOperationException();
            var ret = (ModifierBuilderStub) MemberwiseClone();
            ret.Values = values;
            return ret;
        }

        public IModifier Create()
        {
            throw new InvalidOperationException();
        }
    }
}