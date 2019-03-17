using System.Collections.ObjectModel;
using System.ComponentModel;
using PoESkillTree.Common;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Builds
{
    /// <summary>
    /// Interface for Build view models that wrap a <see cref="IBuild"/> instance and provide additional
    /// UI related functionality. Build view models can be filtered, which currently means that leaf builds can be
    /// hidden.
    /// </summary>
    public interface IBuildViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Gets or sets this view models parent folder.
        /// </summary>
        IBuildFolderViewModel Parent { get; set; }

        /// <summary>
        /// Gets or sets whether this view model is currently selected by the user.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets the build wrapped by this view model.
        /// </summary>
        IBuild Build { get; }

        /// <summary>
        /// Sets the skill tree necessary to update <see cref="PoEBuild.TreeUrl"/> based properties.
        /// Will also set the property on all child nodes.
        /// </summary>
        ISkillTree SkillTree { set; }

        /// <summary>
        /// Applies the filter function to this view model and any children.
        /// </summary>
        void ApplyFilter();
    }

    /// <summary>
    /// <see cref="IBuildViewModel"/> that has a build of type <typeparamref name="T"/>.
    /// </summary>
    public interface IBuildViewModel<out T> : IBuildViewModel
        where T : IBuild
    {
        /// <summary>
        /// Gets the build wrapped by this view model.
        /// </summary>
        new T Build { get; }
    }

    /// <summary>
    /// <see cref="IBuildViewModel"/> interface for view models that wrap <see cref="BuildFolder"/>s.
    /// </summary>
    public interface IBuildFolderViewModel : IBuildViewModel<BuildFolder>
    {
        /// <summary>
        /// Gets the collection of children of this view model. The builds wrapped by the view models of the
        /// collection are the same that are stored in <see cref="BuildFolder.Builds"/>.
        /// </summary>
        ObservableCollection<IBuildViewModel> Children { get; }
    }
}