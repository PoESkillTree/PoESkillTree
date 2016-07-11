using System.Collections.ObjectModel;
using System.ComponentModel;
using POESKillTree.Model.Builds;

namespace POESKillTree.ViewModels.Builds
{
    public interface IBuildViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        IBuildFolderViewModel Parent { get; set; }

        bool IsSelected { get; set; }

        IBuild Build { get; }

        void ApplyFilter();
    }

    public interface IBuildViewModel<out T> : IBuildViewModel
        where T : IBuild
    {
        new T Build { get; }
    }

    public interface IBuildFolderViewModel : IBuildViewModel<BuildFolder>
    {
        ObservableCollection<IBuildViewModel> Children { get; }
    }
}