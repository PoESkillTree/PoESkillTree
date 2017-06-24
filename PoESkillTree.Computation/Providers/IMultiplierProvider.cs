using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IMultiplierProvider
    {

    }

    public static class MultiplierProviders
    {
        public static IMultiplierProvider PerStat(IStatProvider stat, IValueProvider divideBy = null)
        {
            throw new NotImplementedException();
        }
    }
}