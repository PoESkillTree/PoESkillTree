using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatViewModel : Notifier
    {
        private NodeValue? _value;
        private NodeValue? _minimum;
        private NodeValue? _maximum;

        public ConfigurationStatViewModel(IStat stat)
            => Stat = stat;

        public IStat Stat { get; }
        public string Name => ToString();
        public Array EnumValues => Stat.DataType.GetEnumValues();

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, ValueOnPropertyChanged);
        }

        private void ValueOnPropertyChanged()
        {
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(BoolValue));
        }

        public double? NumericValue
        {
            get => Value?.Single;
            set => Value = (NodeValue?) value;
        }

        public bool BoolValue
        {
            get => Value.IsTrue();
            set => Value = (NodeValue?) value;
        }

        public NodeValue? Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value, () => OnPropertyChanged(nameof(NumericMinimum)));
        }

        public double NumericMinimum => Minimum?.Single ?? double.MinValue;

        public NodeValue? Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value, () => OnPropertyChanged(nameof(NumericMaximum)));
        }

        public double NumericMaximum => Maximum?.Single ?? double.MaxValue;

        public override string ToString()
        {
            var result = Stat.Identity;
            if (Stat.Entity != Entity.Character)
            {
                result = $"{Stat.Entity} {result}";
            }
            return result;
        }
    }
}