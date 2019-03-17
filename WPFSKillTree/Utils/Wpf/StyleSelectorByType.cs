using System;
using System.Windows;
using System.Windows.Controls;

namespace PoESkillTree.Utils.Wpf
{
    /// <summary>
    /// <see cref="StyleSelector"/> that selects by <see cref="Type"/>.
    /// </summary>
    public class StyleSelectorByType : StyleSelector
    {
        /// <summary>
        /// <see cref="Type"/> used to select between <see cref="Style"/> and <see cref="DefaultStyle"/>.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// <see cref="Style"/> used if the item is an instance of <see cref="Type"/>.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// <see cref="Style"/> used if the item is not an instance of <see cref="Type"/>.
        /// </summary>
        public Style DefaultStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return Type.IsInstanceOfType(item) ? Style : DefaultStyle;
        }
    }
}