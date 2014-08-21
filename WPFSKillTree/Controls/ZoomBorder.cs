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
                _child.KeyUp += child_KeyDown;
                _child.PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
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

        private void ZoomIn(dynamic e)
        {
            if (_child != null)
            {
                TranslateTransform tt = GetTranslateTransform(_child);
                ScaleTransform st = GetScaleTransform(_child);

                const double zoom = .3;
                Point relative = e.GetPosition(_child);

                double abosuluteX = relative.X * st.ScaleX + tt.X;
                double abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom * st.ScaleX;
                st.ScaleY += zoom * st.ScaleY;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void ZoomOut(dynamic e)
        {
            if (_child != null)
            {
                TranslateTransform tt = GetTranslateTransform(_child);
                ScaleTransform st = GetScaleTransform(_child);

                if ((st.ScaleX < 0.4 || st.ScaleY < 0.4))
                    return;

                const double zoom = -.3;
                Point relative = e.GetPosition(_child);

                double abosuluteX = relative.X * st.ScaleX + tt.X;
                double abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom * st.ScaleX;
                st.ScaleY += zoom * st.ScaleY;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_KeyDown(object sender, KeyEventArgs k)
        {
            if (_child != null)
            {
                var st = GetScaleTransform(_child);
                var tt = GetTranslateTransform(_child);

                double zoom;

                switch (k.Key)
                {
                    case Key.Add:
                        zoom = .3;
                        break;
                    case Key.Subtract:
                        zoom = -.3;
                        break;
                    default:
                        zoom = 0;
                        break;
                }

                if ((st.ScaleX < 0.4 || st.ScaleY < 0.4))
                    return;

                var relative = _origin;

                var abosuluteX = relative.X * st.ScaleX + tt.X;
                var abosuluteY = relative.Y * st.ScaleY + tt.Y;

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

                var tt = GetTranslateTransform(_child);
                _start = e.GetPosition(this);
                _origin = new Point(tt.X, tt.Y);
                Cursor = Cursors.Hand;
                _child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Not sure if this takes the zoom factor into account, but this feels reasonable.
            const double dragThreshold = 5;

            if (_child != null)
            {
                _child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;

                if ((_start - e.GetPosition(this)).LengthSquared >= dragThreshold * dragThreshold)
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
                var st = GetScaleTransform(_child);

                var zoom = e.Delta > 0 ? .3 : -.3;
                if (!(e.Delta > 0) && (st.ScaleX < 0.4 || st.ScaleY < 0.4))
                    return;

                if (zoom > 0)
                    ZoomIn(e);
                else
                    ZoomOut(e);
            }
        }

        private void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Reset();
        }


        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion
    }
}