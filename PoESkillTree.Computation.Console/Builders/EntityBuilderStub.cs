using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EntityBuilderStub : BuilderStub, IEntityBuilder
    {
        protected IConditionBuilders ConditionBuilders { get; }

        public EntityBuilderStub(string stringRepresentation, IConditionBuilders conditionBuilders) 
            : base(stringRepresentation)
        {
            ConditionBuilders = conditionBuilders;
        }

        public IConditionBuilder
            HitByInPastXSeconds(IDamageTypeBuilder damageType, ValueBuilder seconds) =>
            new ConditionBuilderStub(
                $"{this} hit by {damageType} Damage in the past {seconds} seconds");

        public IConditionBuilder
            HitByInPastXSeconds(IDamageTypeBuilder damageType, double seconds) =>
            new ConditionBuilderStub(
                $"{this} hit by {damageType} Damage in the past {seconds} seconds");

        public IConditionBuilder HitByRecently(IDamageTypeBuilder damageType) =>
            new ConditionBuilderStub(
                $"{this} hit by {damageType} Damage recently");

        public IDamageStatBuilder Stat(IDamageStatBuilder stat)
            => new DamageStatBuilderStub($"{stat} for {this}", ConditionBuilders);

        public IFlagStatBuilder Stat(IFlagStatBuilder stat)
            => new FlagStatBuilderStub($"{stat} for {this}", ConditionBuilders);

        public IPoolStatBuilder Stat(IPoolStatBuilder stat)
            => new PoolStatBuilderStub($"{stat} for {this}", ConditionBuilders);

        public IStatBuilder Stat(IStatBuilder stat)
            => new StatBuilderStub($"{stat} for {this}", ConditionBuilders);

        public IStatBuilder Level =>
            new StatBuilderStub($"{this} Level", ConditionBuilders);
    }


    public class EntityBuildersStub : IEntityBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public EntityBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public ISelfBuilder Self => new SelfBuilderStub(_conditionBuilders);
        public IEnemyBuilder Enemy => new EnemyBuilderStub(_conditionBuilders);
        public IEntityBuilder Ally => new EntityBuilderStub("Ally", _conditionBuilders);
        public IEntityBuilder Character => new EntityBuilderStub("Character", _conditionBuilders);

        public ISkillEntityBuilder Totem => new SkillEntityBuilderStub("Totem", _conditionBuilders);

        public ISkillEntityBuilder Minion =>
            new SkillEntityBuilderStub("Minion", _conditionBuilders);

        public IEntityBuilder Any => new EntityBuilderStub("Any Entity", _conditionBuilders);
    }


    public class SelfBuilderStub : EntityBuilderStub, ISelfBuilder
    {
        public SelfBuilderStub(IConditionBuilders conditionBuilders) 
            : base("Self", conditionBuilders)
        {
        }
    }


    public class EnemyBuilderStub : EntityBuilderStub, IEnemyBuilder
    {
        public EnemyBuilderStub(IConditionBuilders conditionBuilders) 
            : base("Enemy", conditionBuilders)
        {
        }

        public IConditionBuilder IsNearby =>
            new ConditionBuilderStub($"{this} is nearby");

        public IConditionBuilder IsRare =>
            new ConditionBuilderStub($"{this} is rare");

        public IConditionBuilder IsUnique =>
            new ConditionBuilderStub($"{this} is unique");

        public IConditionBuilder IsRareOrUnique =>
            new ConditionBuilderStub($"{this} is rare or unique");
    }


    public class SkillEntityBuilderStub : EntityBuilderStub, ISkillEntityBuilder
    {
        public SkillEntityBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public ISkillEntityBuilder With(IKeywordBuilder keyword) =>
            new SkillEntityBuilderStub($"{this} with {keyword}", ConditionBuilders);

        public ISkillEntityBuilder With(params IKeywordBuilder[] keywords) =>
            new SkillEntityBuilderStub($"{this} with {keywords}", ConditionBuilders);

        public ISkillEntityBuilder From(ISkillBuilder skill) =>
            new SkillEntityBuilderStub($"{this} from {skill}", ConditionBuilders);
    }
}