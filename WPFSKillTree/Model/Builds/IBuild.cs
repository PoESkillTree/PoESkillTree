using System.ComponentModel;

namespace PoESkillTree.Model.Builds
{
    /// <summary>
    /// Interface for builds. Builds have a name, can be deep cloned and implement both
    /// <see cref="INotifyPropertyChanged"/> and <see cref="INotifyPropertyChanging"/>.
    /// </summary>
    public interface IBuild : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Gets or sets the name of this build.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns a deep copy of this build. (event handlers are NOT cloned)
        /// </summary>
        IBuild DeepClone();
    }
}