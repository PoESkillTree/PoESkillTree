using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public abstract class LevelBasedStats : UsesStatBuilders, IGivenStats
    {
        protected IModifierBuilder ModifierBuilder { get; }
        protected MonsterBaseStats MonsterBaseStats { get; }
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        protected LevelBasedStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, MonsterBaseStats monsterBaseStats)
            : base(builderFactories)
        {
            ModifierBuilder = modifierBuilder;
            MonsterBaseStats = monsterBaseStats;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public abstract IReadOnlyList<Entity> AffectedEntities { get; }

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        protected abstract GivenStatCollection CreateCollection();

        protected ValueBuilder LevelBased(Func<int, double> selector, string identity)
            => Stat.Level.For(Enemy).Value.Select(v => selector((int) v), v => $"{identity}(level: {v})");
    }
}