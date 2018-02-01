using System.Collections.Generic;
using MoreLinq;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class StatGraphCollectionSuspender : ISuspendableEvents
    {
        private readonly IEnumerable<IReadOnlyStatGraph> _statGraphs;

        public StatGraphCollectionSuspender(IEnumerable<IReadOnlyStatGraph> statGraphs) =>
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