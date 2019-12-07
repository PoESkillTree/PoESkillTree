using PoESkillTree.Utils;

namespace PoESkillTree.Model.Builds
{
    /// <summary>
    /// Abstract base class for <see cref="IBuild"/>. The notify interfaces are implemented
    /// via <see cref="Notifier"/>.
    /// </summary>
    /// <typeparam name="T">Type of the implementing class.</typeparam>
    public abstract class AbstractBuild<T> : Notifier, IBuild
        where T : IBuild
    {
        private string _name = "";

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Returns a deep copy of this build. (event handlers are NOT cloned)
        /// </summary>
        public abstract T DeepClone();

        IBuild IBuild.DeepClone()
        {
            return DeepClone();
        }
    }
}