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
        private UIElement _child;
        private Point _origin;
        private Point _start;
        private bool allowZoom = true;

        const double ZOOM_STEP = 0.3;
        
        const double MAX_ZOOM = 75;
        const double MIN_ZOOM = 0.5;

        // Not sure if this takes the zoom factor into account, but this feels reasonable.
        const double DRAG_THRESHOLD = 5;

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
                _child.MouseLeftButtonUp += child_MouseLeftButtonUp;
                _child.MouseMove += child_MouseMove;
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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //  CaptureMouse();
        }


        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            // if (IsMouseCaptured)
            // {

            //ReleaseMouseCapture();

            if (IsMouseOver)

                RaiseEvent(new RoutedEventArgs(ClickEvent, e));

            //    }
        }

        public void ZoomIn(dynamic e)
        {
            if (!allowZoom)
                return;
            if (_child != null)
            {
                TranslateTransform tt = GetTranslateTransform(_child);
                ScaleTransform st = GetScaleTransform(_child);

                const double zoom = ZOOM_STEP;
                if (st.ScaleX + zoom > MAX_ZOOM || st.ScaleY + zoom > MAX_ZOOM)
                    return;
                Point relative = e.GetPosition(_child);

                double abosuluteX = relative.X * st.ScaleX + tt.X;
                double abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom * st.ScaleX;
                st.ScaleY += zoom * st.ScaleY;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        public void ZoomOut(dynamic e)
        {
            if (!allowZoom)
                return;
            if (_child != null)
            {
                TranslateTransform tt = GetTranslateTransform(_child);
                ScaleTransform st = GetScaleTransform(_child);

                const double zoom = -ZOOM_STEP;
                if (st.ScaleX + zoom < MIN_ZOOM || st.ScaleY + zoom < MIN_ZOOM)
                    return;

                Point relative = e.GetPosition(_child);

                double abosuluteX = relative.X * st.ScaleX + tt.X;
                double abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom * st.ScaleX;
                st.ScaleY += zoom * st.ScaleY;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ZoomOut(e);
                }

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    ZoomIn(e);
                }
                allowZoom = false;
                var tt = GetTranslateTransform(_child);
                _start = e.GetPosition(this);
                _origin = new Point(tt.X, tt.Y);
                Cursor = Cursors.Hand;
                _child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            allowZoom = true;
            if (_child != null)
            {
                _child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;

                if ((_start - e.GetPosition(this)).LengthSquared >= DRAG_THRESHOLD * DRAG_THRESHOLD)
                {
                    // If we dragged a distance larger than our threshold, handle the up event so that
                    // it's not treated as a click on a skill node.
                    e.Handled = true;
                }
            }
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (_child != null)
            {
                if (_child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(_child);
                    var v = _start - e.GetPosition(this);
                    tt.X = _origin.X - v.X;
                    tt.Y = _origin.Y - v.Y;
                }
            }
        }

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child != null)
            {
                if (e.Delta > 0)
                    ZoomIn(e);
                else
                    ZoomOut(e);
            }
        }


        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion
    }
}