using System;
using EnumsNET;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class DataDrivenMechanicCollection : GivenStatCollection
    {
        private readonly IBuilderFactories _builderFactories;

        public DataDrivenMechanicCollection(IModifierBuilder modifierBuilder, IBuilderFactories builderFactories)
            : base(modifierBuilder, builderFactories.ValueBuilders)
        {
            _builderFactories = builderFactories;
        }

        public void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat,
            IDamageRelatedStatBuilder valueParameter, Func<IStatBuilder, IValueBuilder> value)
        {
            stat = stat.WithHits;
            valueParameter = valueParameter.WithHits;
            Add(form, stat.With(AttackDamageHand.MainHand), value(valueParameter.With(AttackDamageHand.MainHand)));
            Add(form, stat.With(AttackDamageHand.OffHand), value(valueParameter.With(AttackDamageHand.OffHand)));
            Add(form, stat.With(DamageSource.Spell), value(valueParameter.With(DamageSource.Spell)));
            Add(form, stat.With(DamageSource.Secondary), value(valueParameter.With(DamageSource.Secondary)));
        }

        public void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat,
            IDamageRelatedStatBuilder valueParameter1, IDamageRelatedStatBuilder valueParameter2,
            Func<IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            stat = stat.WithHits;
            valueParameter1 = valueParameter1.WithHits;
            valueParameter2 = valueParameter2.WithHits;
            Add(form, stat.With(AttackDamageHand.MainHand), value(
                valueParameter1.With(AttackDamageHand.MainHand), valueParameter2.With(AttackDamageHand.MainHand)));
            Add(form, stat.With(AttackDamageHand.OffHand), value(
                valueParameter1.With(AttackDamageHand.OffHand), valueParameter2.With(AttackDamageHand.OffHand)));
            Add(form, stat.With(DamageSource.Spell), value(
                valueParameter1.With(DamageSource.Spell), valueParameter2.With(DamageSource.Spell)));
            Add(form, stat.With(DamageSource.Secondary), value(
                valueParameter1.With(DamageSource.Secondary), valueParameter2.With(DamageSource.Secondary)));
        }

        public void Add(
            IFormBuilder form, Func<IPoolStatBuilder, IStatBuilder> stat, Func<IPoolStatBuilder, IValueBuilder> value)
            => Add(form, p => stat(PoolStatFrom(p)), value);

        public void Add(
            IFormBuilder form, Func<Pool, IStatBuilder> stat, Func<IPoolStatBuilder, IValueBuilder> value)
            => Add(form, stat, p => value(PoolStatFrom(p)));

        private IPoolStatBuilder PoolStatFrom(Pool pool)
            => _builderFactories.StatBuilders.Pool.From(pool);

        public void Add(IFormBuilder form, Func<Pool, IStatBuilder> stat, Func<Pool, IValueBuilder> value)
        {
            foreach (var pool in Enums.GetValues<Pool>())
            {
                Add(form, stat(pool), value(pool));
            }
        }

        public void Add(IFormBuilder form, Func<DamageType, IStatBuilder> stat, Func<DamageType, IValueBuilder> value)
        {
            foreach (var damageType in Enums.GetValues<DamageType>())
            {
                Add(form, stat(damageType), value(damageType));
            }
        }
    }
}