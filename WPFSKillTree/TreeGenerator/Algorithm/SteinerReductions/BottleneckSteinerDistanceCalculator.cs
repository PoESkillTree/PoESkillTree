using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// Calculates bottleneck Steiner distances between all search space nodes.
    /// </summary>
    /// <remarks>
    /// "bottleneck distance": The bottleneck distance of a path is the weight of the longest
    /// edge on the path.
    /// 
    /// "special path": A special path is a path over the distance graph (fully connected graph
    /// between all nodes with the distances between the nodes as edge weights) of which all
    /// intermediary nodes are target nodes (can be none).
    /// 
    /// "special distance" or "bottleneck Steiner distance": The special distance between two
    /// nodes is the minimum bottleneck distance over all special paths between these nodes
    /// in the distance nodes.
    /// 
    /// These distances are used for <see cref="PathsWithManyTerminalsTest"/> and
    /// <see cref="NonTerminalsOfDegreeKTest"/>.
    /// 
    /// Source (original definition of the special distance and the basic algorithm idea):
    ///     C. W. Duin, A. Volgenant (1989): "An edge elimination test for the steiner problem in graphs"
    /// Source for the used "Steiner distance" terminology:
    ///     T. Polzin (2003): "Algorithms for the Steiner Problem in Networks", p. 50
    ///     (freely available: http://scidok.sulb.uni-saarland.de/volltexte/2004/218/pdf/TobiasPolzin_ProfDrKurtMehlhorn.pdf)
    /// </remarks>
    public class BottleneckSteinerDistanceCalculator
    {

        /// <summary>
        /// Trivial implementation of <see cref="IDistanceLookup"/>. Serves as a lookup for 
        /// bottleneck Steiner distances.
        /// </summary>
        private class SMatrixLookup : IDistanceLookup
        {
            public int CacheSize { get; private set; }

            private readonly uint[][] _smatrix;

            public uint this[int a, int b]
            {
                get { return _smatrix[a][b]; }
            }

            public SMatrixLookup(int cacheSize, uint[][] smatrix)
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

        /// <summary>
        /// Calculates the bottleneck Steiner distances for all nodes in the search space with respect
        /// to the given target nodes.
        /// </summary>
        /// <returns>Lookup for the calculated distances between all nodes.</returns>
        public IDistanceLookup CalcBottleneckSteinerDistances(IEnumerable<int> fixedTargetNodes)
        {
            _fixedTargetNodes = fixedTargetNodes.ToList();

            var nodeCount = _distances.CacheSize;
            var smatrix = new uint[nodeCount][];
            var searchSpaceIndices = Enumerable.Range(0, nodeCount).ToList();
            // Calculate values for each node.
            for (var i = 0; i < nodeCount; i++)
            {
                smatrix[i] = CalcBottleneckSteinerDistancesTo(i, searchSpaceIndices);
            }
            return new SMatrixLookup(nodeCount, smatrix);
        }

        private uint[] CalcBottleneckSteinerDistancesTo(int i, IEnumerable<int> searchSpaceIndices)
        {
            // Dijkstra like algorithm that uses the fully connected distance graph but only
            // pathes through target nodes (only paths over target nodes are relevant by definition).
            // We are also not calculating distances over paths but selecting the weight of a single edge:
            // the minimum bottleneck length over all considered paths.

            // All unvisited target nodes.
            var unvisitedTargets = new HashSet<int>(_fixedTargetNodes);
            unvisitedTargets.Remove(i);
            // All unvisited nodes.
            var unvisited = new HashSet<int>(searchSpaceIndices);
            unvisited.Remove(i);

            // Labels of nodes, approaches bottleneck Steiner distance between i and j.
            var labels = new uint[_distances.CacheSize];
            foreach (var j in unvisited)
            {
                // Initialize labels with distance.
                // The bottleneck Steiner is at most this as it is the bottleneck length
                // of the direct path between i and j in the distance graph.
                labels[j] = _distances[i, j];
            }

            // While not all target nodes were visited.
            while (unvisitedTargets.Any())
            {
                // Determine the unvisited target node k with smallest label and visit it.
                // todo Could be made much faster with a priority queue.
                //      Not possible with our current priority queue implementation, though. (DecreasePriority is not implemented)
                var min = uint.MaxValue;
                var k = -1;
                foreach (var t in unvisitedTargets)
                {
                    if (labels[t] < min)
                    {
                        min = labels[t];
                        k = t;
                    }
                }
                unvisitedTargets.Remove(k);
                unvisited.Remove(k);

                // For all unvisited nodes. (because we use the distance graph, all nodes are neighbors)
                foreach (var j in unvisited)
                {
                    // The bottleneck length of the path from i to j over k is either the bottleneck length
                    // of the path from i to k (which was already calculated and is stored in labels[k]) or
                    // the length of the edge from k to j.
                    var bottleneck = Math.Max(labels[k], _distances[k, j]);
                    if (bottleneck < labels[j])
                    {
                        // If this bottleneck is smaller than labels[j], the path from i to j over k has a smaller
                        // bottleneck than the paths that were already checked.
                        labels[j] = bottleneck;
                    }
                    // There might be conditions by which j can be marked as visited at some point but it is efficient
                    // enough as it is now (as long as these distances are only calculated once).
                }
            }

            return labels;
        }

    }
}