using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public abstract class CalculationNodeViewModel : Notifier
    {
        private NodeValue? _value;

        protected CalculationNodeViewModel(IStat stat, NodeType nodeType = NodeType.Total)
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
    }
}