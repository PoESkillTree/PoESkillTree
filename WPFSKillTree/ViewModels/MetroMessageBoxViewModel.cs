using POESKillTree.Model;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace POESKillTree.ViewModels
{
    public class MetroMessageBoxViewModel : CloseableViewModel
    {
        #region fields
        private readonly string _message;
        private readonly string _details;
        private readonly ImageSource _imageSource;

        private readonly bool _isYesVisible;
        private readonly bool _isNoVisible;
        private readonly bool _isOKVisible;
        private readonly bool _isCancelVisible;

        private ICommand _yesCommand;
        private ICommand _noCommand;
        private ICommand _okCommand;
        private ICommand _cancelCommand;
        #endregion

        public MetroMessageBoxViewModel(string message, string details, string title, MessageBoxButton buttons, ImageSource imageSource)
        {
            _message = message;
            _details = details;
            DisplayName = title;
            switch (buttons)
            {
                case MessageBoxButton.YesNo:
                    _isYesVisible = _isNoVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    _isYesVisible = _isNoVisible = _isCancelVisible = true;
                    break;
                case MessageBoxButton.OK:
                    _isOKVisible = true;
                    break;
                case MessageBoxButton.OKCancel:
                    _isOKVisible = _isCancelVisible = true;
                    break;
            }
            _imageSource = imageSource;
        }

        #region commands
        public ICommand YesCommand
        {
            get
            {
                return _yesCommand ?? (_yesCommand = new RelayCommand(args =>
                {
                    Result = MessageBoxResult.Yes;
                    CloseCommand.Execute(null);
                }));
            }
        }
        public ICommand NoCommand
        {
            get
            {
                return _noCommand ?? (_noCommand = new RelayCommand(args =>
                {
                    Result = MessageBoxResult.No;
                    CloseCommand.Execute(null);
                }));
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(args =>
                {
                    Result = MessageBoxResult.Cancel;
                    CloseCommand.Execute(null);
                }));
            }
        }
        public ICommand OKCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(args =>
                {
                    Result = MessageBoxResult.OK;
                    CloseCommand.Execute(null);
                }));
            }
        }
        #endregion

        #region elements content
        public string Message
        {
            get { return _message; }
        }

        public string Details
        {
            get { return _details; }
        }

        public MessageBoxResult Result { get; set; }

        public ImageSource NotificationImageSource
        {
            get { return _imageSource; }
        }

        public string ImageColumnWidth
        {
            get { return _imageSource == null ? "0" : "Auto"; }
        }
        #endregion

        #region buttons visibility
        public bool IsYesVisible
        {
            get { return _isYesVisible; }
        }
        public bool IsNoVisible
        {
            get { return _isNoVisible; }
        }
        public bool IsOKVisible
        {
            get { return _isOKVisible; }
        }
        public bool IsCancelVisible
        {
            get { return _isCancelVisible; }
        }
        #endregion
    }
}
