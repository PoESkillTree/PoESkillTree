using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils.Extensions;

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
            IReadOnlyList<IDamageRelatedStatBuilder> valueParameters,
            Func<IReadOnlyList<IStatBuilder>, IValueBuilder> value)
        {
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
            IFormBuilder form, Func<Ailment, IDamageRelatedStatBuilder> stat,
            Func<Ailment, IDamageRelatedStatBuilder> vp,
            Func<Ailment, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, a => new[] { vp(a) }, (a, ss) => value(a, ss.Single()));
        }

        public void Add(
            IFormBuilder form, Func<Ailment, IDamageRelatedStatBuilder> stat,
            Func<Ailment, IDamageRelatedStatBuilder> vp1, Func<Ailment, IDamageRelatedStatBuilder> vp2,
            Func<Ailment, IDamageRelatedStatBuilder> vp3,
            Func<Ailment, IStatBuilder, IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, a => new[] { vp1(a), vp2(a), vp3(a) }, (a, ss) => value(a, ss[0], ss[1], ss[2]));
        }

        public void Add(
            IFormBuilder form, Func<Ailment, IDamageRelatedStatBuilder> stat,
            Func<Ailment, IDamageRelatedStatBuilder> vp1, Func<Ailment, IDamageRelatedStatBuilder> vp2,
            Func<Ailment, IDamageRelatedStatBuilder> vp3, Func<Ailment, IDamageRelatedStatBuilder> vp4,
            Func<Ailment, IDamageRelatedStatBuilder> vp5,
            Func<IStatBuilder, IStatBuilder, IStatBuilder, IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, a => new[] { vp1(a), vp2(a), vp3(a), vp4(a), vp5(a) },
                (a, ss) => value(ss[0], ss[1], ss[2], ss[3], ss[4]));
        }

        public void Add(
            IFormBuilder form, Func<DamageType, IDamageRelatedStatBuilder> stat,
            Func<DamageType, IDamageRelatedStatBuilder> vp,
            Func<DamageType, IStatBuilder, IValueBuilder> value)
        {
            Add(form, stat, a => new[] { vp(a) }, (a, ss) => value(a, ss.Single()));
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

        public void Add(
            IFormBuilder form, Func<Ailment, DamageType, IDamageRelatedStatBuilder> stat,
            Func<Ailment, DamageType, IDamageRelatedStatBuilder> vp1,
            Func<Ailment, DamageType, IDamageRelatedStatBuilder> vp2,
            Func<IStatBuilder, IStatBuilder, IValueBuilder> value)
        {
            foreach (var ailment in Enums.GetValues<Ailment>())
            {
                Add<DamageType>(form, dt => stat(ailment, dt), dt => new[] { vp1(ailment, dt), vp2(ailment, dt) },
                    (dt, ss) => value(ss[0], ss[1]));
            }
        }

        private void Add<TEnum>(
            IFormBuilder form,
            Func<TEnum, IDamageRelatedStatBuilder> stat,
            Func<TEnum, IEnumerable<IDamageRelatedStatBuilder>> valueParameters,
            Func<TEnum, IReadOnlyList<IStatBuilder>, IValueBuilder> value)
            where TEnum : struct, Enum
        {
            Add<TEnum>(form, dt => stat(dt).With(AttackDamageHand.MainHand),
                dt => value(dt, valueParameters(dt).Select(s => s.With(AttackDamageHand.MainHand)).ToList()));
            Add<TEnum>(form, dt => stat(dt).With(AttackDamageHand.OffHand),
                dt => value(dt, valueParameters(dt).Select(s => s.With(AttackDamageHand.OffHand)).ToList()));
            Add<TEnum>(form, dt => stat(dt).With(DamageSource.Spell),
                dt => value(dt, valueParameters(dt).Select(s => s.With(DamageSource.Spell)).ToList()));
            Add<TEnum>(form, dt => stat(dt).With(DamageSource.Secondary),
                dt => value(dt, valueParameters(dt).Select(s => s.With(DamageSource.Secondary)).ToList()));
        }

        public void Add(
            IFormBuilder form, Func<Pool, IStatBuilder> stat, Func<IPoolStatBuilder, IValueBuilder> value)
            => Add(form, stat, p => value(PoolStatFrom(p)));

        private IPoolStatBuilder PoolStatFrom(Pool pool)
            => _builderFactories.StatBuilders.Pool.From(pool);

        public void Add(IFormBuilder form, Func<Pool, IStatBuilder> stat, Func<Pool, IValueBuilder> value)
            => Add<Pool>(form, stat, value);

        public void Add(IFormBuilder form, Func<Ailment, IStatBuilder> stat, Func<Ailment, IValueBuilder> value)
            => Add<Ailment>(form, stat, value);

        public void Add(IFormBuilder form, Func<DamageType, IStatBuilder> stat, Func<DamageType, IValueBuilder> value)
            => Add<DamageType>(form, stat, value);

        private void Add<TEnum>(IFormBuilder form, Func<TEnum, IStatBuilder> stat, Func<TEnum, IValueBuilder> value)
            where TEnum : struct, Enum
        {
            var ts = Enums.GetValues<TEnum>();
            if (typeof(TEnum) == typeof(DamageType))
            {
                ts = ts.Except((TEnum) (object) DamageType.RandomElement);
            }
            foreach (var t in ts)
            {
                Add(form, stat(t), value(t));
            }
        }
    }
}