using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;

namespace UnitTests
{
    [TestClass]
    public class TestGeneticAlgorithmEasy
    {

        static SkillTree Tree;



        [ClassInitialize]
        public static void Initalize(TestContext testContext)
        {
            Tree = SkillTree.CreateSkillTree(() => { Debug.WriteLine("Download started"); }, (double dummy1, double dummy2) => { }, () => { Debug.WriteLine("Download finished"); });
        }

        [TestMethod]
        public void TestEasyCases()
        {
            SkillNode coldhearted = SkillTree.Skillnodes.Values.Where(n => n.Name == "Coldhearted Calculation").First();
            SkillNode voidBarrier = SkillTree.Skillnodes.Values.Where(n => n.Name == "Void Barrier").First();

            Tree._nodeHighlighter.ToggleHighlightNode(coldhearted, NodeHighlighter.HighlightState.FromNode);
            Tree._nodeHighlighter.ToggleHighlightNode(voidBarrier, NodeHighlighter.HighlightState.FromNode);
            Tree.Chartype = 6; // Shadow

            Tree.SkillAllHighlightedNodes();
            /// Obviously possible to break when tree changes, like most other tests.
            /// The correct value would be whatever is shown in the app + 1.
            Assert.IsTrue(Tree.SkilledNodes.Count == 14);


            Tree.Reset();
            Tree._nodeHighlighter.UnhighlightAllNodes(NodeHighlighter.HighlightState.All);

            // Test if the optimal tree for this also uses all steiner nodes.
            SkillNode dynamo = SkillTree.Skillnodes.Values.Where(n => n.Name == "Dynamo").First();
            SkillNode skittering = SkillTree.Skillnodes.Values.Where(n => n.Name == "Skittering Runes").First();
            SkillNode innerForce = SkillTree.Skillnodes.Values.Where(n => n.Name == "Inner Force").First();
            SkillNode equilibrium = SkillTree.Skillnodes.Values.Where(n => n.Name == "Elemental Equilibrium").First();

            /*Tree._nodeHighlighter.ToggleHighlightNode(dynamo, NodeHighlighter.HighlightState.FromNode);
            Tree._nodeHighlighter.ToggleHighlightNode(skittering, NodeHighlighter.HighlightState.FromNode);
            Tree._nodeHighlighter.ToggleHighlightNode(innerForce, NodeHighlighter.HighlightState.FromNode);
            Tree._nodeHighlighter.ToggleHighlightNode(equilibrium, NodeHighlighter.HighlightState.FromNode);*/
            HashSet<ushort> targetNodes = new HashSet<ushort>{ dynamo.Id, skittering.Id, innerForce.Id, equilibrium.Id };
            Tree.Chartype = 0; // Scion

            SteinerSolver steiner = new SteinerSolver(Tree);
            // FIXME: Fix test.
            //steiner.constructSearchSpace(steiner.buildSearchGraph(targetNodes));
            //steiner.findBestMst();
        }
    }
}
