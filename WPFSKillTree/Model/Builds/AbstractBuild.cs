using POESKillTree.Utils;

namespace POESKillTree.Model.Builds
{
    /// <summary>
    /// Abstract base class for <see cref="IBuild"/>. The notify interfaces are implemented
    /// via <see cref="Notifier"/>.
    /// </summary>
    /// <typeparam name="T">Type of the implementing class.</typeparam>
    public abstract class AbstractBuild<T> : Notifier, IBuild
        where T : IBuild
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// Returns a deep copy of this build.
        /// </summary>
        public abstract T DeepClone();

        IBuild IBuild.DeepClone()
        {
            return DeepClone();
        }
    }
}