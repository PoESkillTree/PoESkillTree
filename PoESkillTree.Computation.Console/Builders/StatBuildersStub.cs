using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuildersStub :  IStatBuilders
    {
        public IStatBuilder Armour => CreateStat("Armour");

        public IEvasionStatBuilder Evasion => new EvasionStatBuilderStub();

        public IStatBuilder Accuracy => CreateStat("Accuracy");

        public IStatBuilder MovementSpeed => CreateStat("Movement Speed");
        public IStatBuilder AnimationSpeed => CreateStat("Animation Speed");

        public IStatBuilder Range => CreateStat("Range");

        public IStatBuilder TrapTriggerAoE => CreateStat("Trap trigger AoE");
        public IStatBuilder MineDetonationAoE => CreateStat("Mine detonation AoE");

        public IStatBuilder ItemQuantity => CreateStat("Item Quantity");
        public IStatBuilder ItemRarity => CreateStat("Item Rarity");

        public IStatBuilder PrimordialJewelsSocketed => CreateStat("Socketed Primoridal jewels");
        public IStatBuilder GrandSpectrumJewelsSocketed => CreateStat("Socketed Grand Spectrum jewels");

        public IStatBuilder RampageStacks => CreateStat("Rampage Stacks");

        public IAttributeStatBuilders Attribute => new AttributeStatBuildersStub();
        public IPoolStatBuilders Pool => new PoolStatBuildersStub();
        public IDodgeStatBuilders Dodge => new DodgeStatBuildersStub();
        public IFlaskStatBuilders Flask => new FlaskStatBuildersStub();
        public IProjectileStatBuilders Projectile => new ProjectileStatBuildersStub();
        public IFlagStatBuilders Flag => new FlagStatBuildersStub();
        public IGemStatBuilders Gem => new GemStatBuildersStub();

        public IStatBuilder ApplyOnce(params IStatBuilder[] stats) =>
            CreateStat(stats, os => $"ApplyOnce({string.Join(", ", os)})");

        public IStatBuilder Unique(string name = "$0") => CreateStat(name);
    }


    public class AttributeStatBuildersStub : IAttributeStatBuilders
    {
        public IStatBuilder Strength => CreateStat("Strength");
        public IStatBuilder Dexterity => CreateStat("Dexterity");
        public IStatBuilder Intelligence => CreateStat("Intelligence");
        public IStatBuilder StrengthDamageBonus => CreateStat("Strength damage bonus");
        public IStatBuilder DexterityEvasionBonus => CreateStat("Dexterity evasion bonus");
    }


    public class DodgeStatBuildersStub : IDodgeStatBuilders
    {
        public IStatBuilder AttackChance => CreateStat("Chance to dodge attacks");
        public IStatBuilder SpellChance => CreateStat("Chance to dodge spells");
    }


    public class FlaskStatBuildersStub : IFlaskStatBuilders
    {
        public IStatBuilder Effect => CreateStat("Flask effect");
        public IStatBuilder Duration => CreateStat("Flask effect duration");
        public IStatBuilder LifeRecovery => CreateStat("Flask life recovery");
        public IStatBuilder ManaRecovery => CreateStat("Flask mana recovery");
        public IStatBuilder RecoverySpeed => CreateStat("Flask recovery speed");
        public IStatBuilder ChargesUsed => CreateStat("Flask charges used");
        public IStatBuilder ChargesGained => CreateStat("Flask charges gained");

        public IConditionBuilder IsAnyActive => CreateCondition("Any flask is active");
    }


    public class GemStatBuildersStub :  IGemStatBuilders
    {
        public IStatBuilder IncreaseLevel(bool onlySupportGems = false) =>
            CreateStat(onlySupportGems ? "Level of socketed support gems" : "Level of socketed gems");
    }


    public class ProjectileStatBuildersStub : IProjectileStatBuilders
    {
        public IStatBuilder Speed => CreateStat("Projectile speed");
        public IStatBuilder Count => CreateStat("Projectile count");

        public IStatBuilder PierceCount => CreateStat("Projectile pierce count");

        public ISelfToAnyActionBuilder Pierce =>
            new SelfToAnyActionBuilderStub("Projectile pierce", (c, _) => c);

        public IStatBuilder ChainCount => CreateStat("Projectile chain count");
        public IStatBuilder TravelDistance => CreateStat("Projectile travel distance");
    }
}