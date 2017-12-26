using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Console
{
    public class CompositionRoot
    {
        private readonly Lazy<IBuilderFactories> _builderFactories =
            new Lazy<IBuilderFactories>(() => new BuilderFactories());

        private readonly Lazy<IReadOnlyList<IStatMatchers>> _statMatchers;
        private readonly Lazy<IReadOnlyList<IReferencedMatchers>> _referencedMatchers;

        private readonly Lazy<IParser> _parser;

        public CompositionRoot()
        {
            _statMatchers = new Lazy<IReadOnlyList<IStatMatchers>>(
                () => CreateStatMatchers(_builderFactories.Value, new MatchContextsStub(), new ModifierBuilder()));
            _referencedMatchers = new Lazy<IReadOnlyList<IReferencedMatchers>>(
                () => CreateReferencedMatchers(_builderFactories.Value));
            _parser = new Lazy<IParser>(
                () => new Parser(ReferencedMatchers, StatMatchers, new StatReplacers().Replacers,
                    _builderFactories.Value));
        }

        public IReadOnlyList<IStatMatchers> StatMatchers => _statMatchers.Value;
        public IReadOnlyList<IReferencedMatchers> ReferencedMatchers => _referencedMatchers.Value;

        public IParser Parser => _parser.Value;

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
            new ItemSlotMatchers(new ItemSlotBuildersStub()),
            new KeywordMatchers(builderFactories.KeywordBuilders),
            new SkillMatchers(),
        };
    }
}