using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public IStatBuilder Chance => ChanceOn(new EnemyBuilderStub());

        public IStatBuilder InstancesOn(IEntityBuilder target) =>
            CreateStat(This, target, (o1, o2) => $"Number of {o1} instances on {o2}");

        public IFlagStatBuilder Source(IDamageTypeBuilder type) =>
            CreateFlagStat(This, (IKeywordBuilder) type, (o1, o2) => $"{type} can apply {this}");

        public IFlagStatBuilder Sources(IEnumerable<IDamageTypeBuilder> types) =>
            CreateFlagStat(This, types.Cast<IKeywordBuilder>(), 
                (o1, o2) => $"[{string.Join(", ", o2)}] can apply {o1}");
    }


    public class AilmentBuilderCollectionStub
        : BuilderCollectionStub<IAilmentBuilder>, IAilmentBuilderCollection
    {
        private readonly IReadOnlyList<IAilmentBuilder> _elements;

        public AilmentBuilderCollectionStub(params IAilmentBuilder[] elements)
            : base(new AilmentBuilderStub("Ailment", (current, _) => current),
                $"[{string.Join<IAilmentBuilder>(", ", elements)}]",
                (current, _) => current)
        {
            _elements = elements;
        }

        public IEnumerator<IAilmentBuilder> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class AilmentBuildersStub : IAilmentBuilders
    {
        private static IAilmentBuilder Create(string stringRepresentation) =>
            new AilmentBuilderStub(stringRepresentation, (current, _) => current);

        public IAilmentBuilder Ignite => Create("Ignite");
        public IAilmentBuilder Shock => Create("Shock");
        public IAilmentBuilder Chill => Create("Chill");
        public IAilmentBuilder Freeze => Create("Freeze");
        public IAilmentBuilder Bleed => Create("Bleed");
        public IAilmentBuilder Poison => Create("Poison");

        public IAilmentBuilderCollection All =>
            new AilmentBuilderCollectionStub(Ignite, Shock, Chill, Freeze, Bleed, Poison);

        public IAilmentBuilderCollection Elemental =>
            new AilmentBuilderCollectionStub(Ignite, Shock, Chill, Freeze);
    }
}