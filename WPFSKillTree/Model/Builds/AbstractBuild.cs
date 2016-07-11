using POESKillTree.Utils;

namespace POESKillTree.Model.Builds
{
    public abstract class AbstractBuild<T> : Notifier, IBuild, IDeepCloneable<T>
        where T : IBuild
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public abstract T DeepClone();

        IBuild IBuild.DeepClone()
        {
            return DeepClone();
        }

        object IDeepCloneable.DeepClone()
        {
            return DeepClone();
        }
    }
}