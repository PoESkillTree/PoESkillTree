using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Steps;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// Implementation of <see cref="IParsingData{T}"/> using <see cref="Stepper"/> and the matcher implementations in
    /// this namespace.
    /// </summary>
    public class ParsingData : IParsingData<ParsingStep>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMatchContexts _matchContexts;

        private readonly Lazy<IReadOnlyList<IStatMatchers>> _statMatchers;

        private readonly Lazy<IReadOnlyList<IReferencedMatchers>> _referencedMatchers;

        private readonly Lazy<IReadOnlyList<StatReplacerData>> _statReplacers =
            new Lazy<IReadOnlyList<StatReplacerData>>(() => new StatReplacers().Replacers);

        private readonly Lazy<IStepper<ParsingStep>> _stepper =
            new Lazy<IStepper<ParsingStep>>(() => new Stepper());

        private readonly Lazy<StatMatchersSelector> _statMatchersSelector;

        public ParsingData(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IReferencedMatchers skillMatchers)
        {
            _builderFactories = builderFactories;
            _matchContexts = matchContexts;

            _statMatchers = new Lazy<IReadOnlyList<IStatMatchers>>(
                () => CreateStatMatchers(new ModifierBuilder()));
            _referencedMatchers = new Lazy<IReadOnlyList<IReferencedMatchers>>(
                () => CreateReferencedMatchers(skillMatchers));
            _statMatchersSelector = new Lazy<StatMatchersSelector>(
                () => new StatMatchersSelector(StatMatchers));
        }

        public IReadOnlyList<IStatMatchers> StatMatchers => _statMatchers.Value;

        public IReadOnlyList<IReferencedMatchers> ReferencedMatchers => _referencedMatchers.Value;

        public IReadOnlyList<StatReplacerData> StatReplacers => _statReplacers.Value;

        public IStepper<ParsingStep> Stepper => _stepper.Value;

        public IStatMatchers SelectStatMatcher(ParsingStep step) => _statMatchersSelector.Value.Get(step);

        private IReadOnlyList<IStatMatchers> CreateStatMatchers(IModifierBuilder modifierBuilder) =>
            new IStatMatchers[]
            {
                new SpecialMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new StatManipulatorMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new ValueConversionMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new FormAndStatMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new FormMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new GeneralStatMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new DamageStatMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new PoolStatMatchers(_builderFactories, _matchContexts, modifierBuilder),
                new ConditionMatchers(_builderFactories, _matchContexts, modifierBuilder),
            };

        private IReadOnlyList<IReferencedMatchers> CreateReferencedMatchers(IReferencedMatchers skillMatchers) =>
            new[]
            {
                new ActionMatchers(_builderFactories.ActionBuilders),
                new AilmentMatchers(_builderFactories.EffectBuilders.Ailment),
                new ChargeTypeMatchers(_builderFactories.ChargeTypeBuilders),
                new DamageTypeMatchers(_builderFactories.DamageTypeBuilders),
                new BuffMatchers(_builderFactories.BuffBuilders),
                new ItemSlotMatchers(_builderFactories.ItemSlotBuilders),
                new KeywordMatchers(_builderFactories.KeywordBuilders),
                skillMatchers,
            };
    }
}