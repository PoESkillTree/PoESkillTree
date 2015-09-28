using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.
    /// </summary>
    public abstract class ViewModelBase : Notifier
    {
        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName { get; protected set; }
    }
}
