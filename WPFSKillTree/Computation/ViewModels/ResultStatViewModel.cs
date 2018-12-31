using System;
using System.Windows.Input;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Common.ViewModels;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel : Notifier
    {
        private NodeValue? _value;

        public ResultStatViewModel(IStat stat, NodeType nodeType, Action<ResultStatViewModel> removeAction)
        {
            (Stat, NodeType) = (stat, nodeType);
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public IStat Stat { get; }
        public string Name => ToString();

        public NodeType NodeType { get; }

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

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

        public string StringValue
        {
            get
            {
                var dataType = Stat.DataType;
                if (dataType == typeof(int) || dataType == typeof(double))
                {
                    return Value?.ToString() ?? L10n.Message("None");
                }
                if (dataType == typeof(bool))
                {
                    return Value.IsTrue().ToString();
                }
                if (dataType.IsEnum)
                {
                    if (Value is null)
                        return L10n.Message("None");
                    return dataType.GetEnumValues().GetValue((int) Value.Single()).ToString();
                }
                return null;
            }
        }

        public ICommand RemoveCommand { get; }
    }
}