using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats shared between all entities.
    /// </summary>
    public class CommonGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public CommonGivenStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // Rampage
            "1% increased Movement Speed per 10 Rampage Stacks",
            "2% increased Damage per 10 Rampage Stacks",
            // other
            "100% of non-chaos damage is taken from energy shield before life",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // pools
            { BaseSet, EnergyShield.Recharge, 20 },
            { BaseSet, Life.RecoveryRate, 1 },
            { BaseSet, Mana.RecoveryRate, 1 },
            { BaseSet, EnergyShield.RecoveryRate, 1 },
            { BaseSet, Life.Regen.TargetPool, (int) Pool.Life },
            { BaseSet, Mana.Regen.TargetPool, (int) Pool.Mana },
            { BaseSet, EnergyShield.Regen.TargetPool, (int) Pool.EnergyShield },
            { BaseSet, EnergyShield.Recharge.Start, 1 },
            { BaseSet, AllSkills.ReservationPool, (int) Pool.Mana },
            // flasks
            { BaseSet, Flask.Effect, 1 },
            { BaseSet, Flask.RecoverySpeed, 1 },
            // speed
            { BaseSet, Stat.ActionSpeed, 1 },
            { BaseSet, Stat.MovementSpeed, 1 },
            // crit
            { BaseSet, CriticalStrike.Chance.Maximum, 95 },
            { BaseSet, CriticalStrike.Chance.Minimum, 5 },
            // evasion
            { BaseSet, Evasion.Chance.Maximum, 95 },
            { BaseSet, Evasion.Chance.Minimum, 5 },
            { BaseSet, Stat.ChanceToHit.Maximum, 95 },
            { BaseSet, Stat.ChanceToHit.Minimum, 5 },
            // block
            { BaseSet, Block.AttackChance.Maximum, 75 },
            { BaseSet, Block.SpellChance.Maximum, 75 },
            // dodge
            { BaseSet, Stat.Dodge.AttackChance.Maximum, 75 },
            { BaseSet, Stat.Dodge.SpellChance.Maximum, 75 },
            // charges
            { BaseSet, Charge.Endurance.Amount.Maximum, 3 },
            { BaseSet, Charge.Frenzy.Amount.Maximum, 3 },
            { BaseSet, Charge.Power.Amount.Maximum, 3 },
            // Rampage
            { BaseSet, Stat.RampageStacks.Maximum, 1000 },
            // leech
            { BaseSet, Life.Leech.RateLimit, 20 },
            { BaseSet, Mana.Leech.RateLimit, 20 },
            { BaseSet, Life.Leech.Rate, 2 },
            { BaseSet, Mana.Leech.Rate, 2 },
            { BaseSet, Life.Leech.TargetPool, (int) Pool.Life },
            { BaseSet, Mana.Leech.TargetPool, (int) Pool.Mana },
            // resistances
            { BaseSet, Elemental.Resistance.Maximum, 75 },
            { BaseSet, Chaos.Resistance.Maximum, 75 },
            { BaseSet, Damage.Taken, 1 },
            // ailments
            { BaseSet, Ailment.Ignite.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Ignite.Source(Fire), 1 },
            { BaseSet, Ailment.Ignite.Duration, 4000 },
            { TotalOverride, Ailment.Ignite.CriticalStrikesAlwaysInflict, 1 },
            { PercentLess, Damage.With(Ailment.Ignite), 50 },
            { BaseSet, Ailment.Shock.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Shock.Source(Lightning), 1 },
            { BaseSet, Ailment.Shock.Duration, 2000 },
            { TotalOverride, Ailment.Shock.CriticalStrikesAlwaysInflict, 1 },
            { BaseSet, Ailment.ShockEffect, 1 },
            { BaseSet, Ailment.Chill.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Chill.Source(Cold), 1 },
            { BaseSet, Ailment.Chill.Duration, 2000 },
            { BaseAdd, Ailment.Chill.Chance, 100 },
            { BaseSet, Ailment.ChillEffect, 1 },
            { BaseSet, Ailment.Freeze.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Freeze.Source(Cold), 1 },
            { BaseSet, Ailment.Freeze.Duration, 60 },
            { TotalOverride, Ailment.Freeze.CriticalStrikesAlwaysInflict, 1 },
            { BaseSet, Ailment.Bleed.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Bleed.Source(Physical), 1 },
            { BaseSet, Ailment.Bleed.Duration, 5000 },
            { PercentLess, Damage.With(Ailment.Bleed), 30 },
            { PercentMore, Damage.With(Ailment.Bleed), 100, Enemy.IsMoving },
            { BaseSet, Ailment.Poison.Source(Physical.And(Chaos)), 1 },
            { BaseSet, Ailment.Poison.Duration, 2000 },
            { PercentLess, Damage.With(Ailment.Poison), 80 },
            // buffs
            { BaseSet, Buff.CurseLimit, 1 },
            { BaseSet, Buffs(Self).Effect, 1 },
            { BaseSet, Buff.Fortify.Duration, 4000 },
            { BaseSet, Buff.Maim.Duration, 4000 },
            { BaseSet, Buff.Taunt.Duration, 3000 },
            { BaseSet, Buff.ArcaneSurge.Duration, 4000 },
            // stun
            { BaseSet, Effect.Stun.Threshold, 1 },
            { BaseSet, Effect.Stun.Recovery, 1 },
            { BaseSet, Effect.Stun.Duration, 350 },
        };
    }
}