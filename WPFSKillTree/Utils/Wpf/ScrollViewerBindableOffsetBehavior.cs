using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace PoESkillTree.Utils.Wpf
{
    /// <summary>
    /// Allows two way binding to the VerticalOffset property of ScrollViewer.
    /// Source to ScrollViewer: ScrollViewer.ScrollToVerticalOffset,
    /// ScrollViewer to Source: ScrollViewer.PART_VerticalScrollBar.Value
    /// </summary>
    public class ScrollViewerBindableOffsetBehavior : Behavior<ScrollViewer>
    {
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof(double), typeof(ScrollViewerBindableOffsetBehavior),
            new PropertyMetadata(default(double), VerticalOffsetOnPropertyChanged));

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        private static void VerticalOffsetOnPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var behavior = (ScrollViewerBindableOffsetBehavior) dependencyObject;
            var offset = (double) args.NewValue;
            behavior.AssociatedObject.ScrollToVerticalOffset(offset);
        }

        private ScrollBar _verticalScrollBar;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += (sender, args) =>
            {
                // not pretty but works
                AssociatedObject.ApplyTemplate();
                _verticalScrollBar = AssociatedObject.Template.FindName("PART_VerticalScrollBar", AssociatedObject) as ScrollBar;
                if (_verticalScrollBar != null)
                {
                    _verticalScrollBar.ValueChanged += (o, eventArgs) =>
                    {
                        VerticalOffset = _verticalScrollBar.Value;
                    };
                }
            };
            base.OnAttached();
        }
    }
}