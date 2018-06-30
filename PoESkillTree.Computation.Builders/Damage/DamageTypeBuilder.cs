using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Damage
{
    public class DamageTypeBuilder : IDamageTypeBuilder
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreDamageTypeBuilder _coreDamageType;

        public DamageTypeBuilder(IStatFactory statFactory, DamageType damageType)
            : this(statFactory, new LeafDamageTypeBuilder(damageType))
        {
        }

        private DamageTypeBuilder(IStatFactory statFactory, ICoreDamageTypeBuilder coreDamageType)
        {
            _coreDamageType = coreDamageType;
            _statFactory = statFactory;
        }

        private IDamageTypeBuilder With(ICoreDamageTypeBuilder coreDamageType) =>
            new DamageTypeBuilder(_statFactory, coreDamageType);

        public IKeywordBuilder Resolve(ResolveContext context) =>
            With(_coreDamageType.Resolve(context));

        public Keyword Build()
        {
            var types = _coreDamageType.Build().ToList();
            if (types.IsEmpty())
                throw new ParseException("Can't built zero damage types");
            if (types.Count > 1)
                throw new ParseException("Can't built multiple damage types");
            if (Enum.TryParse(types.Single().ToString(), out Keyword keyword))
                return keyword;
            throw new ParseException($"{_coreDamageType} can't be used as a keyword");
        }

        public IReadOnlyList<DamageType> BuildDamageTypes() => _coreDamageType.Build().ToList();

        public IDamageTypeBuilder And(IDamageTypeBuilder type) =>
            With(new AndDamageTypeBuilder(_coreDamageType, new ProxyDamageTypeBuilder(type)));

        public IDamageTypeBuilder Invert =>
            With(new InvertDamageTypeBuilder(_coreDamageType));

        public IDamageTypeBuilder Except(IDamageTypeBuilder type) =>
            With(new ExceptDamageTypeBuilder(_coreDamageType, new ProxyDamageTypeBuilder(type)));

        public IStatBuilder Resistance =>
            new StatBuilder(_statFactory, CoreStat(typeof(int)));

        public IDamageStatBuilder Damage =>
            new DamageStatBuilder(_statFactory, CoreStat(_statFactory.Damage));

        public IDamageTakenConversionBuilder DamageTakenFrom(IPoolStatBuilder pool)
        {
            var damage = CoreStat(_statFactory.Damage);
            var takenFrom = new ParametrisedCoreStatBuilder<IStatBuilder>(damage, pool,
                (p, s) => _statFactory.CopyWithSuffix(s, $"TakenFrom({((IPoolStatBuilder) p).BuildPool()})",
                    typeof(int)));
            return new DamageTakenConversionBuilder(_statFactory, takenFrom);
        }

        public IDamageRelatedStatBuilder Penetration =>
            DamageRelatedStatBuilder.Create(_statFactory, CoreStat(typeof(int))).WithHits;

        public IFlagStatBuilder IgnoreResistance =>
            new StatBuilder(_statFactory, CoreStat(typeof(bool)));

        public IStatBuilder ReflectedDamageTaken =>
            new StatBuilder(_statFactory, CoreStat(typeof(int)));

        private ICoreStatBuilder CoreStat(Type dataType, [CallerMemberName] string identitySuffix = null) =>
            CoreStat((e, t) => _statFactory.FromIdentity($"{t}.{identitySuffix}", e, dataType));

        private ICoreStatBuilder CoreStat(Func<Entity, DamageType, IStat> statFactory) =>
            new DamageTypeCoreStatBuilder(statFactory, _coreDamageType, new ModifierSourceEntityBuilder());

        private class DamageTakenConversionBuilder : IDamageTakenConversionBuilder
        {
            private readonly IStatFactory _statFactory;
            private readonly ICoreStatBuilder _coreStat;

            public DamageTakenConversionBuilder(IStatFactory statFactory, ICoreStatBuilder coreStat)
            {
                _statFactory = statFactory;
                _coreStat = coreStat;
            }

            public IStatBuilder Before(IPoolStatBuilder pool)
            {
                var coreStat = new ParametrisedCoreStatBuilder<IStatBuilder>(_coreStat, pool,
                    (p, s) => _statFactory.CopyWithSuffix(s, $"Before({((IPoolStatBuilder) p).BuildPool()})",
                        typeof(int)));
                return new StatBuilder(_statFactory, coreStat);
            }
        }
    }
}