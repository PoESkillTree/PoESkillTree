using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public abstract class StatBuildersBase
    {
        protected IStatFactory StatFactory { get; }

        protected StatBuildersBase(IStatFactory statFactory) =>
            StatFactory = statFactory;

        protected IFlagStatBuilder FromIdentity(
            Type dataType, bool isExplicitlyRegistered = false, [CallerMemberName] string identity = null) =>
            FromIdentity(identity, dataType, isExplicitlyRegistered);

        protected IFlagStatBuilder FromIdentity(string identity, Type dataType, bool isExplicitlyRegistered = false) =>
            StatBuilderUtils.FromIdentity(StatFactory, identity, dataType, isExplicitlyRegistered);

        protected IFlagStatBuilder FromCore(ICoreStatBuilder coreStatBuilder) =>
            new StatBuilder(StatFactory, coreStatBuilder);
    }
}