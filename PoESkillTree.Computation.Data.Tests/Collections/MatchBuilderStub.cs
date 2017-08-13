using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    internal class MatchBuilderStub : IMatchBuilder
    {
        internal IEnumerable<IConditionProvider> Conditions { get; private set; }
        internal IEnumerable<IFormProvider> Forms { get; private set; }
        internal IEnumerable<IStatProvider> Stats { get; private set; }
        internal Func<IStatProvider, IStatProvider> StatConverter { get; private set; }
        internal IEnumerable<ValueProvider> Values { get; private set; }
        internal ValueFunc ValueConverter { get; private set; }

        public IMatchBuilder WithCondition(IConditionProvider condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (Conditions != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Conditions = new[] { condition };
            return ret;
        }

        public IMatchBuilder WithConditions(IEnumerable<IConditionProvider> conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));
            if (Conditions != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Conditions = conditions;
            return ret;
        }

        public IMatchBuilder WithForm(IFormProvider form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            if (Forms != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Forms = new[] { form };
            return ret;
        }

        public IMatchBuilder WithForms(IEnumerable<IFormProvider> forms)
        {
            if (forms == null)
                throw new ArgumentNullException(nameof(forms));
            if (Forms != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Forms = forms;
            return ret;
        }

        public IMatchBuilder WithStat(IStatProvider stat)
        {
            if (stat == null)
                throw new ArgumentNullException(nameof(stat));
            if (Stats != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Stats = new[] { stat };
            return ret;
        }

        public IMatchBuilder WithStatConverter(Func<IStatProvider, IStatProvider> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
            if (StatConverter != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.StatConverter = converter;
            return ret;
        }

        public IMatchBuilder WithStats(IEnumerable<IStatProvider> stats)
        {
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));
            if (Stats != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Stats = stats;
            return ret;
        }

        public IMatchBuilder WithValue(ValueProvider value)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(nameof(value));
            if (Values != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Values = new[] { value };
            return ret;
        }

        public IMatchBuilder WithValueConverter(ValueFunc converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
            if (ValueConverter != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.ValueConverter = converter;
            return ret;
        }

        public IMatchBuilder WithValues(IEnumerable<ValueProvider> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (Values != null)
                throw new InvalidOperationException();
            var ret = (MatchBuilderStub) MemberwiseClone();
            ret.Values = values;
            return ret;
        }
    }
}