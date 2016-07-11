using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace POESKillTree.Model.Builds
{
    public class BuildFolder : AbstractBuild<BuildFolder>
    {
        private bool _isExpanded = true;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        public ObservableCollection<IBuild> Builds { get; } = new ObservableCollection<IBuild>();

        public override BuildFolder DeepClone()
        {
            var o = (BuildFolder) SafeMemberwiseClone();
            o.Builds.Clear();
            foreach (var build in Builds)
            {
                o.Builds.Add(build.DeepClone());
            }
            return o;
        }

        public IEnumerable<PoEBuild> BuildsPreorder()
        {
            foreach (var build in Builds)
            {
                var b = build as PoEBuild;
                if (b != null)
                {
                    yield return b;
                }
                else
                {
                    foreach (var child in ((BuildFolder) build).BuildsPreorder())
                    {
                        yield return child;
                    }
                }
            }
        }
    }
}