using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public abstract class StatBuildersBase
    {
        protected IStatFactory StatFactory { get; }

        protected StatBuildersBase(IStatFactory statFactory) =>
            StatFactory = statFactory;

        protected IFlagStatBuilder FromIdentity(
            Type dataType, ExplicitRegistrationType explicitRegistrationType = null,
            [CallerMemberName] string identity = null) =>
            FromIdentity(identity, dataType, explicitRegistrationType);

        protected IFlagStatBuilder FromIdentity(
            string identity, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null) =>
            StatBuilderUtils.FromIdentity(StatFactory, identity, dataType, explicitRegistrationType);

        protected IFlagStatBuilder FromStatFactory(Func<Entity, IStat> statFactory)
            => new StatBuilder(StatFactory, new LeafCoreStatBuilder(statFactory));

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(
            Type dataType, [CallerMemberName] string identity = null) =>
            DamageRelatedFromIdentity(identity, dataType);

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(string identity, Type dataType) =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, identity, dataType);
    }
}