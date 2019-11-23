using System;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Localization;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class CalculationNodeViewModel : Notifier
    {
        private NodeValue? _value;
        private string? _stringValue;

        public CalculationNodeViewModel(IStat stat, NodeType nodeType = NodeType.Total)
            => (Stat, NodeType) = (stat, nodeType);

        public IStat Stat { get; }
        public NodeType NodeType { get; }
        public Type DataType => Stat.DataType;
        public Array EnumValues => DataType.GetEnumValues();

        public override string ToString()
        {
            var result = Stat.Identity;
            if (Stat.Entity != Entity.Character)
            {
                result = $"{Stat.Entity} {result}";
            }
            if (NodeType != NodeType.Total)
            {
                result = $"{result} ({NodeType})";
            }
            return result;
        }

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, ValueOnPropertyChanged);
        }

        private void ValueOnPropertyChanged()
        {
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(BoolValue));
            OnPropertyChanged(nameof(HasValue));
            StringValue = CalculateStringValue();
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

        public bool HasValue => Value.HasValue;

        public string StringValue
        {
            get => _stringValue ??= CalculateStringValue();
            private set => SetProperty(ref _stringValue, value);
        }

        private string CalculateStringValue()
        {
            if (DataType == typeof(bool))
                return Value.IsTrue().ToString();
            if (Value is null)
                return L10n.Message("None");
            if (DataType.IsEnum)
                return EnumValues.GetValue((int) Value.Single())!.ToString()!;
            return Value.Select(d => Math.Round(d, 2, MidpointRounding.AwayFromZero))
                .ToString()!.Replace(" to ", " \nto ");
        }
    }
}