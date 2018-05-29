using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Collection of <see cref="PathDefinition"/>s. Implemented as a multi-set: Only the first time a path is added
    /// adds it to the <see cref="ISuspendableEventViewProvider{T}"/> views and its is only removed again the last time
    /// it is removed using <see cref="Remove"/>.
    /// </summary>
    public class PathDefinitionCollection : ISuspendableEventViewProvider<IObservableCollection<PathDefinition>>
    {
        private readonly ISuspendableEventViewProvider<ObservableCollection<PathDefinition>> _viewProvider;
        private readonly IDictionary<PathDefinition, int> _multiSet = new Dictionary<PathDefinition, int>();

        public PathDefinitionCollection(ISuspendableEventViewProvider<ObservableCollection<PathDefinition>> viewProvider)
        {
            _viewProvider = viewProvider;
        }

        public IObservableCollection<PathDefinition> DefaultView => _viewProvider.DefaultView;
        public IObservableCollection<PathDefinition> SuspendableView => _viewProvider.SuspendableView;
        public ISuspendableEvents Suspender => _viewProvider.Suspender;
        public int SubscriberCount => _viewProvider.SubscriberCount;

        public void Add(PathDefinition path)
        {
            _viewProvider.DefaultView.Add(path);
            _viewProvider.SuspendableView.Add(path);

            if (!_multiSet.ContainsKey(path))
            {
                _multiSet[path] = 0;
            }
            _multiSet[path]++;
        }

        public void Remove(PathDefinition path)
        {
            if (!_multiSet.ContainsKey(path))
                return;
            _multiSet[path]--;
            if (_multiSet[path] == 0)
            {
                _multiSet.Remove(path);
                _viewProvider.DefaultView.Remove(path);
                _viewProvider.SuspendableView.Remove(path);
            }
        }
    }
}