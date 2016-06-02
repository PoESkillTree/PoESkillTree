using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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



        public static RangeAlignment GetAlignment(DependencyObject obj)
        {
            return (RangeAlignment)obj.GetValue(AlignmentProperty);
        }

        public static void SetAlignment(DependencyObject obj, RangeAlignment value)
        {
            obj.SetValue(AlignmentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Alignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlignmentProperty =
            DependencyProperty.RegisterAttached(
                "Alignment",
                typeof(RangeAlignment),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(
                    RangeAlignment.Center,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure)
                , new ValidateValueCallback(IsValidRangeAlignment));

        private static bool IsValidRangeAlignment(object value)
        {
            var val = (RangeAlignment)value;
            return (val == RangeAlignment.Begin || val == RangeAlignment.Center || val == RangeAlignment.End);
        }

        public static double GetRange(DependencyObject obj)
        {
            return (double)obj.GetValue(RangeProperty);
        }

        public static void SetRange(DependencyObject obj, double value)
        {
            obj.SetValue(RangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for Range.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RangeProperty =
            DependencyProperty.RegisterAttached(
                "Range",
                typeof(double),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));



        protected override bool HasLogicalOrientation
        {
            get
            {
                return true;
            }
        }

        protected override Orientation LogicalOrientation
        {
            get
            {
                return this.Orientation;
            }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
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
                        new PropertyChangedCallback(OnOrientationChanged)),
                new ValidateValueCallback(IsValidOrientation));

        private static bool IsValidOrientation(object o)
        {
            Orientation value = (Orientation)o;
            return value == Orientation.Horizontal || value == Orientation.Vertical;
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var el = (d as UIElement);
            if (el == null)
                return;
            el.InvalidateMeasure();
        }

        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;
            return !(double.IsNaN(d) || double.IsInfinity(d));
        }

        #region minimum property
        public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
                "Minimum",
                typeof(double),
                typeof(RangePanel),
                new FrameworkPropertyMetadata(
                        0.0d,
                        FrameworkPropertyMetadataOptions.AffectsMeasure,
                        new PropertyChangedCallback(OnMinimumChanged),
                        new CoerceValueCallback(CoerceMinimum)),
                new ValidateValueCallback(IsValidDoubleValue));


        private static object CoerceMinimum(DependencyObject d, object value)
        {
            RangePanel pan = (RangePanel)d;
            double max = pan.Maximum;
            if ((double)value > max)
            {
                return max;
            }
            return value;
        }


        /// <summary>
        ///     Minimum restricts the minimum value of range
        /// </summary>
        [Bindable(true), Category("Behavior")]
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        ///     Called when MinimumProperty is changed on "d."
        /// </summary>
        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangePanel pan = (RangePanel)d;
            pan.CoerceValue(MaximumProperty);
            pan.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        ///     This method is invoked when the Minimum property changes.
        /// </summary>
        /// <param name="oldMinimum">The old value of the Minimum property.</param>
        /// <param name="newMinimum">The new value of the Minimum property.</param>
        protected virtual void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
        }

        #endregion

        #region Maximum property
        public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
        "Maximum",
        typeof(double),
        typeof(RangePanel),
        new FrameworkPropertyMetadata(
                100.0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(OnMaximumChanged),
                        new CoerceValueCallback(CoerceMaximum)),
                new ValidateValueCallback(IsValidDoubleValue));


        private static object CoerceMaximum(DependencyObject d, object value)
        {
            RangePanel pan = (RangePanel)d;
            double min = pan.Minimum;
            if ((double)value < min)
            {
                return min;
            }
            return value;
        }
        /// <summary>
        ///     Minimum restricts the minimum value of range
        /// </summary>
        [Bindable(true), Category("Behavior")]
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        ///     Called when MinimumProperty is changed on "d."
        /// </summary>
        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangePanel pan = (RangePanel)d;
            //  pan.CoerceValue(MinimumProperty);
            pan.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
        }


        /// <summary>
        ///     This method is invoked when the Maximum property changes.
        /// </summary>
        /// <param name="oldMaximum">The old value of the Maximum property.</param>
        /// <param name="newMaximum">The new value of the Maximum property.</param>
        protected virtual void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
        }

        #endregion

        public RangePanel()
        {
            ClipToBounds = true;
        }


        /// <summary>
        /// this should do same thing as canvas arrange
        /// </summary>
        /// <param name="arrangeSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {

            double w = 0;
            double h = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(constraint);
                var s = GetItemPosition(constraint,child);

                if (s.Right > w)
                    w = s.Right;

                if (s.Bottom > h)
                    h = s.Bottom;
            }

            if (!double.IsNaN(this.Width))
                w = this.Width;


            if (!double.IsNaN(this.Height))
                h = this.Height;

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
            foreach (UIElement item in InternalChildren)
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

                var elm = VisualTreeHelper.GetChild(item, 0) as UIElement;
                if (elm != null)
                    item = elm;
            }


            var pos = GetPosition(item);
            var range = GetRange(item);
            var align = GetAlignment(item);

            double x = 0;
            double y = 0;
            double w = item.DesiredSize.Width;
            double h = item.DesiredSize.Height;

            if (!double.IsNaN(range))
            {
                if ((Orientation == Orientation.Horizontal))
                    w = ScaleToSize(range, arrangeSize.Width);
                else
                    h = ScaleToSize(range, arrangeSize.Height);
            }


            Size size;

            if (Orientation == Orientation.Horizontal)
            {
                x = ScaleToSize(pos, arrangeSize.Width);
                size = new Size(w, arrangeSize.Height);
                x -= SizeAdjustment(align, w);
            }
            else
            {
                y = ScaleToSize(pos, arrangeSize.Height);
                size = new Size(arrangeSize.Width, h);
                y -= SizeAdjustment(align, h);
            }

            return new Rect(new Point(x, y), size);
        }

        private static double SizeAdjustment(RangeAlignment align, double size)
        {
            switch (align) //how to place item beside given pos
            {
                case RangeAlignment.Center:
                    return size / 2;
                case RangeAlignment.End:
                    return size;
            }

            return 0;
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }
    }

}
