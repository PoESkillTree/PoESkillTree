using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Localization;

namespace POESKillTree.Views.Computation
{
    public class MockStatViewModel
    {
        public Entity Entity { get; set; } = Entity.Character;
        public string Identity { get; set; }
        public NodeType NodeType { get; set; } = NodeType.Total;
        public string Name => ToString();

        public Type DataType { get; set; }
        public Array EnumValues => DataType.GetEnumValues();

        public double? MinimumValue { get; set; }
        public double? MaximumValue { get; set; }

        public NodeValue? Value
        {
            get
            {
                if (MaximumValue.HasValue && MinimumValue.HasValue)
                    return new NodeValue(MinimumValue.Value, MaximumValue.Value);
                return null;
            }
            set
            {
                MinimumValue = value?.Minimum;
                MaximumValue = value?.Maximum;
            }
        }

        public double? SingleValue
        {
            get => Value?.Single;
            set => Value = (NodeValue?) value;
        }

        public bool BoolValue
        {
            get => Value.IsTrue();
            set => Value = (NodeValue?) value;
        }

        public override string ToString()
        {
            var result = Identity;
            if (Entity != Entity.Character)
            {
                result = $"{Entity} {result}";
            }
            if (NodeType != NodeType.Total)
            {
                result = $"{result} ({NodeType})";
            }
            return result;
        }

        public string StringValue
        {
            get
            {
                if (DataType == typeof(int) || DataType == typeof(double))
                {
                    return Value?.ToString() ?? L10n.Message("None");
                }
                if (DataType == typeof(bool))
                {
                    return BoolValue.ToString();
                }
                if (DataType.IsEnum)
                {
                    if (Value is null)
                        return L10n.Message("None");
                    return EnumValues.GetValue((int) Value.Single()).ToString();
                }
                return null;
            }
        }
    }
}