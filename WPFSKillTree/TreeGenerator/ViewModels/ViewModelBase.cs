using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    // Original source: https://msdn.microsoft.com/en-us/magazine/dd419663.aspx
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class ViewModelBase : Notifier
    {

        #region DisplayName

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName { get; protected set; }

        #endregion // DisplayName
    }
}
