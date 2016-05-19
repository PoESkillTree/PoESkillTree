using POESKillTree.ViewModels;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for MetroMessageBox.xaml
    /// </summary>
    public partial class MetroMessageBoxView
    {
        public MetroMessageBoxView()
        {
            InitializeComponent();
        }
    }

    // todo Move
    public static class MetroMessageBox
    {
        private static ImageSource MessageBoxImageToImageSource(MessageBoxImage image)
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

        private static SystemSound MessageBoxImageToSystemSound(MessageBoxImage image)
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

        public static async Task<MessageBoxResult> Show(Window owner, string message, string details = null, string title = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK)
        {
            var view = new MetroMessageBoxView();
            var viewModel = new MetroMessageBoxViewModel(message, details, title, buttons, MessageBoxImageToImageSource(image));

            viewModel.RequestsClose += () => ((MetroWindow) owner).HideMetroDialogAsync(view);
            viewModel.Result = defaultResult;
            view.DataContext = viewModel;
            await ((MetroWindow)owner).ShowMetroDialogAsync(view);

            MessageBoxImageToSystemSound(image).Play();
            await view.WaitUntilUnloadedAsync();

            return viewModel.Result;
        }
    }
}
