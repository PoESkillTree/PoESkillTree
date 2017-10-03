using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class AilmentBuilderStub : AvoidableEffectBuilderStub, IAilmentBuilder
    {
        public AilmentBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Chance => ChanceOn(new EnemyBuilderStub());

        public IStatBuilder InstancesOn(IEntityBuilder target) =>
            new StatBuilderStub($"Number of {this} instances on {target}");

        public IFlagStatBuilder AddSource(IDamageTypeBuilder type) =>
            new FlagStatBuilderStub($"{type} can apply {this}");

        public IFlagStatBuilder AddSources(IEnumerable<IDamageTypeBuilder> types) =>
            new FlagStatBuilderStub($"[{string.Join(", ", types)}] can apply {this}");
    }


    public class AilmentBuilderCollectionStub 
        : BuilderCollectionStub<IAilmentBuilder>, IAilmentBuilderCollection
    {
        public AilmentBuilderCollectionStub(IReadOnlyList<IAilmentBuilder> elements) 
            : base(elements)
        {
        }
    }


    public class AilmentBuildersStub : IAilmentBuilders
    {
        public AilmentBuildersStub()
        {
            IAilmentBuilder[] all = { Ignite, Shock, Chill, Freeze, Bleed, Poison };
            All = new AilmentBuilderCollectionStub(all);
            IAilmentBuilder[] elemental = { Ignite, Shock, Chill, Freeze };
            Elemental = new AilmentBuilderCollectionStub(elemental);
        }

        public IAilmentBuilder Ignite => new AilmentBuilderStub("Ignite");
        public IAilmentBuilder Shock => new AilmentBuilderStub("Shock");
        public IAilmentBuilder Chill => new AilmentBuilderStub("Chill");
        public IAilmentBuilder Freeze => new AilmentBuilderStub("Freeze");
        public IAilmentBuilder Bleed => new AilmentBuilderStub("Bleed");
        public IAilmentBuilder Poison => new AilmentBuilderStub("Poison");

        public IAilmentBuilderCollection All { get; }
        public IAilmentBuilderCollection Elemental { get; }
    }
}