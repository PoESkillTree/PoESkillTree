using System;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class DataDrivenMechanicCollection : GivenStatCollection
    {
        public DataDrivenMechanicCollection(IModifierBuilder modifierBuilder, IValueBuilders valueFactory)
            : base(modifierBuilder, valueFactory)
        {
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
    }
}