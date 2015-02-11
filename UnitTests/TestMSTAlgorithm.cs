using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using System.Collections.Generic;

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

            Dictionary<int, GraphNode> graphNodes = GetGraphNodesIdIndex(searchGraph1);


            // Test unconnected graph
            try
            {

            }
            catch (POESKillTree.SkillTreeFiles.SteinerTrees.MinimalSpanningTree.GraphNotConnectedException e)
            {

            }
        }
    }
}
