using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation
{
    // Data structures and data in separate projects to allow hiding implementation with internal

    // Interfaces have "Provider" in name as they may be dependent on the values and/or groups of the
    // actually matched stat line and to not lead to confusion with existing classes.

    public interface IComputationData
    {
        IReadOnlyList<string> GivenStats { get; }

        GivenBaseStatsCollection GivenBaseStats { get; }

        EffectStatCollection EffectStats { get; }

        StatReplacerCollection StatReplacers { get; }
            
        FormMatcherCollection FormMatchers { get; }

        FormAndStatMatcherCollection FormAndStatMatchers { get; }

        StatMatcherCollection StatMatchers { get; }

        StatMatcherCollection<IDamageStatProvider> DamageStatMatchers { get; }

        StatMatcherCollection<IPoolStatProvider> PoolStatMatchers { get; }

        DamageTypeMatcherCollection DamageTypeMatchers { get; }

        ChargeTypeMatcherCollection ChargeTypeMatchers { get; }

        AilmentMatcherCollection AilmentMatchers { get; }

        FlagMatcherCollection FlagMatchers { get; }

        KeywordMatcherCollection KeywordMatchers { get; }

        IReadOnlyDictionary<string, ItemSlot> ItemSlotMatchers { get; }

        ActionMatcherCollection ActionMatchers { get; }

        ConditionMatcherCollection ConditionMatchers { get; }

        ValueConversionMatcherCollection ValueConversionMatchers { get; }

        StatManipulatorMatcherCollection StatManipulationMatchers { get; }

        SpecialMatcherCollection SpecialMatchers { get; }

        StatMatcherCollection PropertyMatchers { get; }
    }


    public class GivenBaseStatsCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // IFormProvider is FormProviders.BaseAdd
        public void Add(IStatProvider stat, ValueProvider value)
        {
            throw new NotImplementedException();
        }
    }


    public class EffectStatCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IEffectProvider effect, params string[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add(IEffectProvider effect, params IFlagStatProvider[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add(IFlagStatProvider stat, params string[] stats)
        {
            throw new NotImplementedException();
        }
    }


    public class StatReplacerCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add([RegexPattern] string originalStat, params string[] replacements)
        {
            throw new NotImplementedException();
        }
    }
}