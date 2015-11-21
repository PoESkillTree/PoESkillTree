using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public class VoronoiPartition
    {
        private readonly IReadOnlyGraphEdgeSet _edgeSet;

        private readonly List<int> _terminals;

        private readonly Dictionary<int, HashSet<int>> _regionDict;

        private Dictionary<int, List<GraphEdge>> _linksDict;

        private readonly int[] _base;

        public VoronoiPartition(IDistanceLookup distances, IEnumerable<int> terminals, IReadOnlyGraphEdgeSet edgeSet)
        {
            _edgeSet = edgeSet;
            _terminals = terminals.ToList();
            var isTerminal = new bool[distances.CacheSize];
            foreach (var t in _terminals)
            {
                isTerminal[t] = true;
            }
            _base = new int[distances.CacheSize];
            _regionDict = new Dictionary<int, HashSet<int>>(_terminals.Count);

            var prioQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);
            var pathDists = new uint[distances.CacheSize];
            var connected = new bool[distances.CacheSize];
            for (var i = 0; i < distances.CacheSize; i++)
            {
                if (isTerminal[i])
                {
                    prioQueue.Enqueue(new LinkedGraphEdge(i, i, 0));
                    _base[i] = i;
                    _regionDict.Add(i, i);
                    pathDists[i] = 0;
                }
                else
                {
                    _base[i] = -1;
                    pathDists[i] = uint.MaxValue;
                }
            }
            while (prioQueue.Count > 0)
            {
                var edge = prioQueue.Dequeue();
                var k = edge.Outside;
                if (connected[k]) continue;
                connected[k] = true;

                foreach (var m in edgeSet.NeighborsOf(k))
                {
                    if (connected[m] || pathDists[m] <= pathDists[k] + distances[k, m]) continue;
                    pathDists[m] = pathDists[k] + distances[k, m];
                    prioQueue.Enqueue(new LinkedGraphEdge(k, m, pathDists[m]));
                    _base[m] = _base[k];
                    _regionDict.Add(_base[k], m);
                }
            }
        }

        public IEnumerable<int> Region(int terminal)
        {
            return _regionDict[terminal];
        }

        public int Base(int node)
        {
            return _base[node];
        }

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