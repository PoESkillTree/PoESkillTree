using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using System.Collections.Generic;
using Priority_Queue;

//namespace UnitTests
namespace UnitTests
{
    [TestClass]
    public class TestMSTAlgorithm
    {
        // Builds a graph from an adjacency matrix.
        // Only the lower left half is checked.
        SearchGraph SearchGraphFromData(bool[,] adjacencyMatrix)
        {
            int n = adjacencyMatrix.GetUpperBound(0) + 1;
            // Don't screw this up.
            Assert.IsTrue(n == adjacencyMatrix.GetUpperBound(1) + 1);

            List<SkillNode> nodes = new List<SkillNode>();
            for (int i = 0; i < n; i++)
            {
                SkillNode node = new SkillNode();
                node.Id = (ushort)i;
                nodes.Add(node);
            }

            for (int i = 0; i < n; i++)
            {
                nodes[i].Neighbor = new List<SkillNode>();
                for (int j = 0; j < i; j++)
                {
                    if (adjacencyMatrix[i, j])
                    {
                        nodes[i].Neighbor.Add(nodes[j]);
                        // No directed edges atm.
                        nodes[j].Neighbor.Add(nodes[i]);
                    }
                }
            }

            SearchGraph graph = new SearchGraph();
            foreach (SkillNode node in nodes)
            {
                graph.AddNode(node);
            }
            return graph;
        }

        Dictionary<int, GraphNode> GetGraphNodesIdIndex(SearchGraph graph)
        {
            Dictionary<int, GraphNode> retval = new Dictionary<int, GraphNode>();
            foreach (GraphNode node in graph.nodeDict.Values)
            {
                retval.Add(node.Id, node);
            }
            return retval;
        }

        [TestMethod]
        public void TestDijkstra()
        {
            // TODO: Maybe make the graphs class members.
            /// 0 -- 1 -- 2 -- 3
            ///  \        |   /
            ///    \      | /
            ///      4 -- 5
            bool[,] graph1 = {
                                 { false, true,  false, false, true,  false },
                                 { true,  false, true,  false, false, false },
                                 { false, true,  false, true,  false, true  },
                                 { false, false, true,  false, false, true  },
                                 { true,  false, false, false, false, true  },
                                 { false, false, true,  true,  true,  false },
                             };

            SearchGraph searchGraph1 = SearchGraphFromData(graph1);

            Dictionary<int, GraphNode> graphNodes = GetGraphNodesIdIndex(searchGraph1);


            DistanceLookup distanceLookup = new DistanceLookup();
            Assert.IsTrue(distanceLookup.GetDistance(graphNodes[0], graphNodes[0]) == 0, "Failed 0 distance test");
            Assert.IsTrue(distanceLookup.GetDistance(graphNodes[0], graphNodes[5]) == 2, "Wrong distance");
            Assert.IsTrue(distanceLookup.GetDistance(graphNodes[0], graphNodes[3]) == 3, "Wrong distance");
        }

        [TestMethod]
        public void TestMSTNodeCounter()
        {
            Steiner testSteiner = new Steiner();

            /// 0 -- 1 -- 2 -- 3
            ///  \        |   /
            ///    \      | /
            ///      4 -- 5
            bool[,] graph1 = {
                                 { false, true,  false, false, true,  false },
                                 { true,  false, true,  false, false, false },
                                 { false, true,  false, true,  false, true  },
                                 { false, false, true,  false, false, true  },
                                 { true,  false, false, false, false, true  },
                                 { false, false, true,  true,  true,  false },
                             };

            SearchGraph searchGraph1 = SearchGraphFromData(graph1);
            Dictionary<int, GraphNode> graphNodes = GetGraphNodesIdIndex(searchGraph1);


        }


        [TestMethod]
        public void TestMST()
        {
            /// 0 -- 1 -- 2 -- 3
            ///  \        |   /
            ///    \      | /
            ///      4 -- 5
            bool[,] graph1 = {
                                 { false, true,  false, false, true,  false },
                                 { true,  false, true,  false, false, false },
                                 { false, true,  false, true,  false, true  },
                                 { false, false, true,  false, false, true  },
                                 { true,  false, false, false, false, true  },
                                 { false, false, true,  true,  true,  false },
                             };

            SearchGraph searchGraph1 = SearchGraphFromData(graph1);

            Dictionary<int, GraphNode> graphNodes1 = GetGraphNodesIdIndex(searchGraph1);
            DistanceLookup distanceLookup = new DistanceLookup();

            HashSet<GraphNode> mstNodes1 = new HashSet<GraphNode> { graphNodes1[2], graphNodes1[5] };
            MinimalSpanningTree mst1 = new MinimalSpanningTree(mstNodes1);
            mst1.Span(graphNodes1[0]);
            Assert.IsTrue(mst1.UsedNodeCount == 3, "Wrong MST length");


            /// Test unconnected graph

            /// 0 -- 1    2 -- 3
            ///           |   /
            ///           | /
            ///      4 -- 5
            bool[,] graph2 = {
                                 { false, true,  false, false, false, false },
                                 { true,  false, false, false, false, false },
                                 { false, false, false, true,  false, true  },
                                 { false, false, true,  false, false, true  },
                                 { false, false, false, false, false, true  },
                                 { false, false, true,  true,  true,  false },
                             };
            SearchGraph searchGraph2 = SearchGraphFromData(graph2);
            Dictionary<int, GraphNode> graphNodes2 = GetGraphNodesIdIndex(searchGraph2);
            HashSet<GraphNode> mstNodes2 = new HashSet<GraphNode> { graphNodes2[0], graphNodes2[2], graphNodes2[4] };

            bool pass = false;
            try
            {
                MinimalSpanningTree mst = new MinimalSpanningTree(mstNodes2);
                mst.Span(graphNodes2[3]);
            }
            catch (DistanceLookup.GraphNotConnectedException)
            {
                pass = true;
            }
            Assert.IsTrue(pass, "No exception thrown for disconnected graph");
        }

        [TestMethod]
        public void TestPriorityQueue()
        {

            int[] queueTestOrder = { 10, 3, 11, 6, -3, 17, 13, -6, 2, 8, -2, -8 };
            int nodeCount = 0;
            for (int i = 0; i < queueTestOrder.Length; i++)
                nodeCount = Math.Max(queueTestOrder[i] + 1, nodeCount);

            HeapPriorityQueue<TestNode> queue = new HeapPriorityQueue<TestNode>(nodeCount);


            TestNode[] testNodes = new TestNode[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                testNodes[i] = new TestNode();

            for (int i = 0; i < queueTestOrder.Length; i++)
            {
                int t = queueTestOrder[i];

                if (t > 0)
                    queue.Enqueue(testNodes[t], t);
                if (t < 0)
                    Assert.IsTrue(queue.Dequeue().Priority == -t);
            }
        }

        class TestNode : PriorityQueueNode
        {
        }
    }
}
