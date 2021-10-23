using System;
using System.Globalization;
using System.Windows.Data;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class MasteryEffectEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo _)
        {
            if (values[0] is ushort effect && values[1] is MasteryEffectSelectionViewModel vm)
            {
                return vm.IsEffectEnabled(effect);
            }

            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
