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
        public EntityBuilderStub(string stringRepresentation) 
            : base(stringRepresentation)
        {
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
            => new DamageStatBuilderStub($"{stat} for {this}");

        public IFlagStatBuilder Stat(IFlagStatBuilder stat)
            => new FlagStatBuilderStub($"{stat} for {this}");

        public IPoolStatBuilder Stat(IPoolStatBuilder stat)
            => new PoolStatBuilderStub($"{stat} for {this}");

        public IStatBuilder Stat(IStatBuilder stat)
            => new StatBuilderStub($"{stat} for {this}");

        public IStatBuilder Level =>
            new StatBuilderStub($"{this} Level");
    }


    public class EntityBuildersStub : IEntityBuilders
    {
        public ISelfBuilder Self => new SelfBuilderStub();
        public IEnemyBuilder Enemy => new EnemyBuilderStub();
        public IEntityBuilder Ally => new EntityBuilderStub("Ally");
        public IEntityBuilder Character => new EntityBuilderStub("Character");

        public ISkillEntityBuilder Totem => new SkillEntityBuilderStub("Totem");

        public ISkillEntityBuilder Minion =>
            new SkillEntityBuilderStub("Minion");

        public IEntityBuilder Any => new EntityBuilderStub("Any Entity");
    }


    public class SelfBuilderStub : EntityBuilderStub, ISelfBuilder
    {
        public SelfBuilderStub() 
            : base("Self")
        {
        }
    }


    public class EnemyBuilderStub : EntityBuilderStub, IEnemyBuilder
    {
        public EnemyBuilderStub() 
            : base("Enemy")
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
        public SkillEntityBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public ISkillEntityBuilder With(IKeywordBuilder keyword) =>
            new SkillEntityBuilderStub($"{this} with {keyword}");

        public ISkillEntityBuilder With(params IKeywordBuilder[] keywords) =>
            new SkillEntityBuilderStub($"{this} with {keywords}");

        public ISkillEntityBuilder From(ISkillBuilder skill) =>
            new SkillEntityBuilderStub($"{this} from {skill}");
    }
}