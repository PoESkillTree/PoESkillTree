using System.IO;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    public class FileSelectorViewModel : CloseableViewModel<string>
    {
        private string _filePath;
        private readonly bool _isFolderPicker;

        public string Message { get; }

        public string FilePath
        {
            get { return _filePath; } 
            set { SetProperty(ref _filePath, value); }
        }

        public ICommand SelectFileCommand { get; }

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
                InitialDirectory = Path.GetDirectoryName(FilePath),
                DefaultFileName = Path.GetFileName(FilePath)
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dialog.FileName;
            }
        }
    }
}