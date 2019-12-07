using System.Windows;
using MahApps.Metro.IconPacks;
using PoESkillTree.Common.ViewModels;

namespace PoESkillTree.Controls.Dialogs.ViewModels
{
    public class MetroMessageBoxViewModel : CloseableViewModel<MessageBoxResult>
    {
        public MetroMessageBoxViewModel(string message, string? details, string title, MessageBoxButton buttons,
            PackIconModernKind? imageKind)
        {
            Message = message;
            Details = details;
            DisplayName = title;
            switch (buttons)
            {
                case MessageBoxButton.YesNo:
                    IsYesVisible = IsNoVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    IsYesVisible = IsNoVisible = IsCancelVisible = true;
                    break;
                case MessageBoxButton.OK:
                    IsOKVisible = true;
                    break;
                case MessageBoxButton.OKCancel:
                    IsOKVisible = IsCancelVisible = true;
                    break;
            }
            ImageKind = imageKind;
        }

        #region elements content
        public string Message { get; }

        public string? Details { get; }

        public PackIconModernKind? ImageKind { get; }
        #endregion

        #region buttons visibility
        public bool IsYesVisible { get; }

        public bool IsNoVisible { get; }

        public bool IsOKVisible { get; }

        public bool IsCancelVisible { get; }
        #endregion
    }
}
