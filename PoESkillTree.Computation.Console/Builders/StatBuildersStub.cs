using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuildersStub : IStatBuilders
    {
        private readonly IStatFactory _statFactory = new StatFactory();

        public IStatBuilder Level => CreateStat("Level");

        public IStatBuilder Armour => CreateStat("Armour");

        public IEvasionStatBuilder Evasion => new EvasionStatBuilder(_statFactory);

        public IDamageRelatedStatBuilder Accuracy => CreateDamageStat("Accuracy");

        public IStatBuilder MovementSpeed => CreateStat("Movement Speed");
        public IStatBuilder AnimationSpeed => CreateStat("Animation Speed");

        public IDamageRelatedStatBuilder CastSpeed => CreateDamageStat("Attack/Cast Speed");
        public IStatBuilder EffectivenessOfAddedDamage => CreateStat("Effectiveness of added damage");
        public IStatBuilder AreaOfEffect => CreateStat("Area of effect");
        public IStatBuilder Range => CreateStat("Range");
        public IStatBuilder CooldownRecoverySpeed => CreateStat("Cooldown recovery speed");
        public IStatBuilder Duration => CreateStat("Duration");

        public ITrapStatBuilders Trap => new TrapStatBuildersStub();
        public IMineStatBuilders Mine => new MineStatBuildersStub();
        public ISkillEntityStatBuilders Totem => new TotemStatBuildersStub();

        public IStatBuilder ItemQuantity => CreateStat("Item Quantity");
        public IStatBuilder ItemRarity => CreateStat("Item Rarity");

        public IStatBuilder PrimordialJewelsSocketed => CreateStat("Socketed Primordial jewels");
        public IStatBuilder GrandSpectrumJewelsSocketed => CreateStat("Socketed Grand Spectrum jewels");

        public IStatBuilder RampageStacks => CreateStat("Rampage Stacks");

        public IAttributeStatBuilders Attribute => new AttributeStatBuildersStub();
        public IPoolStatBuilders Pool => new PoolStatBuildersStub();
        public IDodgeStatBuilders Dodge => new DodgeStatBuildersStub();
        public IFlaskStatBuilders Flask => new FlaskStatBuildersStub();
        public IProjectileStatBuilders Projectile => new ProjectileStatBuildersStub();
        public IFlagStatBuilders Flag => new FlagStatBuildersStub();
        public IGemStatBuilders Gem => new GemStatBuildersStub();

        public IStatBuilder Unique(string name) => CreateStat(name);
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


    public class GemStatBuildersStub : IGemStatBuilders
    {
        public IStatBuilder IncreaseLevel(bool onlySupportGems = false) =>
            CreateStat(onlySupportGems ? "Level of socketed support gems" : "Level of socketed gems");
    }


    public class ProjectileStatBuildersStub : IProjectileStatBuilders
    {
        public IStatBuilder Speed => CreateStat("Projectile speed");
        public IStatBuilder Count => CreateStat("Projectile count");

        public IStatBuilder PierceCount => CreateStat("Projectile pierce count");
        public IStatBuilder ChainCount => CreateStat("Projectile chain count");
        public IStatBuilder TravelDistance => CreateStat("Projectile travel distance");
    }


    public class TrapStatBuildersStub : ITrapStatBuilders
    {
        public IStatBuilder Speed => CreateStat("Trap throwing speed");
        public IStatBuilder Duration => CreateStat("Trap duration");
        public IStatBuilder TriggerAoE => CreateStat("Trap trigger AoE");
    }


    public class MineStatBuildersStub : IMineStatBuilders
    {
        public IStatBuilder Speed => CreateStat("Mine laying speed");
        public IStatBuilder Duration => CreateStat("Mine duration");
        public IStatBuilder DetonationAoE => CreateStat("Mine detonation AoE");
    }


    public class TotemStatBuildersStub : ISkillEntityStatBuilders
    {
        public IStatBuilder Speed => CreateStat("Totem placement speed");
        public IStatBuilder Duration => CreateStat("Totem duration");
    }
}