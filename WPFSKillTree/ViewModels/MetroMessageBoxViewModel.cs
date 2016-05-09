using POESKillTree.Model;
using POESKillTree.Views;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels
{
    public class MetroMessageBoxViewModel : Notifier
    {
        #region fields
        private string _title;
        private string _message;
        private MessageBoxButton _buttons;
        private MessageBoxImage _image;
        private ImageSource _imageSource;

        private MetroMessageBoxView _view;

        private bool _isYesVisible;
        private bool _isNoVisible;
        private bool _isOKVisible;
        private bool _isCancelVisible;

        private bool _isYesDefault;
        private bool _isNoDefault;
        private bool _isOKDefault;
        private bool _isCancelDefault;


        private ICommand _yesCommand;
        private ICommand _noCommand;
        private ICommand _okCommand;
        private ICommand _cancelCommand;
        private ICommand _escapeCommand;
        private ICommand _closeCommand;

        #endregion

        public MetroMessageBoxViewModel(MetroMessageBoxView mmbView, string message, string title, MessageBoxButton buttons, ImageSource imageSource)
        {
            _view = mmbView;

            Message = message;
            BoxTitle = title;
            Buttons = buttons;
            NotificationImageSource = imageSource;
        }

        #region properties
        public MessageBoxButton Buttons
        {
            get { return _buttons; }
            set
            {
                _buttons = value;
                switch (_buttons)
                {
                    case MessageBoxButton.YesNo:
                        IsYesVisible = IsNoVisible = true;
                        IsYesDefault = true;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        IsYesVisible = IsNoVisible = IsCancelVisible = true;
                        IsYesDefault = true;
                        break;
                    case MessageBoxButton.OK:
                        IsOKVisible = true;
                        IsOKDefault = true;
                        break;
                    case MessageBoxButton.OKCancel:
                        IsOKVisible = IsCancelVisible = true;
                        IsOKDefault = true;
                        break;
                }
            }
        }

        #region commands
        public ICommand YesCommand
        {
            get
            {
                if (_yesCommand == null)
                {
                    _yesCommand = new RelayCommand(args =>
                    {
                        this.Result = MessageBoxResult.Yes;
                        _view.Close();
                    });
                }
                return _yesCommand;
            }
        }
        public ICommand NoCommand
        {
            get
            {
                if (_noCommand == null)
                {
                    _noCommand = new RelayCommand(args =>
                    {
                        this.Result = MessageBoxResult.No;
                        _view.Close();
                    });
                }
                return _noCommand;
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(args =>
                    {
                        this.Result = MessageBoxResult.Cancel;
                        _view.Close();
                    });
                }
                return _cancelCommand;
            }
        }
        public ICommand OKCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(args =>
                    {
                        this.Result = MessageBoxResult.OK;
                        _view.Close();
                    });
                }
                return _okCommand;
            }
        }
        public ICommand EscapeCommand
        {
            get
            {
                if (_escapeCommand == null)
                {
                    _escapeCommand = new RelayCommand(args =>
                    {
                        this.Result = IsCancelVisible ? MessageBoxResult.Cancel :
                            (IsNoVisible ? MessageBoxResult.No : MessageBoxResult.OK);
                        _view.Close();
                    });

                }
                return _escapeCommand;
            }
        }
        #endregion

        #region elements content
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public string BoxTitle
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public MessageBoxResult Result
        {
            get;
            set;
        }

        public ImageSource NotificationImageSource
        {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
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
            set { SetProperty(ref _isYesVisible, value); }
        }
        public bool IsNoVisible
        {
            get { return _isNoVisible; }
            set { SetProperty(ref _isNoVisible, value); }
        }
        public bool IsOKVisible
        {
            get { return _isOKVisible; }
            set { SetProperty(ref _isOKVisible, value); }
        }
        public bool IsCancelVisible
        {
            get { return _isCancelVisible; }
            set { SetProperty(ref _isCancelVisible, value); }
        }
        #endregion

        #region buttons defaults
        public bool IsYesDefault
        {
            get { return _isYesDefault; }
            set { SetProperty(ref _isYesDefault, value); }
        }
        public bool IsNoDefault
        {
            get { return _isNoDefault; }
            set { SetProperty(ref _isNoDefault, value); }
        }
        public bool IsOKDefault
        {
            get { return _isOKDefault; }
            set { SetProperty(ref _isOKDefault, value); }
        }
        public bool IsCancelDefault
        {
            get { return _isCancelDefault; }
            set { SetProperty(ref _isCancelDefault, value); }
        }
        #endregion

        #endregion
    }
}
