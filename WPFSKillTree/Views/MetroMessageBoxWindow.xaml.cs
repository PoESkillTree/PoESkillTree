using MahApps.Metro.Controls;
using POESKillTree.ViewModels;
using System;
using System.Drawing;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for MetroMessageBox.xaml
    /// </summary>
    public partial class MetroMessageBoxView : MetroWindow
    {
        public MetroMessageBoxView()
        {
            this.WindowStyle = WindowStyle.None;
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
        }

        private double GetActualLeft(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                var leftField = typeof(Window).GetField("_actualLeft",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (double)leftField.GetValue(window);
            }
            else
                return window.Left;
        }

        #region events
        protected override void OnActivated(EventArgs e)
        {
            if (Owner != null)
            {
                /*
                 * this.Left = WindowActualLeft(Owner);
                 * this.Width = Owner.ActualWidth
                 * 
                * TODO: it looks quite nice when messagebox is stretched to its parents width,
                * but it also looks very awkward if you try to move it, especially in fullscreen. 
                * Then, how to disable it from moving?
                */
                this.Left = GetActualLeft(Owner) + (Owner.ActualWidth - this.ActualWidth) / 2;
                this.Top = Owner.Top + (Owner.ActualHeight - this.ActualHeight) / 2;
            } // else, window is centered on the screen
            base.OnActivated(e);
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && DataContext != null)
            {
                (DataContext as MetroMessageBoxViewModel).EscapeCommand.Execute(null);
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
        #endregion
    }
    public class MetroMessageBox
    {
        public static ImageSource MessageBoxImageToImageSource(MessageBoxImage image)
        {
            Icon icon;
            switch (image)
            {
                case MessageBoxImage.None:
                    icon = null;
                    break;
                case MessageBoxImage.Error: // also MessageBoxImage.Hand ans MessageBoxImage.Stop:
                    icon = SystemIcons.Hand;
                    break;
                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;
                case MessageBoxImage.Warning: // also MessageBoxImage.Exclamation
                    icon = SystemIcons.Exclamation;
                    break;
                case MessageBoxImage.Information: //case MessageBoxImage.Asterisk
                    icon = SystemIcons.Asterisk;
                    break;
                default:
                    icon = SystemIcons.Application;
                    break;
            }
            return (icon == null) ? null : Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static SystemSound MessageBoxImageToSystemSound(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Hand: // also MessageBoxImage.Error ans MessageBoxImage.Stop:
                    return SystemSounds.Hand;
                case MessageBoxImage.Question:
                    return SystemSounds.Question;
                case MessageBoxImage.Exclamation: // also MessageBoxImage.Warning
                    return SystemSounds.Exclamation;
                case MessageBoxImage.Asterisk: //case MessageBoxImage.Information
                    return SystemSounds.Asterisk;
                default:
                    return SystemSounds.Beep;
            }
        }

        public static MessageBoxResult Show(string message, string title = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None, MessageBoxResult defaultResult=MessageBoxResult.OK, bool playSystemSound = true)
        {
            return ConstrucBox(null, message, title, buttons, image, defaultResult, playSystemSound);
        }
        public static MessageBoxResult Show(Window owner, string message, string title = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK, bool playSystemSound = true)
        {
            return ConstrucBox(owner, message, title, buttons, image, defaultResult, playSystemSound);
        }


        private static MessageBoxResult ConstrucBox(Window owner, string message, string title, MessageBoxButton buttons, MessageBoxImage image, 
            MessageBoxResult defaultResult, bool playSystemSound)
        {
            MetroMessageBoxView mmbView = new MetroMessageBoxView();
            MetroMessageBoxViewModel mmbViewModel = new MetroMessageBoxViewModel(mmbView, message, title, buttons, MessageBoxImageToImageSource(image));

            mmbView.Owner = owner;
            mmbView.DataContext = mmbViewModel;
            mmbViewModel.Result = defaultResult;

            MessageBoxImageToSystemSound(image).Play();

            mmbView.ShowDialog();
            return mmbViewModel.Result;
        }
    }
}
