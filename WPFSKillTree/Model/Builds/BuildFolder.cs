using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace POESKillTree.Model.Builds
{
    /// <summary>
    /// Build implementation that represents a folder which can contain other <see cref="IBuild"/>s.
    /// A folder hierarchy must be a tree, i.e. each build is only in one folder and there are no cycles.
    /// </summary>
    public class BuildFolder : AbstractBuild<BuildFolder>
    {
        private bool _isExpanded = true;

        /// <summary>
        /// Gets or sets whether the contents of this folder should be visible in all visual representations
        /// of this build.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        /// <summary>
        /// Gets the observable collection that contains all <see cref="IBuild"/>s of this folder.
        /// </summary>
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

        /// <summary>
        /// Enumerates all leafs (<see cref="PoEBuild"/>s) starting from this tree node.
        /// </summary>
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

        /// <summary>
        /// Enumerates all intermediate nodes (<see cref="BuildFolder"/>s) starting from this tree node.
        /// </summary>
        public IEnumerable<BuildFolder> FoldersPreorder()
        {
            yield return this;
            foreach (var build in Builds)
            {
                var b = build as BuildFolder;
                if (b == null)
                    continue;
                yield return b;
                foreach (var child in b.FoldersPreorder())
                {
                    yield return child;
                }
            }
        }
    }
}