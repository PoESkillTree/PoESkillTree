using System;
using System.Runtime.CompilerServices;
using EnumsNET;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ChargeTypeBuilder : IChargeTypeBuilder
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreBuilder<ChargeType> _chargeType;

        public ChargeTypeBuilder(IStatFactory statFactory, ICoreBuilder<ChargeType> chargeType)
        {
            _statFactory = statFactory;
            _chargeType = chargeType;
        }

        public IChargeTypeBuilder Resolve(ResolveContext context) => this;

        public IStatBuilder Amount => new StatBuilder(_statFactory, CoreStat(typeof(uint)));
        public IStatBuilder Duration => new StatBuilder(_statFactory, CoreStat(typeof(double)));

        public IDamageRelatedStatBuilder ChanceToGain =>
            DamageRelatedStatBuilder.Create(_statFactory, CoreStat(typeof(uint))).WithHits;

        private ICoreStatBuilder CoreStat(Type dataType, [CallerMemberName] string identitySuffix = null) =>
            CoreStat((e, t) => _statFactory.FromIdentity(t.GetName() + "." + identitySuffix, e, dataType));

        private ICoreStatBuilder CoreStat(Func<Entity, ChargeType, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<ChargeType>(_chargeType, statFactory);

        public IActionBuilder GainAction =>
            new ActionBuilder(_statFactory,
                CoreBuilder.UnaryOperation(_chargeType, t => t + ".GainAction"), new ModifierSourceEntityBuilder());

        public ChargeType Build(BuildParameters parameters) => _chargeType.Build(parameters);
    }
}