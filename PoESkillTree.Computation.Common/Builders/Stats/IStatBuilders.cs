using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats.
    /// </summary>
    public interface IStatBuilders
    {
        IStatBuilder Level { get; }
        IStatBuilder CharacterClass { get; }
        IStatBuilder PassivePoints { get; }
        IStatBuilder AscendancyPassivePoints { get; }

        IStatBuilder Armour { get; }

        IEvasionStatBuilder Evasion { get; }


        IDamageRelatedStatBuilder Accuracy { get; }

        IDamageRelatedStatBuilder ChanceToHit { get; }


        /// <summary>
        /// Gets a stat representing the multiplier to Self's movement speed.
        /// </summary>
        IStatBuilder MovementSpeed { get; }

        /// <summary>
        /// Gets a stat representing the multiplier to Self's action/animation speed. This acts like a more modifier
        /// to all kinds of speeds (movement, cast, trap throwing, ...).
        /// </summary>
        IStatBuilder ActionSpeed { get; }


        /// <summary>
        /// The rate with which to cast (or attack with) the main skill (casts per second)
        /// </summary>
        IDamageRelatedStatBuilder CastRate { get; }

        /// <summary>
        /// The main skill's cast time (in seconds), used to initialize CastRate
        /// </summary>
        IDamageRelatedStatBuilder BaseCastTime { get; }

        /// <summary>
        /// The rate at which the main skill hits, for skills where that rate is independent from CastRate.
        /// </summary>
        IStatBuilder HitRate { get; }

        /// <summary>
        /// A secondary cast rate with which to cast the main skill (casts per second). Combined with CastRate to
        /// get the real casts per second.
        /// </summary>
        IStatBuilder AdditionalCastRate { get; }

        IStatBuilder DamageHasKeyword(DamageSource damageSource, IKeywordBuilder keyword);

        /// <summary>
        /// Gets a stat representing the modifier to the main skill's area of effect. The square root of this stat is
        /// used as a modifier to <see cref="Radius"/>.
        /// </summary>
        IStatBuilder AreaOfEffect { get; }

        /// <summary>
        /// Gets a stat representing the main skill's radius, if it has one.
        /// </summary>
        IStatBuilder Radius { get; }

        /// <summary>
        /// Gets a stat representing the weapon range
        /// </summary>
        IDamageRelatedStatBuilder Range { get; }

        /// <summary>
        /// The main skill's cooldown
        /// </summary>
        IStatBuilder Cooldown { get; }

        /// <summary>
        /// The main skill's cooldown recovery speed
        /// </summary>
        IStatBuilder CooldownRecoverySpeed { get; }

        /// <summary>
        /// The main skill's duration in seconds.
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// The duration in seconds of secondary effects of the main skill.
        /// </summary>
        IStatBuilder SecondaryDuration { get; }

        IStatBuilder SkillStage { get; }
        IStatBuilder MainSkillPart { get; }


        ITrapStatBuilders Trap { get; }

        IMineStatBuilders Mine { get; }

        ISkillEntityStatBuilders Totem { get; }


        IStatBuilder ItemQuantity { get; }

        IStatBuilder ItemRarity { get; }


        /// <summary>
        /// Gets a stat representing the number of primordial jewels socketed into the skill tree.
        /// </summary>
        IStatBuilder PrimordialJewelsSocketed { get; }

        /// <summary>
        /// Gets a stat representing the number of grand spectrum jewels socketed into the skill tree.
        /// </summary>
        IStatBuilder GrandSpectrumJewelsSocketed { get; }

        /// <summary>
        /// Gets a stat representing the number of currently active rampage stacks.
        /// </summary>
        IStatBuilder RampageStacks { get; }

        /// <summary>
        /// The percentage of damage taken gained as mana over 4 seconds.
        /// </summary>
        IStatBuilder DamageTakenGainedAsMana { get; }

        /// <summary>
        /// The number of brands attached to Self.
        /// </summary>
        IStatBuilder AttachedBrands { get; }

        IStatBuilder BannerStage { get; }

        /// <summary>
        /// Returns the value of a stat with type uint that can only be specified by the user.
        /// </summary>
        ValueBuilder UniqueAmount(string name);

        ValueBuilder Unique<T>(string name) where T: struct, System.Enum;

        IStatBuilder IndependentMultiplier(string identity);
        IStatBuilder IndependentTotal(string identity);


        // Stats from sub factories

        IAttributeStatBuilders Attribute { get; }
        IRequirementStatBuilders Requirements { get; }

        IPoolStatBuilders Pool { get; }

        IDodgeStatBuilders Dodge { get; }

        IFlaskStatBuilders Flask { get; }

        IProjectileStatBuilders Projectile { get; }

        IFlagStatBuilders Flag { get; }

        IGemStatBuilders Gem { get; }
    }
}