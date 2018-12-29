using System;
using System.Globalization;
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

        public double? MinimumValue { get; set; }
        public double? MaximumValue { get; set; }

        public NodeValue? Value => MaximumValue.HasValue && MinimumValue.HasValue
            ? new NodeValue(MinimumValue.Value, MaximumValue.Value)
            : (NodeValue?) null;

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
                    return Value is null ? L10n.Message("None") : Value.ToString();
                }
                if (DataType == typeof(bool))
                {
                    return Value.IsTrue() ? L10n.Message("True") : L10n.Message("False");
                }
                if (DataType.IsEnum)
                {
                    if (Value is null)
                        return L10n.Message("None");
                    var enumValue =
                        (Enum) Enum.Parse(DataType, Value.Single().ToString(CultureInfo.InvariantCulture));
                    return enumValue.ToString();
                }
                return null;
            }
        }
    }
}