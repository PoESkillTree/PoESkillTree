using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;

namespace PoESkillTree.Computation.Builders.Entities
{
    public class EntityBuilders : IEntityBuilders
    {
        private readonly IStatFactory _statFactory;

        public EntityBuilders(IStatFactory statFactory) => _statFactory = statFactory;

        public IEntityBuilder Self => new ModifierSourceEntityBuilder();
        public IEnemyBuilder Enemy => new EnemyBuilder(_statFactory);
        public IEntityBuilder Ally => new EntityBuilder(Entity.Minion, Entity.Totem);
        public IEntityBuilder Totem => new EntityBuilder(Entity.Totem);
        public IEntityBuilder Minion => new EntityBuilder(Entity.Minion);

        private class EnemyBuilder : EntityBuilder, IEnemyBuilder
        {
            private readonly IStatFactory _statFactory;

            public EnemyBuilder(IStatFactory statFactory) : base(Entity.Enemy) =>
                _statFactory = statFactory;

            public IConditionBuilder IsNearby => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsNearby");
            public IConditionBuilder IsRare => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsNearby");
            public IConditionBuilder IsUnique => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsNearby");
            public IConditionBuilder IsRareOrUnique => IsRare.Or(IsUnique);
        }
    }
}