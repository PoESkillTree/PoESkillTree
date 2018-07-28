using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Utils;
using PoESkillTree.Common.Utils.Extensions;
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
            Add(form, stat, new[] { valueParameter }, ss => value(ss.Single()));
        }

        public void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat,
            IDamageRelatedStatBuilder vp1, IDamageRelatedStatBuilder vp2,
            Func<IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, new[] { vp1, vp2 }, ss => value(ss[0], ss[1]));
        }

        public void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat,
            IDamageRelatedStatBuilder vp1, IDamageRelatedStatBuilder vp2, IDamageRelatedStatBuilder vp3,
            Func<IStatBuilder, IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, new[] { vp1, vp2, vp3 }, ss => value(ss[0], ss[1], ss[2]));
        }

        private void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat,
            IEnumerable<IDamageRelatedStatBuilder> valueParameters,
            Func<IReadOnlyList<IStatBuilder>, IValueBuilder> value)
        {
            stat = stat.WithSkills;
            valueParameters = valueParameters.Select(s => s.WithHits).ToList();
            Add(form, stat.With(AttackDamageHand.MainHand),
                value(valueParameters.Select(s => s.With(AttackDamageHand.MainHand)).ToList()));
            Add(form, stat.With(AttackDamageHand.OffHand),
                value(valueParameters.Select(s => s.With(AttackDamageHand.OffHand)).ToList()));
            Add(form, stat.With(DamageSource.Spell),
                value(valueParameters.Select(s => s.With(DamageSource.Spell)).ToList()));
            Add(form, stat.With(DamageSource.Secondary),
                value(valueParameters.Select(s => s.With(DamageSource.Secondary)).ToList()));
        }

        public void Add(
            IFormBuilder form, IDamageRelatedStatBuilder stat, Func<DamageType, IDamageRelatedStatBuilder> value)
        {
            Add(form, _ => stat, value);
        }

        public void Add(
            IFormBuilder form,
            Func<DamageType, IDamageRelatedStatBuilder> stat,
            Func<DamageType, IDamageRelatedStatBuilder> value)
        {
            stat = stat.AndThen(s => s.WithHits);
            value = value.AndThen(v => v.WithHits);
            Add(form, dt => stat(dt).With(AttackDamageHand.MainHand),
                dt => value(dt).With(AttackDamageHand.MainHand).Value);
            Add(form, dt => stat(dt).With(AttackDamageHand.OffHand),
                dt => value(dt).With(AttackDamageHand.OffHand).Value);
            Add(form, dt => stat(dt).With(DamageSource.Spell), dt => value(dt).With(DamageSource.Spell).Value);
            Add(form, dt => stat(dt).With(DamageSource.Secondary), dt => value(dt).With(DamageSource.Secondary).Value);
        }

        public void Add(
            IFormBuilder form, Func<DamageType, IDamageRelatedStatBuilder> stat,
            Func<DamageType, IDamageRelatedStatBuilder> vp1, Func<DamageType, IDamageRelatedStatBuilder> vp2,
            Func<DamageType, IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, dt => new[] { vp1(dt), vp2(dt) }, (dt, ss) => value(dt, ss[0], ss[1]));
        }

        public void Add(
            IFormBuilder form, Func<DamageType, IDamageRelatedStatBuilder> stat,
            Func<DamageType, IDamageRelatedStatBuilder> vp1, Func<DamageType, IDamageRelatedStatBuilder> vp2,
            Func<DamageType, IDamageRelatedStatBuilder> vp3,
            Func<DamageType, IStatBuilder, IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, dt => new[] { vp1(dt), vp2(dt), vp3(dt) }, (dt, ss) => value(dt, ss[0], ss[1], ss[2]));
        }

        private void Add(
            IFormBuilder form,
            Func<DamageType, IDamageRelatedStatBuilder> stat,
            Func<DamageType, IEnumerable<IDamageRelatedStatBuilder>> valueParameters,
            Func<DamageType, IReadOnlyList<IStatBuilder>, IValueBuilder> value)
        {
            stat = stat.AndThen(s => s.WithHits);
            valueParameters = valueParameters.AndThen(ss => ss.Select(s => s.WithHits));
            Add(form, dt => stat(dt).With(AttackDamageHand.MainHand),
                dt => value(dt, valueParameters(dt).Select(s => s.With(AttackDamageHand.MainHand)).ToList()));
            Add(form, dt => stat(dt).With(AttackDamageHand.OffHand),
                dt => value(dt, valueParameters(dt).Select(s => s.With(AttackDamageHand.OffHand)).ToList()));
            Add(form, dt => stat(dt).With(DamageSource.Spell),
                dt => value(dt, valueParameters(dt).Select(s => s.With(DamageSource.Spell)).ToList()));
            Add(form, dt => stat(dt).With(DamageSource.Secondary),
                dt => value(dt, valueParameters(dt).Select(s => s.With(DamageSource.Secondary)).ToList()));
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
            foreach (var damageType in Enums.GetValues<DamageType>().Except(DamageType.RandomElement))
            {
                Add(form, stat(damageType), value(damageType));
            }
        }
    }
}