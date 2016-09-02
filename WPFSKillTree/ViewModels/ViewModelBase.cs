using POESKillTree.Utils;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.
    /// </summary>
    public abstract class ViewModelBase : Notifier
    {
        private string _displayName;
        /// <summary>
        /// Returns the user-friendly name of this object.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            protected set { SetProperty(ref _displayName, value); }
        }
    }
}
