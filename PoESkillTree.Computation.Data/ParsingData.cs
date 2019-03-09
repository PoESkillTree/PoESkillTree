using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.GivenStats;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// Implementation of <see cref="IParsingData{T}"/> using <see cref="Stepper"/> and the matcher implementations in
    /// this namespace.
    /// </summary>
    public class ParsingData : IParsingData<ParsingStep>
    {
        private readonly IBuilderFactories _builderFactories;

        private readonly Lazy<IReadOnlyList<IStatMatchers>> _statMatchers;

        private readonly Lazy<IReadOnlyList<IReferencedMatchers>> _referencedMatchers;

        private readonly Lazy<IReadOnlyList<StatReplacerData>> _statReplacers =
            new Lazy<IReadOnlyList<StatReplacerData>>(() => new StatReplacers().Replacers);

        private readonly Lazy<IReadOnlyList<IGivenStats>> _givenStats;

        private readonly Lazy<IStepper<ParsingStep>> _stepper =
            new Lazy<IStepper<ParsingStep>>(() => new Stepper());

        private readonly Lazy<StatMatchersSelector> _statMatchersSelector;

        private ParsingData(
            IBuilderFactories builderFactories,
            SkillDefinitions skills, PassiveTreeDefinition passives,
            CharacterBaseStats characterBaseStats, MonsterBaseStats monsterBaseStats)
        {
            _builderFactories = builderFactories;

            _statMatchers = new Lazy<IReadOnlyList<IStatMatchers>>(
                () => CreateStatMatchers(ModifierBuilder.Empty, passives.Nodes));
            _referencedMatchers = new Lazy<IReadOnlyList<IReferencedMatchers>>(
                () => CreateReferencedMatchers(skills.Skills));
            _givenStats = new Lazy<IReadOnlyList<IGivenStats>>(
                () => new GivenStatsCollection(builderFactories, characterBaseStats, monsterBaseStats));
            _statMatchersSelector = new Lazy<StatMatchersSelector>(
                () => new StatMatchersSelector(StatMatchers));
        }

        public static async Task<IParsingData<ParsingStep>> CreateAsync(
            GameData gameData, Task<IBuilderFactories> builderFactoriesTask)
        {
            var skillsTask = gameData.Skills;
            var passivesTask = gameData.PassiveTree;
            var characterTask = gameData.CharacterBaseStats;
            var monsterTask = gameData.MonsterBaseStats;
            return new ParsingData(
                await builderFactoriesTask.ConfigureAwait(false),
                await skillsTask.ConfigureAwait(false),
                await passivesTask.ConfigureAwait(false),
                await characterTask.ConfigureAwait(false),
                await monsterTask.ConfigureAwait(false));
        }

        public IReadOnlyList<IStatMatchers> StatMatchers => _statMatchers.Value;

        public IReadOnlyList<IReferencedMatchers> ReferencedMatchers => _referencedMatchers.Value;

        public IReadOnlyList<StatReplacerData> StatReplacers => _statReplacers.Value;

        public IReadOnlyList<IGivenStats> GivenStats => _givenStats.Value;

        public IStepper<ParsingStep> Stepper => _stepper.Value;

        public IStatMatchers SelectStatMatcher(ParsingStep step) => _statMatchersSelector.Value.Get(step);

        private IReadOnlyList<IStatMatchers> CreateStatMatchers(
            IModifierBuilder modifierBuilder, IReadOnlyList<PassiveNodeDefinition> passives)
            => new IStatMatchers[]
            {
                new SpecialMatchers(_builderFactories, modifierBuilder),
                new StatManipulatorMatchers(_builderFactories, modifierBuilder),
                new ValueConversionMatchers(_builderFactories, modifierBuilder),
                new FormAndStatMatchers(_builderFactories, modifierBuilder),
                new KeystoneStatMatchers(_builderFactories, modifierBuilder, passives),
                new FormMatchers(_builderFactories, modifierBuilder),
                new GeneralStatMatchers(_builderFactories, modifierBuilder),
                new DamageStatMatchers(_builderFactories, modifierBuilder),
                new PoolStatMatchers(_builderFactories, modifierBuilder),
                new ConditionMatchers(_builderFactories, modifierBuilder),
                new ActionConditionMatchers(_builderFactories, modifierBuilder),
            };

        private IReadOnlyList<IReferencedMatchers> CreateReferencedMatchers(IReadOnlyList<SkillDefinition> skills) =>
            new IReferencedMatchers[]
            {
                new ActionMatchers(_builderFactories.ActionBuilders, _builderFactories.EffectBuilders),
                new AilmentMatchers(_builderFactories.EffectBuilders.Ailment),
                new ChargeTypeMatchers(_builderFactories.ChargeTypeBuilders),
                new DamageTypeMatchers(_builderFactories.DamageTypeBuilders),
                new BuffMatchers(_builderFactories.BuffBuilders),
                new KeywordMatchers(_builderFactories.KeywordBuilders),
                new SkillMatchers(skills, _builderFactories.SkillBuilders.FromId),
            };
    }
}