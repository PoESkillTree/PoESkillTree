using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IValueProvider
    {

    }

    public delegate ValueProvider ValueFunc(ValueProvider value);

    public class ValueProvider : IValueProvider
    {
        public static implicit operator ValueProvider(int value)
        {
            throw new NotImplementedException();
        }

        public static implicit operator ValueProvider(double value)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator ==(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator !=(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator >=(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator <=(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator >(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider operator <(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static ValueProvider operator *(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static ValueProvider operator /(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static ValueProvider operator -(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public static ValueProvider operator +(ValueProvider left, ValueProvider right)
        {
            throw new NotImplementedException();
        }

        public ValueProvider AsPercentage => this / 100;
        public ValueProvider Invert => 1 / this;

        // to how many digits depends on the number of significant digits the value has
        // they also need to be rounded to more digits before floored/ceiled to avoid e.g. 0.99999 being floored to 0
        public ValueProvider Rounded => throw new NotImplementedException();
        public ValueProvider Floored => throw new NotImplementedException();
        public ValueProvider Ceiled => throw new NotImplementedException();
    }


    public interface IValueProviderCollection : IProviderCollection<ValueProvider>
    {

    }


    public static class ValueProviders
    {
        public static readonly IValueProviderCollection Values;

        public static readonly ValueProvider Value = Values.Single;

        public static readonly ValueFunc TimeToPerSecond = v => v.Invert;

        public static ValueFunc PerStat(IStatProvider stat, ValueProvider divideBy = null) => 
            v => v * (stat.Value / divideBy ?? 1).Floored;

        public static ValueFunc PerStatCeiled(IStatProvider stat, ValueProvider divideBy) => 
            v => v * (stat.Value / divideBy ?? 1).Ceiled;

        public static ValueFunc PerLevel => throw new NotImplementedException();

        public static ValueFunc PercentOf(IStatProvider stat) => 
            v => stat.Value * v.AsPercentage;

        public static ValueFunc LinearScale(IStatProvider yStat, params (int y, double multiplier)[] points) =>
            throw new NotImplementedException();
    }
}