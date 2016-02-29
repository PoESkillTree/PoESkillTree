using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public class BottleneckSteinerDistanceCalculator
    {

        private class SMatrixLookup : IDistanceLookup
        {
            public int CacheSize { get; private set; }

            private readonly uint[,] _smatrix;

            public uint this[int a, int b]
            {
                get { return _smatrix[a, b]; }
            }

            public SMatrixLookup(int cacheSize, uint[,] smatrix)
            {
                CacheSize = cacheSize;
                _smatrix = smatrix;
            }
        }

        private readonly IDistanceLookup _distances;

        private List<int> _fixedTargetNodes;

        public BottleneckSteinerDistanceCalculator(IDistanceLookup distances)
        {
            _distances = distances;
        }

        public IDistanceLookup CalcBottleneckSteinerDistances(IEnumerable<GraphNode> fixedTargetNodes)
        {
            _fixedTargetNodes = fixedTargetNodes.Select(n => n.DistancesIndex).ToList();

            var nodeCount = _distances.CacheSize;
            var smatrix = new uint[nodeCount, nodeCount];
            var searchSpaceIndices = Enumerable.Range(0, nodeCount).ToList();
            // For each node i
            for (var i = 0; i < nodeCount; i++)
            {
                var labels = CalcBottleneckSteinerDistancesTo(i, searchSpaceIndices);

                foreach (var j in searchSpaceIndices)
                {
                    smatrix[i, j] = labels[j];
                }
            }
            return new SMatrixLookup(nodeCount, smatrix);
        }

        private uint[] CalcBottleneckSteinerDistancesTo(int from, IEnumerable<int> to)
        {
            // All not permanetly labeled target nodes.
            var targetsRemaining = new HashSet<int>(_fixedTargetNodes);
            targetsRemaining.Remove(from);
            // All not permanently labeled nodes (neigbors and targets, we don't care about other nodes).
            var nodesRemaining = new HashSet<int>(targetsRemaining);
            nodesRemaining.UnionWith(to);

            // Labels of nodes, approaches special distance to i.
            var labels = new uint[_distances.CacheSize];
            foreach (var j in nodesRemaining)
            {
                // Initialize labels with distance.
                labels[j] = _distances[from, j];
            }

            // While not all target nodes were labeled.
            while (targetsRemaining.Any())
            {
                // Determine the not permanently labeled target node k* with smallest label and select it.
                var max = uint.MaxValue;
                var kstar = -1;
                foreach (var t in targetsRemaining)
                {
                    if (labels[t] < max)
                    {
                        max = labels[t];
                        kstar = t;
                    }
                }
                // Label k* permanently.
                targetsRemaining.Remove(kstar);
                nodesRemaining.Remove(kstar);

                // For all not permanently labeled nodes.
                foreach (var j in nodesRemaining)
                {
                    // Decrease label if the bottleneck length on the path over k* is smaller.
                    labels[j] = Math.Min(labels[j], Math.Max(labels[kstar], _distances[kstar, j]));
                }
            }

            return labels;
        }

    }
}