using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PoESkillTree.Controls
{
    public class ZoomBorder : Border
    {
        private enum ManipulationState
        {
            None, MouseDrag, MouseZoom, Touch
        };

        private Point _originalTranslation;
        private Point _mouseDragAbsoluteStart;
        private Point _mouseZoomRelativeStart;
        private double _lastMouseAbsoluteY;
        private bool _thresholdExceeded;
        private ManipulationState _manipulationState = ManipulationState.None;

        private const double ZoomStep = 0.3;

        private const double MaxZoom = 75;
        private const double MinZoom = 0.5;

        private const double DragThreshold = 5; // (in ZoomBorder coordinates, not Child)
        private const double DragThresholdSquared = DragThreshold * DragThreshold;

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (value != null && !Equals(value, Child))
                {
                    Initialize(value);
                }

                base.Child = value;
            }
        }

        private static ScaleTransform GetScaleTransform(UIElement element)
        {
            return
                (ScaleTransform) ((TransformGroup) element.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }

        private static TranslateTransform GetTranslateTransform(UIElement element)
        {
            return
                (TranslateTransform)
                    ((TransformGroup) element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        private void Initialize(UIElement element)
        {
            var group = new TransformGroup();

            var st = new ScaleTransform();
            group.Children.Add(st);

            var tt = new TranslateTransform();

            group.Children.Add(tt);

            element.RenderTransform = group;
            element.RenderTransformOrigin = new Point(0.0, 0.0);

            element.MouseWheel += Child_MouseWheel;
            element.MouseLeftButtonDown += Child_MouseLeftButtonDown;
            element.MouseLeftButtonUp += Child_MouseEitherButtonUp;
            element.MouseRightButtonDown += Child_MouseRightButtonDown;
            element.MouseRightButtonUp += Child_MouseEitherButtonUp;
            element.MouseMove += Child_MouseMove;

            element.IsManipulationEnabled = true;
            element.ManipulationStarting += Child_ManipulationStarting;
            element.ManipulationDelta += Child_ManipulationDelta;
            element.ManipulationCompleted += Child_ManipulationCompleted;
        }

        public void Reset()
        {
            if (Child != null)
            {
                // reset zoom
                ScaleTransform st = GetScaleTransform(Child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                TranslateTransform tt = GetTranslateTransform(Child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        public static readonly RoutedEvent ClickEvent;


        static ZoomBorder()
        {
            ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof (ZoomBorder));
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (IsMouseOver)
                RaiseEvent(new RoutedEventArgs(ClickEvent, e));
        }

        public void ZoomIn(dynamic e)
        {
            if (Child != null)
                ZoomOnPoint(e.GetPosition(Child), 1.0 + ZoomStep);
        }

        public void ZoomOut(dynamic e)
        {
            if (Child != null)
                ZoomOnPoint(e.GetPosition(Child), 1.0 - ZoomStep);
        }

        /// <summary>
        /// Changes the ScaleTransform and TranslateTransform of the child to achieve a zoom
        /// centered on the given point.
        /// </summary>
        /// <param name="relativeCenter">The zoom center relative to the child.</param>
        /// <param name="zoomFactor">Factor (around 1.0) by how much to zoom in or out.</param>
        private void ZoomOnPoint(Point relativeCenter, double zoomFactor)
        {
            if (Child == null)
                return;

            if (_manipulationState == ManipulationState.MouseDrag)
                return;

            TranslateTransform tt = GetTranslateTransform(Child);
            ScaleTransform st = GetScaleTransform(Child);

            if (st.ScaleX * zoomFactor < MinZoom || st.ScaleY * zoomFactor < MinZoom)
                return;
            if (st.ScaleX * zoomFactor > MaxZoom || st.ScaleY * zoomFactor > MaxZoom)
                return;

            double absoluteX = relativeCenter.X * st.ScaleX + tt.X;
            double absoluteY = relativeCenter.Y * st.ScaleY + tt.Y;

            st.ScaleX *= zoomFactor;
            st.ScaleY *= zoomFactor;

            // Correct the translate transform so that the given point of the child
            // remains at the same absolute position (i.e. with respect to the zoom border).
            tt.X = absoluteX - relativeCenter.X * st.ScaleX;
            tt.Y = absoluteY - relativeCenter.Y * st.ScaleY;
        }

        // Initiate translational dragging.
        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_manipulationState != ManipulationState.None)
                return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ZoomOut(e);
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ZoomIn(e);
            }

            _manipulationState = ManipulationState.MouseDrag;
            var tt = GetTranslateTransform(Child);
            _mouseDragAbsoluteStart = e.GetPosition(this);
            _originalTranslation = new Point(tt.X, tt.Y);
            _thresholdExceeded = false;

            Cursor = Cursors.Hand;
            Child.CaptureMouse();
        }

        // Initiate zoom dragging.
        private void Child_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_manipulationState != ManipulationState.None)
                return;

            _manipulationState = ManipulationState.MouseZoom;
            _mouseZoomRelativeStart = e.GetPosition(Child);
            _mouseDragAbsoluteStart = e.GetPosition(this);
            _lastMouseAbsoluteY = _mouseDragAbsoluteStart.Y;
            _thresholdExceeded = false;

            Cursor = Cursors.Hand;
            Child.CaptureMouse();
        }

        // Process translational or zoom dragging updates.
        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Child.IsMouseCaptured)
                return;

            if (_manipulationState == ManipulationState.MouseDrag)
            {
                var tt = GetTranslateTransform(Child);
                var v = _mouseDragAbsoluteStart - e.GetPosition(this);
                tt.X = _originalTranslation.X - v.X;
                tt.Y = _originalTranslation.Y - v.Y;

                if (v.LengthSquared > DragThresholdSquared)
                    _thresholdExceeded = true;
            }

            if (_manipulationState == ManipulationState.MouseZoom)
            {
                var currentAbsolutePosition = e.GetPosition(this);
                double dy = _lastMouseAbsoluteY - currentAbsolutePosition.Y;
                double scale = 1 + dy / 200;
                ZoomOnPoint(_mouseZoomRelativeStart, scale);
                _lastMouseAbsoluteY = currentAbsolutePosition.Y;

                if (Math.Abs(currentAbsolutePosition.Y - _mouseDragAbsoluteStart.Y) > DragThreshold)
                    _thresholdExceeded = true;
            }
        }

        // End either kind of dragging, consuming the event if the drag threshold was exceeded.
        private void Child_MouseEitherButtonUp(object sender, MouseButtonEventArgs e)
        {
            _manipulationState = ManipulationState.None;
            Child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;

            // If we dragged a distance larger than our threshold, handle the up event so that
            // it's not treated as a click on a skill node.
            if (_thresholdExceeded)
                e.Handled = true;
        }

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn(e);
            else
                ZoomOut(e);
        }

        // Initiate touch dragging/pinching.
        private void Child_ManipulationStarting(object? sender, ManipulationStartingEventArgs e)
        {
            if (_manipulationState != ManipulationState.None)
                return;

            _manipulationState = ManipulationState.Touch;
            _thresholdExceeded = false;
            // Things would be easier if we made the child the container to use as reference
            // for all calculations, but changing the child transformations while manipulation
            // events are coming in (and are calculated based on outdated transformation values)
            // apparently leads to oscillations.
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
            e.IsSingleTouchEnabled = true;
            e.Handled = true;
        }

        // Process touch dragging/pinching updates.
        private void Child_ManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
        {
            if (e.IsInertial)
                e.Complete();

            TranslateTransform tt = GetTranslateTransform(Child);

            // Apply any translation (from drag or pinch).
            var translationDelta = e.DeltaManipulation.Translation;
            tt.X += translationDelta.X;
            tt.Y += translationDelta.Y;
            
            // Apply any zoom (from pinch).
            var absoluteOrigin = e.ManipulationOrigin;
            // Using the above transforms alone is not enough because there are more (due to layout).
            var relativeOrigin = TranslatePoint(absoluteOrigin, Child);
            var scale = e.DeltaManipulation.Scale.X;
            ZoomOnPoint(relativeOrigin, scale);

            if (e.CumulativeManipulation.Translation.LengthSquared > DragThresholdSquared)
                _thresholdExceeded = true;

            if (e.CumulativeManipulation.Scale.X != 1.0)
                _thresholdExceeded = true;

            e.Handled = true;
        }

        // End touch dragging/pinching.
        private void Child_ManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            _manipulationState = ManipulationState.None;

            // If no real manipulation (zoom or translation) happened, cancel this 
            // manipulation event. This results in equivalent mouse events being raised
            // instead which the other interaction code can handle.
            if (!_thresholdExceeded)
                e.Cancel();
        }


        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion
    }
}