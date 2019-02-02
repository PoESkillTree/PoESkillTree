using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace POESKillTree.Controls
{
    public class ZoomBorder : Border
    {
        enum ManipulationState
        {
            NONE, MOUSE_DRAG, MOUSE_ZOOM, TOUCH
        };

        private UIElement _child;

        private Point _originalTranslation;
        private Point _mouseDragAbsoluteStart;
        private Point _mouseZoomRelativeStart;
        private double _lastMouseAbsoluteY;
        private bool _thresholdExceeded;
        private ManipulationState _manipulationState = ManipulationState.NONE;

        const double ZOOM_STEP = 0.3;
        
        const double MAX_ZOOM = 75;
        const double MIN_ZOOM = 0.5;
        
        const double DRAG_THRESHOLD = 5; // (in ZoomBorder coordinates, not _child)
        const double DRAG_THRESHOLD_SQUARED = DRAG_THRESHOLD * DRAG_THRESHOLD;

        public Point Origin
        {
            get
            {
                TranslateTransform tt = GetTranslateTransform(_child);

                return new Point(tt.X, tt.Y);
            }
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && !Equals(value, Child))
                {
                    Initialize(value);
                }

                base.Child = value;
            }
        }

        public ScaleTransform GetScaleTransform(UIElement element)
        {
            return
                (ScaleTransform) ((TransformGroup) element.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }

        public TranslateTransform GetTranslateTransform(UIElement element)
        {
            return
                (TranslateTransform)
                    ((TransformGroup) element.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        public void Initialize(UIElement element)
        {
            _child = element;
            if (_child != null)
            {
                var group = new TransformGroup();

                var st = new ScaleTransform();
                group.Children.Add(st);

                var tt = new TranslateTransform();

                group.Children.Add(tt);

                _child.RenderTransform = group;
                _child.RenderTransformOrigin = new Point(0.0, 0.0);

                _child.MouseWheel += child_MouseWheel;
                _child.MouseLeftButtonDown += child_MouseLeftButtonDown;
                _child.MouseLeftButtonUp += child_MouseEitherButtonUp;
                _child.MouseRightButtonDown += child_MouseRightButtonDown;
                _child.MouseRightButtonUp += child_MouseEitherButtonUp;
                _child.MouseMove += child_MouseMove;

                _child.IsManipulationEnabled = true;
                _child.ManipulationStarting += child_ManipulationStarting;
                _child.ManipulationDelta += child_ManipulationDelta;
                _child.ManipulationCompleted += child_ManipulationCompleted;
            }
        }

        public void Reset()
        {
            if (_child != null)
            {
                // reset zoom
                ScaleTransform st = GetScaleTransform(_child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                TranslateTransform tt = GetTranslateTransform(_child);
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
            if (_child != null)
                ZoomOnPoint(e.GetPosition(_child), 1.0 + ZOOM_STEP);
        }

        public void ZoomOut(dynamic e)
        {
            if (_child != null)
                ZoomOnPoint(e.GetPosition(_child), 1.0 - ZOOM_STEP);
        }

        /// <summary>
        /// Changes the ScaleTransform and TranslateTransform of the child to achieve a zoom
        /// centered on the given point.
        /// </summary>
        /// <param name="relativeCenter">The zoom center relative to the child.</param>
        /// <param name="zoomFactor">Factor (around 1.0) by how much to zoom in or out.</param>
        public void ZoomOnPoint(Point relativeCenter, double zoomFactor)
        {
            if (_child == null)
                return;

            if (_manipulationState == ManipulationState.MOUSE_DRAG)
                return;

            TranslateTransform tt = GetTranslateTransform(_child);
            ScaleTransform st = GetScaleTransform(_child);

            if (st.ScaleX * zoomFactor < MIN_ZOOM || st.ScaleY * zoomFactor < MIN_ZOOM)
                return;
            if (st.ScaleX * zoomFactor > MAX_ZOOM || st.ScaleY * zoomFactor > MAX_ZOOM)
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
        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_manipulationState != ManipulationState.NONE)
                return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ZoomOut(e);
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ZoomIn(e);
            }

            _manipulationState = ManipulationState.MOUSE_DRAG;
            var tt = GetTranslateTransform(_child);
            _mouseDragAbsoluteStart = e.GetPosition(this);
            _originalTranslation = new Point(tt.X, tt.Y);
            _thresholdExceeded = false;

            Cursor = Cursors.Hand;
            _child.CaptureMouse();
        }

        // Initiate zoom dragging.
        private void child_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_manipulationState != ManipulationState.NONE)
                return;

            _manipulationState = ManipulationState.MOUSE_ZOOM;
            _mouseZoomRelativeStart = e.GetPosition(_child);
            _mouseDragAbsoluteStart = e.GetPosition(this);
            _lastMouseAbsoluteY = _mouseDragAbsoluteStart.Y;
            _thresholdExceeded = false;

            Cursor = Cursors.Hand;
            _child.CaptureMouse();
        }

        // Process translational or zoom dragging updates.
        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_child.IsMouseCaptured)
                return;

            if (_manipulationState == ManipulationState.MOUSE_DRAG)
            {
                var tt = GetTranslateTransform(_child);
                var v = _mouseDragAbsoluteStart - e.GetPosition(this);
                tt.X = _originalTranslation.X - v.X;
                tt.Y = _originalTranslation.Y - v.Y;

                if (v.LengthSquared > DRAG_THRESHOLD_SQUARED)
                    _thresholdExceeded = true;
            }

            if (_manipulationState == ManipulationState.MOUSE_ZOOM)
            {
                var currentAbsolutePosition = e.GetPosition(this);
                double dy = _lastMouseAbsoluteY - currentAbsolutePosition.Y;
                double scale = 1 + dy / 200;
                ZoomOnPoint(_mouseZoomRelativeStart, scale);
                _lastMouseAbsoluteY = currentAbsolutePosition.Y;

                if (Math.Abs(currentAbsolutePosition.Y - _mouseDragAbsoluteStart.Y) > DRAG_THRESHOLD)
                    _thresholdExceeded = true;
            }
        }

        // End either kind of dragging, consuming the event if the drag threshold was exceeded.
        private void child_MouseEitherButtonUp(object sender, MouseButtonEventArgs e)
        {
            _manipulationState = ManipulationState.NONE;
            _child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;

            // If we dragged a distance larger than our threshold, handle the up event so that
            // it's not treated as a click on a skill node.
            if (_thresholdExceeded)
                e.Handled = true;
        }

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn(e);
            else
                ZoomOut(e);
        }

        // Initiate touch dragging/pinching.
        private void child_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            if (_manipulationState != ManipulationState.NONE)
                return;

            _manipulationState = ManipulationState.TOUCH;
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
        private void child_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.IsInertial)
                e.Complete();

            TranslateTransform tt = GetTranslateTransform(_child);
            ScaleTransform st = GetScaleTransform(_child);

            // Apply any translation (from drag or pinch).
            var translationDelta = e.DeltaManipulation.Translation;
            tt.X += translationDelta.X;
            tt.Y += translationDelta.Y;
            
            // Apply any zoom (from pinch).
            var absoluteOrigin = e.ManipulationOrigin;
            // Using the above transforms alone is not enough because there are more (due to layout).
            var relativeOrigin = TranslatePoint(absoluteOrigin, _child);
            var scale = e.DeltaManipulation.Scale.X;
            ZoomOnPoint(relativeOrigin, scale);

            if (e.CumulativeManipulation.Translation.LengthSquared > DRAG_THRESHOLD_SQUARED)
                _thresholdExceeded = true;

            if (e.CumulativeManipulation.Scale.X != 1.0)
                _thresholdExceeded = true;

            e.Handled = true;
        }

        // End touch dragging/pinching.
        private void child_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            _manipulationState = ManipulationState.NONE;

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