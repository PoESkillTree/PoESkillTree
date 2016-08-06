using System;
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
        private readonly string _validationSubPath;

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

        public bool IsCancelable { get; }

        public FileSelectorViewModel(string title, string message, string defaultPath, bool isCancelable,
            bool isFolderPicker, string validationSubPath = null)
        {
            if (!isFolderPicker && !string.IsNullOrEmpty(validationSubPath))
                throw new ArgumentException("validationSubPath may only be given if isFolderPicker is true",
                    nameof(validationSubPath));
            DisplayName = title;
            Message = message;
            FilePath = defaultPath;
            IsCancelable = isCancelable;
            _isFolderPicker = isFolderPicker;
            _validationSubPath = validationSubPath;
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
            if (PathEx.IsPathValid(trimmed, out message, mustBeDirectory: _isFolderPicker, mustBeFile: !_isFolderPicker))
            {
                if (!string.IsNullOrEmpty(_validationSubPath))
                {
                    PathEx.IsPathValid(Path.Combine(trimmed, _validationSubPath), out message);
                }
                SanitizedFilePath = trimmed;
            }
            return new[] {message};
        }
    }
}