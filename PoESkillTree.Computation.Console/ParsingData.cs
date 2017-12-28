using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Console
{
    // TODO do something with SkillMatchers to remove dependencies to the Console project
    //      and move this to Computation.Data
    public class ParsingData : IParsingData<ParsingStep>
    {
        private readonly Lazy<IReadOnlyList<IStatMatchers>> _statMatchers;

        private readonly Lazy<IReadOnlyList<IReferencedMatchers>> _referencedMatchers;

        private readonly Lazy<IReadOnlyList<StatReplacerData>> _statReplacers =
            new Lazy<IReadOnlyList<StatReplacerData>>(() => new StatReplacers().Replacers);

        private readonly Lazy<IStepper<ParsingStep>> _stepper = 
            new Lazy<IStepper<ParsingStep>>(() => new Stepper());

        private readonly Lazy<StatMatchersSelector> _statMatchersSelector;

        public ParsingData(IBuilderFactories builderFactories)
        {
            _statMatchers = new Lazy<IReadOnlyList<IStatMatchers>>(
                () => CreateStatMatchers(builderFactories, new MatchContextsStub(), new ModifierBuilder()));
            _referencedMatchers = new Lazy<IReadOnlyList<IReferencedMatchers>>(
                () => CreateReferencedMatchers(builderFactories));
            _statMatchersSelector = new Lazy<StatMatchersSelector>(
                () => new StatMatchersSelector(StatMatchers));
        }

        public IReadOnlyList<IStatMatchers> StatMatchers => _statMatchers.Value;

        public IReadOnlyList<IReferencedMatchers> ReferencedMatchers => _referencedMatchers.Value;

        public IReadOnlyList<StatReplacerData> StatReplacers => _statReplacers.Value;

        public IStepper<ParsingStep> Stepper => _stepper.Value;

        public IStatMatchers SelectStatMatcher(ParsingStep step) => _statMatchersSelector.Value.Get(step);

        private static IReadOnlyList<IStatMatchers> CreateStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts,
            IModifierBuilder modifierBuilder) => new IStatMatchers[]
        {
            new SpecialMatchers(builderFactories, matchContexts, modifierBuilder),
            new StatManipulatorMatchers(builderFactories, matchContexts, modifierBuilder),
            new ValueConversionMatchers(builderFactories, matchContexts, modifierBuilder),
            new FormAndStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new FormMatchers(builderFactories, matchContexts, modifierBuilder),
            new GeneralStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new DamageStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new PoolStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new ConditionMatchers(builderFactories, matchContexts, modifierBuilder),
        };

        private static IReadOnlyList<IReferencedMatchers> CreateReferencedMatchers(
            IBuilderFactories builderFactories) => new IReferencedMatchers[]
        {
            new ActionMatchers(builderFactories.ActionBuilders),
            new AilmentMatchers(builderFactories.EffectBuilders.Ailment),
            new ChargeTypeMatchers(builderFactories.ChargeTypeBuilders),
            new DamageTypeMatchers(builderFactories.DamageTypeBuilders),
            new FlagMatchers(builderFactories.StatBuilders.Flag),
            new ItemSlotMatchers(builderFactories.ItemSlotBuilders),
            new KeywordMatchers(builderFactories.KeywordBuilders),
            new SkillMatchers(),
        };
    }
}