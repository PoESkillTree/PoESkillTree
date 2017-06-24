using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IValueProvider
    {

    }

    public static class ValueProviders
    {
        public static IValueProvider Value(int index)
        {
            throw new NotImplementedException();
        }

        public static IValueProvider FixedValue(int value)
        {
            throw new NotImplementedException();
        }
    }
}