using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EntityBuilderStub : BuilderStub, IEntityBuilder
    {
        private readonly Resolver<IEntityBuilder> _resolver;

        public EntityBuilderStub(string stringRepresentation, Resolver<IEntityBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public static EntityBuilderStub Self() => new EntityBuilderStub("Self", (c, _) => c);

        protected IEntityBuilder This => this;

        public IEntityBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);

        public IReadOnlyCollection<Entity> Build(Entity modifierSourceEntity) => new Entity[0];
    }


    public class EntityBuildersStub : IEntityBuilders
    {
        public IEntityBuilder Self => EntityBuilderStub.Self();
        public IEnemyBuilder Enemy => new EnemyBuilderStub();
        public IEntityBuilder Ally => new EntityBuilderStub("Ally", (c, _) => c);
        public IEntityBuilder ModifierSource => new EntityBuilderStub("Modifier Source", (c, _) => c);

        public IEntityBuilder Totem => new EntityBuilderStub("Totem", (c, _) => c);

        public IEntityBuilder Minion => new EntityBuilderStub("Minion", (c, _) => c);
    }


    public class EnemyBuilderStub : EntityBuilderStub, IEnemyBuilder
    {
        public EnemyBuilderStub()
            : base("Enemy", (c, _) => c)
        {
        }

        public IConditionBuilder IsNearby =>
            CreateCondition(This, o => $"{o} is nearby");

        public IConditionBuilder IsRare =>
            CreateCondition(This, o => $"{o} is rare");

        public IConditionBuilder IsUnique =>
            CreateCondition(This, o => $"{o} is unique");

        public IConditionBuilder IsRareOrUnique =>
            CreateCondition(This, o => $"{o} is rare or unique");
    }
}