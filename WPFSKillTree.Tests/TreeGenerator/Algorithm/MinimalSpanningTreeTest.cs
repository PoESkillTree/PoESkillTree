using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Algorithm.Model;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeGenerator.Algorithm
{
    [TestFixture]
    public class MinimalSpanningTreeTest
    {
        // Builds a graph from an adjacency matrix.
        // Only the lower left half is checked.
        private static SearchGraph SearchGraphFromData(bool[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetUpperBound(0) + 1;
            // Don't screw this up.
            Assert.IsTrue(n == adjacencyMatrix.GetUpperBound(1) + 1);

            List<PassiveNodeViewModel> nodes = new List<PassiveNodeViewModel>();
            for (ushort i = 0; i < n; i++)
            {
                var node = new PassiveNodeViewModel(i);
                nodes.Add(node);
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (adjacencyMatrix[i, j])
                    {
                        nodes[i].NeighborPassiveNodes[nodes[j].Id] = nodes[j];
                        // No directed edges atm.
                        nodes[j].NeighborPassiveNodes[nodes[i].Id] = nodes[i];
                    }
                }
            }

            SearchGraph graph = new SearchGraph();
            foreach (PassiveNodeViewModel node in nodes)
            {
                graph.AddNode(node);
            }
            return graph;
        }

        private static Dictionary<ushort, GraphNode> GetGraphNodesIdIndex(SearchGraph graph)
            => graph.NodeDict.Values.ToDictionary(n => n.Id);

        [Test]
        public void TestDijkstra()
        {
            // 0 -- 1 -- 2 -- 3
            //  \        |   /
            //    \      | /
            //      4 -- 5
            bool[,] graph =
            {
                { false, true, false, false, true, false },
                { true, false, true, false, false, false },
                { false, true, false, true, false, true },
                { false, false, true, false, false, true },
                { true, false, false, false, false, true },
                { false, false, true, true, true, false },
            };
            var searchGraph = SearchGraphFromData(graph);
            var graphNodes = GetGraphNodesIdIndex(searchGraph);

            var distanceLookup = new DistanceCalculator(graphNodes.Values.ToList());

            Assert.AreEqual((uint) 0, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[0].DistancesIndex],
                "Failed 0 distance test");
            Assert.AreEqual((uint) 2, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[5].DistancesIndex],
                "Wrong distance");
            Assert.AreEqual((uint) 3, distanceLookup[graphNodes[0].DistancesIndex, graphNodes[3].DistancesIndex],
                "Wrong distance");
        }

        [Test]
        public void DijkstraUnconnected()
        {
            // 0 -- 1    2 -- 3
            //           |   /
            //           | /
            //      4 -- 5
            bool[,] graph =
            {
                { false, true, false, false, false, false },
                { true, false, false, false, false, false },
                { false, false, false, true, false, true },
                { false, false, true, false, false, true },
                { false, false, false, false, false, true },
                { false, false, true, true, true, false },
            };
            var searchGraph = SearchGraphFromData(graph);
            var graphNodes = GetGraphNodesIdIndex(searchGraph);
            var mstNodes = new List<GraphNode> { graphNodes[0], graphNodes[2], graphNodes[4], graphNodes[3] };

            var distances = new DistanceCalculator(graphNodes.Values.ToArray());

            Assert.IsNull(distances.GetShortestPath(mstNodes[0].DistancesIndex, mstNodes[3].DistancesIndex));
        }

        [Test]
        public void TestMST()
        {
            // 0 -- 1 -- 2 -- 3
            //  \        |   /
            //    \      | /
            //      4 -- 5 -- 6 -- 7
            bool[,] graph =
            {
                { false, true, false, false, true, false, false, false },
                { true, false, true, false, false, false, false, false },
                { false, true, false, true, false, true, false, false },
                { false, false, true, false, false, true, false, false },
                { true, false, false, false, false, true, false, false },
                { false, false, true, true, true, false, true, false },
                { false, false, false, false, false, true, false, true },
                { false, false, false, false, false, false, true, false },
            };
            var searchGraph = SearchGraphFromData(graph);
            var graphNodes = GetGraphNodesIdIndex(searchGraph);
            var mstNodes = new List<GraphNode>
                { graphNodes[3], graphNodes[5], graphNodes[7], graphNodes[0] };
            var distances = new DistanceCalculator(mstNodes);

            var mst = new MinimalSpanningTree(mstNodes.Select(n => n.DistancesIndex).ToList(), distances.DistanceLookup);
            var spanningEdges = mst.Span(graphNodes[0].DistancesIndex);

            Assert.AreEqual(3, spanningEdges.Count, "Wrong amount of spanning edges");
            var goalEdges = new[]
            {
                new[] { 0, 5 }, new[] { 5, 3 }, new[] { 5, 7 }
            };
            foreach (var edge in goalEdges)
            {
                Assert.AreEqual(1,
                    spanningEdges.Select(
                        e => new Tuple<ushort, ushort>(distances.IndexToNode(e.Inside).Id,
                            distances.IndexToNode(e.Outside).Id)).Count(
                        t =>
                            (t.Item1 == edge[0] && t.Item2 == edge[1]) ||
                            (t.Item1 == edge[1] && t.Item2 == edge[0])),
                    "Edge " + edge + " not contained exactly once.");
            }
        }
    }
}