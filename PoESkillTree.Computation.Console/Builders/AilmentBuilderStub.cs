using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class AilmentBuilderStub : AvoidableEffectBuilderStub, IAilmentBuilder
    {
        public AilmentBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Chance => ChanceOn(new EnemyBuilderStub(ConditionBuilders));

        public IStatBuilder InstancesOn(IEntityBuilder target) =>
            new StatBuilderStub($"Number of {this} instances on {target}", ConditionBuilders);

        public IFlagStatBuilder AddSource(IDamageTypeBuilder type) =>
            new FlagStatBuilderStub($"{type} can apply {this}", ConditionBuilders);

        public IFlagStatBuilder AddSources(IEnumerable<IDamageTypeBuilder> types) =>
            new FlagStatBuilderStub($"[{string.Join(", ", types)}] can apply {this}",
                ConditionBuilders);
    }


    public class AilmentBuilderCollectionStub 
        : BuilderCollectionStub<IAilmentBuilder>, IAilmentBuilderCollection
    {
        public AilmentBuilderCollectionStub(IReadOnlyList<IAilmentBuilder> elements, 
            IConditionBuilders conditionBuilders) : base(elements, conditionBuilders)
        {
        }
    }


    public class AilmentBuildersStub : IAilmentBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public AilmentBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;

            IAilmentBuilder[] all = { Ignite, Shock, Chill, Freeze, Bleed, Poison };
            All = new AilmentBuilderCollectionStub(all, conditionBuilders);
            IAilmentBuilder[] elemental = { Ignite, Shock, Chill, Freeze };
            Elemental = new AilmentBuilderCollectionStub(elemental, conditionBuilders);
        }

        public IAilmentBuilder Ignite => new AilmentBuilderStub("Ignite", _conditionBuilders);
        public IAilmentBuilder Shock => new AilmentBuilderStub("Shock", _conditionBuilders);
        public IAilmentBuilder Chill => new AilmentBuilderStub("Chill", _conditionBuilders);
        public IAilmentBuilder Freeze => new AilmentBuilderStub("Freeze", _conditionBuilders);
        public IAilmentBuilder Bleed => new AilmentBuilderStub("Bleed", _conditionBuilders);
        public IAilmentBuilder Poison => new AilmentBuilderStub("Poison", _conditionBuilders);

        public IAilmentBuilderCollection All { get; }
        public IAilmentBuilderCollection Elemental { get; }
    }
}