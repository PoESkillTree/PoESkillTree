using System.Windows;
using System.Windows.Interactivity;

namespace PoESkillTree.Utils.Wpf
{
    /// <summary>
    /// Sets the property value bound to <see cref="Property"/> to <see cref="Value"/>.
    /// </summary>
    public class Setter : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(Setter));

        /// <summary>
        /// The value to set the property to.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(
            "Property", typeof(object), typeof(Setter),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// This property is set to <see cref="Value"/>. Bind the property you want to change
        /// to this.
        /// </summary>
        public object Property
        {
            get { return GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            Property = Value;
        }
    }
}