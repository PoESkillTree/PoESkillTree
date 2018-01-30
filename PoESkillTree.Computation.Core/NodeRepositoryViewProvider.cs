using System.Collections.Generic;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class NodeRepositoryViewProvider : ISuspendableEventViewProvider<INodeRepository>
    {
        public NodeRepositoryViewProvider(IReadOnlyStatGraphCollection statGraphCollection)
        {
            var statGraphCollection1 = statGraphCollection;
            DefaultView = new DefaultViewNodeRepository(statGraphCollection1);
            SuspendableView = new SuspendableViewNodeRepository(statGraphCollection1);
            Suspender = new Suspendable(statGraphCollection);
        }

        public INodeRepository DefaultView { get; }

        public INodeRepository SuspendableView { get; }

        public ISuspendableEvents Suspender { get; }


        private class DefaultViewNodeRepository : INodeRepository
        {
            private readonly IReadOnlyStatGraphCollection _statGraphCollection;

            public DefaultViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) => 
                _statGraphCollection = statGraphCollection;

            public ICalculationNode GetNode(IStat stat, NodeType nodeType) => 
                _statGraphCollection.GetOrAdd(stat).GetNode(nodeType).DefaultView;

            public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) =>
                _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form).DefaultView;
        }


        private class SuspendableViewNodeRepository : INodeRepository
        {
            private readonly IReadOnlyStatGraphCollection _statGraphCollection;

            public SuspendableViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) => 
                _statGraphCollection = statGraphCollection;

            public ICalculationNode GetNode(IStat stat, NodeType nodeType) => 
                _statGraphCollection.GetOrAdd(stat).GetNode(nodeType).SuspendableView;

            public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) => 
                _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form).SuspendableView;
        }


        private class Suspendable : ISuspendableEvents
        {
            private readonly IEnumerable<IReadOnlyStatGraph> _statGraphs;

            public Suspendable(IEnumerable<IReadOnlyStatGraph> statGraphs) => 
                _statGraphs = statGraphs;

            private IEnumerable<ISuspendableEvents> SelectSuspendables()
            {
                foreach (var statGraph in _statGraphs)
                {
                    foreach (var node in statGraph.Nodes.Values)
                    {
                        yield return node.Suspender;
                    }

                    foreach (var formCollection in statGraph.FormNodeCollections.Values)
                    {
                        yield return formCollection.Suspender;
                    }
                }
            }

            public void SuspendEvents() => SelectSuspendables().ForEach(s => s.SuspendEvents());

            public void ResumeEvents() => SelectSuspendables().ForEach(s => s.ResumeEvents());
        }
    }
}