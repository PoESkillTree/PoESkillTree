using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Builders.Stats
{
    public abstract class StatBuildersBase
    {
        protected IStatFactory StatFactory { get; }

        protected StatBuildersBase(IStatFactory statFactory) =>
            StatFactory = statFactory;

        protected IStatBuilder FromIdentity(
            Type dataType, ExplicitRegistrationType explicitRegistrationType = null,
            [CallerMemberName] string identity = null) =>
            FromIdentity(identity, dataType, explicitRegistrationType);

        protected virtual IStatBuilder FromIdentity(
            string identity, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null) =>
            StatBuilderUtils.FromIdentity(StatFactory, identity, dataType, explicitRegistrationType);

        protected IStatBuilder FromStatFactory(Func<Entity, IStat> statFactory)
            => new StatBuilder(StatFactory, new LeafCoreStatBuilder(statFactory));

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(
            Type dataType, [CallerMemberName] string identity = null) =>
            DamageRelatedFromIdentity(identity, dataType);

        protected virtual IDamageRelatedStatBuilder DamageRelatedFromIdentity(string identity, Type dataType) =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, identity, dataType);
    }

    internal abstract class PrefixedStatBuildersBase : StatBuildersBase
    {
        private readonly string _identityPrefix;

        protected PrefixedStatBuildersBase(IStatFactory statFactory, string identityPrefix) : base(statFactory)
            => _identityPrefix = identityPrefix;

        protected override IStatBuilder FromIdentity(
            string identity, Type dataType, ExplicitRegistrationType explicitRegistrationType = null)
            => base.FromIdentity(_identityPrefix + "." + identity, dataType, explicitRegistrationType);

        protected override IDamageRelatedStatBuilder DamageRelatedFromIdentity(string identity, Type dataType)
            => base.DamageRelatedFromIdentity(_identityPrefix + "." + identity, dataType);
    }
}