using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using POESKillTree.Common.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.Controls.Dialogs.ViewModels
{
    public class FileSelectorViewModel : ErrorInfoViewModel<string>
    {
        private string _filePath;
        private string _sanitizedFilePath;
        private readonly bool _isFolderPicker;
        private bool _isCancelable = true;

        public string Message { get; }

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        public string SanitizedFilePath
        {
            get { return _sanitizedFilePath; }
            set { SetProperty(ref _sanitizedFilePath, value); }
        }

        public ICommand SelectFileCommand { get; }

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

        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            if (propertyName != nameof(FilePath))
                return null;
            string message;
            var trimmed = PathEx.TrimTrailingDirectorySeparators(FilePath);
            if (PathEx.IsPathValid(trimmed, out message, mustBeDirectory: true))
            {
                SanitizedFilePath = trimmed;
            }
            return new[] {message};
        }
    }
}