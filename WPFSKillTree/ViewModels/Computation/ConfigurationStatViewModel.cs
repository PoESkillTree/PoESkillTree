using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Computation
{
    public class ConfigurationStatViewModel : Notifier
    {
        private NodeValue? _value;

        public ConfigurationStatViewModel(IStat stat)
            => Stat = stat;

        public IStat Stat { get; }
        public string Name => ToString();
        public Array EnumValues => Stat.DataType.GetEnumValues();

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
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
            var result = Stat.Identity;
            if (Stat.Entity != Entity.Character)
            {
                result = $"{Stat.Entity} {result}";
            }
            return result;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Value))
            {
                base.OnPropertyChanged(nameof(SingleValue));
                base.OnPropertyChanged(nameof(BoolValue));
            }
        }
    }
}