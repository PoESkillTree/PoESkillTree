using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace POESKillTree.ViewModels
{
    class AttributeToTextblockConverter:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var attr = value as Attribute;
            if (attr == null)
                throw new NotImplementedException();

            var tb = new TextBlock(){TextWrapping= TextWrapping.Wrap};
            var txt = new Run(attr.Text);
            if (attr.Missing)
                txt.Foreground = Brushes.Red;

            tb.Inlines.Add(txt);

            foreach (var i in attr.Deltas)
            {
                if (i != 0)
                {
                    tb.Inlines.Add(" ");
                    txt = new Run(i.ToString("+#;-#;0"));
                    txt.Foreground = (i < 0) ? Brushes.Red : Brushes.Green;
                    tb.Inlines.Add(txt);
                }
            }

            return tb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
