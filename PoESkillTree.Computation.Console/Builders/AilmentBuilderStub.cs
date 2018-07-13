using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class AilmentBuilderStub : AvoidableEffectBuilderStub, IAilmentBuilder
    {
        public AilmentBuilderStub(string stringRepresentation, Resolver<IEffectBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Chance => ChanceOn(new EntityBuilder(Entity.Enemy));

        public IStatBuilder InstancesOn(IEntityBuilder target) =>
            CreateStat(This, target, (o1, o2) => $"Number of {o1} instances on {o2}");

        public IFlagStatBuilder Source(IDamageTypeBuilder type) =>
            CreateFlagStat(This, (IKeywordBuilder) type, (o1, o2) => $"{type} can apply {this}");

        public new Ailment Build() => default;
    }
}