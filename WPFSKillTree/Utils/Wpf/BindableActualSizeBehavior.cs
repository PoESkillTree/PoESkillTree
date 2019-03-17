using System.Windows;
using System.Windows.Interactivity;

namespace PoESkillTree.Utils.Wpf
{
    /// <summary>
    /// Allows (OneWayToSource) binding to the ActualHeight property of FrameworkElements.
    /// </summary>
    public class BindableActualSizeBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ActualHeightProperty = DependencyProperty.Register(
            "ActualHeight", typeof(double), typeof(BindableActualSizeBehavior), new PropertyMetadata(default(double)));

        public double ActualHeight
        {
            get { return (double) GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += (sender, args) =>
            {
                ActualHeight = AssociatedObject.ActualHeight;
            };
            base.OnAttached();
        }
    }
}