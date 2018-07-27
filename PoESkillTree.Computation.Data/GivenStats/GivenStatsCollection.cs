using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class GivenStatsCollection : IReadOnlyCollection<IGivenStats>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly Lazy<IReadOnlyList<IGivenStats>> _lazyCollection;

        public GivenStatsCollection(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
        {
            _builderFactories = builderFactories;
            _metaStatBuilders = metaStatBuilders;
            _lazyCollection = new Lazy<IReadOnlyList<IGivenStats>>(() => CreateCollection(new ModifierBuilder()));
        }

        public IEnumerator<IGivenStats> GetEnumerator() => _lazyCollection.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _lazyCollection.Value.Count;

        private IReadOnlyList<IGivenStats> CreateCollection(IModifierBuilder modifierBuilder)
            => new IGivenStats[]
            {
                new CommonGivenStats(_builderFactories, modifierBuilder),
                new CharacterGivenStats(_builderFactories, modifierBuilder),
                new MonsterGivenStats(_builderFactories, modifierBuilder),
                new TotemGivenStats(_builderFactories, modifierBuilder),
                new EffectStats(_builderFactories, modifierBuilder),
                new DataDrivenMechanics(_builderFactories, modifierBuilder, _metaStatBuilders),
            };
    }
}