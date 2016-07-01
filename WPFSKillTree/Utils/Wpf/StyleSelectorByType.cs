using System;
using System.Windows;
using System.Windows.Controls;

namespace POESKillTree.Utils.Wpf
{
    public class StyleSelectorByType : StyleSelector
    {
        public Type Type { get; set; }

        public Style Style { get; set; }

        public Style DefaultStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return Type.IsInstanceOfType(item) ? Style : DefaultStyle;
        }
    }
}