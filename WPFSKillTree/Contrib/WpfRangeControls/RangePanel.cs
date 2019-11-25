using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfRangeControls
{
    public class RangePanel : Panel
    {
        static RangePanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(RangePanel), new FrameworkPropertyMetadata(true));

            HorizontalAlignmentProperty.OverrideMetadata(typeof(RangePanel), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

            VerticalAlignmentProperty.OverrideMetadata(typeof(RangePanel), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
        }

        public static double GetPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject obj, double value)
        {
            obj.SetValue(PositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached(
                "Position",
                typeof(double),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(
                    (double)0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure)
        );

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => Orientation;

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(
                        Orientation.Vertical,
                        FrameworkPropertyMetadataOptions.AffectsMeasure,
                        OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement el))
                return;
            el.InvalidateMeasure();
        }

        #region minimum property

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(double),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.AffectsMeasure));


        /// <summary>
        ///     Minimum restricts the minimum value of range
        /// </summary>
        [Bindable(true), Category("Behavior")]
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        #endregion

        #region Maximum property

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(double),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(100.0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Minimum restricts the minimum value of range
        /// </summary>
        [Bindable(true), Category("Behavior")]
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        #endregion

        public RangePanel()
        {
            ClipToBounds = true;
        }

        protected override Size MeasureOverride(Size constraint)
        {

            double w = 0;
            double h = 0;

            foreach (var child in InternalChildren.OfType<UIElement>())
            {
                child.Measure(constraint);
                var s = GetItemPosition(constraint, child);

                if (s.Right > w)
                    w = s.Right;

                if (s.Bottom > h)
                    h = s.Bottom;
            }

            if (!double.IsNaN(Width))
                w = Width;


            if (!double.IsNaN(Height))
                h = Height;

            if (HorizontalAlignment == HorizontalAlignment.Stretch)
                w = 0;

            if (VerticalAlignment == VerticalAlignment.Stretch)
                h = 0;

            return new Size(w, h);
        }


        private double ScaleToSize(double val, double size)
        {
            var len = Maximum - Minimum;
            return val / len * size;
        }

        /// <summary>
        /// this should do same thing as canvas arrange
        /// </summary>
        /// <param name="arrangeSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (var item in InternalChildren.OfType<UIElement>())
            {
                Rect r = GetItemPosition(arrangeSize, item);
                item.Arrange(r);
            }
            return arrangeSize;
        }

        private Rect GetItemPosition(Size arrangeSize, UIElement item)
        {
            if (item is ContentPresenter && item.ReadLocalValue(PositionProperty) == DependencyProperty.UnsetValue)
            { //should this be recursive until uielement with set Position is found?

                if (VisualTreeHelper.GetChild(item, 0) is UIElement elm)
                    item = elm;
            }

            var pos = GetPosition(item);

            double x = 0;
            double y = 0;
            double w = item.DesiredSize.Width;
            double h = item.DesiredSize.Height;

            Size size;

            if (Orientation == Orientation.Horizontal)
            {
                x = ScaleToSize(pos, arrangeSize.Width);
                size = new Size(w, arrangeSize.Height);
                x -= SizeAdjustment(w);
            }
            else
            {
                y = ScaleToSize(pos, arrangeSize.Height);
                size = new Size(arrangeSize.Width, h);
                y -= SizeAdjustment(h);
            }

            return new Rect(new Point(x, y), size);
        }

        private static double SizeAdjustment(double size) => size / 2;
    }

}
