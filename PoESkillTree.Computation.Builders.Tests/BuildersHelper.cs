using Moq;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders
{
    public static class BuildersHelper
    {
        public static ResolveContext MockResolveContext() =>
            new ResolveContext(Mock.Of<IMatchContext<IValueBuilder>>(), Mock.Of<IMatchContext<IReferenceConverter>>());
    }
}