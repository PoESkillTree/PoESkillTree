using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public static class StatBuilderUtils
    {
        public static IConditionBuilder ConditionFromIdentity(
            IStatFactory statFactory, string identity, bool isExplicitlyRegistered = false) =>
            FromIdentity(statFactory, identity, typeof(bool), isExplicitlyRegistered).IsSet;

        public static IFlagStatBuilder FromIdentity(
            IStatFactory statFactory, string identity, Type dataType, bool isExplicitlyRegistered = false) =>
            new StatBuilder(statFactory,
                LeafCoreStatBuilder.FromIdentity(statFactory, identity, dataType, isExplicitlyRegistered));

        public static IDamageRelatedStatBuilder DamageRelatedFromIdentity(
            IStatFactory statFactory, string identity, Type dataType) =>
            new DamageRelatedStatBuilder(statFactory,
                LeafCoreStatBuilder.FromIdentity(statFactory, identity, dataType));
    }
}