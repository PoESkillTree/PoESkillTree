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
        [TestMethod]
        public void TestDijkstra()
        {
            DistanceLookup distanceLookup = new DistanceLookup();

            // Test
            List<SkillNode> nodes = new List<SkillNode>();
            for (int i = 0; i < 3; i++)
            {
                SkillNode node = new SkillNode();
                node.Id = (ushort)i;
                nodes.Add(node);
            }

            nodes[0].Neighbor = new List<SkillNode>() { nodes[0], nodes[1] };
            // ...

            SearchGraph graph = new SearchGraph();
            foreach (SkillNode node in nodes)
            {
                graph.AddNode(node, false);
            }

            List<GraphNode> graphNodes = new List<GraphNode>(graph.nodeDict.Values);


            Assert.IsTrue(distanceLookup.GetDistance(graphNodes[0], graphNodes[0]) == 0, "Failed 0 distance test!");


        }

        [TestMethod]
        public void TestMST()
        {


        }
    }
}
