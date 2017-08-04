using System;
using System.Globalization;
using System.Windows.Data;

namespace POESKillTree.Views.Equipment
{
    /// <summary>
    /// The thumb of a scroll bar is not always centered above the scroll bar value. If the scroll bar value is at
    /// minimum, the upper end of the thumb is above the scroll bar value. If it is at maximum, the lower end is
    /// above the scroll bar value. To get object on the scroll bar (RangeScrollBar) to follow that behavior,
    /// they need to be offset by a percentage of the thumb size (0% at the top, 100% at the bottom).
    /// </summary>
    public class ScrollBarPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var position = System.Convert.ToDouble(values[0]);
            var rows = System.Convert.ToDouble(values[1]);
            var visibleRows = System.Convert.ToDouble(values[2]);
            var offset = (position / rows) * visibleRows;
            return position - offset;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}