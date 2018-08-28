using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders
{
    public delegate IValueBuilder ValueConverter(IValueBuilder value);

    public static class ValueConverterExtensions
    {
        public static ValueConverter AndThen(this ValueConverter @this, ValueConverter next) =>
            v => next(@this(v));
    }
}