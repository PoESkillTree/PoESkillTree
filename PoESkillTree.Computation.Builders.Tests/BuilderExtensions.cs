using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Tests
{
    internal static class BuilderExtensions
    {
        public static IValue Build(this IValueBuilder @this) => @this.Build(default);

        public static (StatConverter statConverter, IValue value) Build(this IConditionBuilder @this) =>
            @this.Build(default);
    }
}