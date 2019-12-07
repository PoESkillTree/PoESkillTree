using System.Diagnostics.CodeAnalysis;
using PoESkillTree.Utils;

namespace PoESkillTree.Common.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.
    /// </summary>
    public abstract class ViewModelBase : Notifier
    {
        private string? _displayName;
        /// <summary>
        /// Returns the user-friendly name of this object.
        /// </summary>
        [DisallowNull]
        public string? DisplayName
        {
            get => _displayName;
            protected set => SetProperty(ref _displayName, value);
        }
    }
}
