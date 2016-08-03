using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    public class FileSelectorViewModel : CloseableViewModel<string>, INotifyDataErrorInfo
    {
        private string _filePath;
        private string _sanitizedFilePath;
        private readonly bool _isFolderPicker;
        private string _filePathError;
        private bool _isCancelable = true;

        public string Message { get; }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                SetProperty(ref _filePath, value, () =>
                {
                    if (CheckPathValidity())
                    {
                        SanitizedFilePath = FilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    }
                });
            }
        }

        public string SanitizedFilePath
        {
            get { return _sanitizedFilePath; }
            set { SetProperty(ref _sanitizedFilePath, value); }
        }

        public ICommand SelectFileCommand { get; }

        public bool HasErrors
        {
            get { return _filePathError != null; }
        }

        public bool IsCancelable
        {
            get { return _isCancelable; }
            set { SetProperty(ref _isCancelable, value); }
        }

        public FileSelectorViewModel(string title, string message, string defaultFile, bool isFolderPicker)
        {
            DisplayName = title;
            Message = message;
            FilePath = defaultFile;
            _isFolderPicker = isFolderPicker;
            SelectFileCommand = new RelayCommand(SelectFile);
        }

        private void SelectFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = _isFolderPicker,
                InitialDirectory = Path.GetDirectoryName(SanitizedFilePath),
                DefaultFileName = Path.GetFileName(SanitizedFilePath)
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dialog.FileName;
            }
        }

        private bool CheckPathValidity()
        {
            string message = null;
            var path = FilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrEmpty(path))
            {
                message = L10n.Message("Value is required.");
            }
            else if (string.IsNullOrWhiteSpace(path))
            {
                message = L10n.Message("Value only contains white space characters.");
            }
            else
            {
                try
                {
                    var fi = new FileInfo(path);
                    if (fi.Exists && File.Exists(path))
                    {
                        message = L10n.Message("Path exists and is not a directory.");
                    }
                }
                catch (ArgumentException)
                {
                    message = L10n.Message("Value contains invalid characters.");
                }
                catch (UnauthorizedAccessException)
                {
                    message = L10n.Message("Path could not be accessed.");
                }
                catch (PathTooLongException)
                {
                    message = L10n.Message("Value is too long.");
                }
                catch (NotSupportedException)
                {
                    message = L10n.Message("Value contains a colon at an invalid position-");
                }
            }

            if (_filePathError != message)
            {
                _filePathError = message;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(FilePath)));
            }

            return message == null;
        }

        protected override bool CanClose(string param)
        {
            return (!HasErrors || param == null) && base.CanClose(param);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(FilePath) && _filePathError != null)
            {
                return new[] {_filePathError};
            }
            return null;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}