using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders
{
    internal static class BuilderExtensions
    {
        public static IValue Build(this IValueBuilder @this) => @this.Build(default);

        public static ConditionBuilderResult Build(this IConditionBuilder @this) =>
            @this.Build(default);
    }
}