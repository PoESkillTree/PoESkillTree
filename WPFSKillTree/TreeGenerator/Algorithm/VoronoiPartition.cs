using System.Collections.Generic;
using System.Linq;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Calculates and enables access to the Voronoi regions of a set of terminals.
    /// A Voronoi region for a terminal is the set of non-terminals that are not closer to
    /// any other terminal.
    /// 
    /// After calculating these, the nearest terminal for each node is directly accessable and
    /// all edges leaving the region of a terminal can easily be calculated.
    /// These values are used in certain reduction tests in <see cref="SteinerPreprocessor"/>.
    /// 
    /// The region and base calculation runs in O(|E| + |V|).
    /// Calculating links for all terminals takes O(|E|) time.
    /// </summary>
    public class VoronoiPartition
    {
        private readonly IReadOnlyGraphEdgeSet _edgeSet;

        private readonly List<int> _terminals;

        /// <summary>
        /// Voronoi regions of each terminal.
        /// </summary>
        private readonly Dictionary<int, HashSet<int>> _regionDict;

        /// <summary>
        /// Edges leaving the voronoi region for each terminal.
        /// </summary>
        private Dictionary<int, List<GraphEdge>> _linksDict;

        /// <summary>
        /// Nearest terminals for each node.
        /// </summary>
        private readonly int[] _base;

        public VoronoiPartition(IDistanceLookup distances, IEnumerable<int> terminals, IReadOnlyGraphEdgeSet edgeSet)
        {
            _edgeSet = edgeSet;
            _terminals = terminals.ToList();
            _base = new int[distances.CacheSize];
            _regionDict = new Dictionary<int, HashSet<int>>(_terminals.Count);

            CalcRegions(distances);
        }

        /// <summary>
        /// Calculates the Voronoi regions of each terminal with a Dijkstra-like algorithm
        /// with each terminal as a starting node.
        /// </summary>
        private void CalcRegions(IDistanceLookup distances)
        {
            var isTerminal = new bool[distances.CacheSize];
            foreach (var t in _terminals)
            {
                isTerminal[t] = true;
            }

            // Priority queue storing the unchecked edges. The priority is the shortest
            // found path to any terminal of the outside node (which goes over the inside node).
            var prioQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);
            // Currently shortest distance to any terminal for each non terminal.
            var pathDists = new uint[distances.CacheSize];
            // Saves the unchecked nodes. True once the node got dequeued.
            var connected = new bool[distances.CacheSize];
            // Initialize
            for (var i = 0; i < distances.CacheSize; i++)
            {
                if (isTerminal[i])
                {
                    // Terminals are added to queue. They are checked first.
                    prioQueue.Enqueue(new LinkedGraphEdge(i, i, 0));
                    // Nearest terminal of a terminal is itself (with distance 0).
                    _base[i] = i;
                    pathDists[i] = 0;
                    // Regions contain at least the terminal.
                    _regionDict.Add(i, i);
                }
                else
                {
                    _base[i] = -1;
                    // Non terminals are initialized as being far away.
                    pathDists[i] = uint.MaxValue;
                }
            }
            // Traverse the graph starting at the terminals.
            while (prioQueue.Count > 0)
            {
                // Dequeue the shortest edge until the outside node is unchecked.
                var k = prioQueue.Dequeue().Outside;
                if (connected[k]) continue;
                // The node is now checked.
                connected[k] = true;
                // The nearest terminal of k.
                var t = _base[k];

                foreach (var m in _edgeSet.NeighborsOf(k))
                {
                    // Find all unchecked neighbors m for which the path over the current node is shorter
                    // than the currently shortest distance from m to any terminal.
                    if (connected[m] || pathDists[m] <= pathDists[k] + distances[k, m]) continue;
                    // t -- k -- m is the shortest found path from m to any terminal.
                    // So t is the nearest terminal of m at this point.
                    pathDists[m] = pathDists[k] + distances[k, m];
                    prioQueue.Enqueue(new LinkedGraphEdge(k, m, pathDists[m]));
                    _base[m] = t;
                }
            }

            // Fill the actual regions.
            for (var i = 0; i < distances.CacheSize; i++)
            {
                _regionDict.Add(_base[i], i);
            }
        }

        /// <summary>
        /// Returns a nearest terminal of the given node.
        /// </summary>
        public int Base(int node)
        {
            return _base[node];
        }

        /// <summary>
        /// Calulate the edges leaving each region.
        /// (Edges with one node inside and one node outside the region of a terminal)
        /// </summary>
        private Dictionary<int, List<GraphEdge>> CalcLinks()
        {
            var links = new Dictionary<int, List<GraphEdge>>(_terminals.Count);
            foreach (var t in _terminals)
            {
                links[t] = (from i in _regionDict[t]
                                 from j in _edgeSet.NeighborsOf(i)
                                 where _base[j] != t
                                 select _edgeSet[i, j]).ToList();
            }
            return links;
        }

        /// <summary>
        /// Returns the edges leaving the Voronoi region of the given node.
        /// (Edges with one node inside and one node outside the region of the terminal)
        /// </summary>
        public IReadOnlyList<GraphEdge> Links(int terminal)
        {
            if (_linksDict == null)
            {
                _linksDict = CalcLinks();
            }
            return _linksDict[terminal];
        }
    }
}