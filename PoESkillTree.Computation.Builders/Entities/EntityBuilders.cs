using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Builders.Entities
{
    public class EntityBuilders : IEntityBuilders
    {
        private readonly IStatFactory _statFactory;

        public EntityBuilders(IStatFactory statFactory) => _statFactory = statFactory;

        public IEntityBuilder Self => new ModifierSourceEntityBuilder();
        public IEntityBuilder OpponentOfSelf => new ModifierSourceOpponentEntityBuilder();
        public IEnemyBuilder Enemy => new EnemyBuilder(_statFactory);
        public IEntityBuilder Character => new EntityBuilder(Entity.Character);
        public IEntityBuilder Ally => new EntityBuilder(Entity.Minion, Entity.Totem);
        public IEntityBuilder Totem => new EntityBuilder(Entity.Totem);
        public IEntityBuilder Minion => new EntityBuilder(Entity.Minion);
        public IEntityBuilder From(IEnumerable<Entity> entities) => new EntityBuilder(entities.ToArray());

        private class EnemyBuilder : EntityBuilder, IEnemyBuilder
        {
            private readonly IStatFactory _statFactory;

            public EnemyBuilder(IStatFactory statFactory) : base(Entity.Enemy) =>
                _statFactory = statFactory;

            public IConditionBuilder IsNearby => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsNearby",
                ExplicitRegistrationTypes.UserSpecifiedValue());

            public ValueBuilder CountNearby
                => StatBuilderUtils.FromIdentity(_statFactory, "Enemy.CountNearby", typeof(int),
                    ExplicitRegistrationTypes.UserSpecifiedValue()).Value;

            public IConditionBuilder IsRare => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsRare",
                ExplicitRegistrationTypes.UserSpecifiedValue());

            public IConditionBuilder IsUnique => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsUnique",
                ExplicitRegistrationTypes.UserSpecifiedValue());

            public IConditionBuilder IsRareOrUnique => IsRare.Or(IsUnique);

            public IConditionBuilder IsMoving => StatBuilderUtils.ConditionFromIdentity(_statFactory, "Enemy.IsMoving",
                ExplicitRegistrationTypes.UserSpecifiedValue());
        }
    }
}