using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

[assembly: InternalsVisibleTo("UnitTests")]
namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    [assembly: InternalsVisibleTo("UnitTests")]
    public class Steiner
    {
        // TODO: Update explanation.
        /// 
        /// 
        /// Terminology:
        ///  - Skilltree: A "real" skilltree as used in PoE.
        ///  - Steiner point: A point that has more than two neighbors.
        ///  - Steiner set: A set of steiner points.
        ///  - Steiner tree: A minimal spanning tree of a Steiner set. Edge weights
        ///    equal the node distance in the corresponding SkillTree.
        ///  - DistanceLookup: Calculates and caches distances between nodes.

        /// Algorithm:
        ///  The search space is all steiner sets. A GA optimizes the solution
        ///  vectors (bitstrings encoding the steiner sets).
        ///  
        ///  The fitness function for a given steiner set is computed by finding
        ///  the minimal spanning tree (MST) of the contained points plus the
        ///  target nodes (in polynomial time with a greedy algorithm).
        ///  The sum of its edge weights is then used as a fitness measure.
        ///  Note that this can overestimate the cost of an encoded skill tree!
        ///  However, since 1) it never underestimates the cost and 2) every
        ///  optimal skilltree still has a same-cost representation in the search
        ///  space, we're fine: No encoded tree can have a better fitness than the
        ///  optimal one.
        ///  
        /// 
        /// For the later goal of using weighted nodes (or even a tree-wide
        /// fitness function like "dps"), the search space would be extended by
        /// including the (potential, weighted) target nodes.
        ///
        /// It would make sense to limit the amount of steiner points under
        /// consideration when the target nodes aren't spread out too much.
        /// 
        /// Also note that the actual associated skilltree isn't created at any
        /// point during the algorithm. 
        /// 

        /// A nodegroup is only contracted if
        ///  - It has exactly one adjacent node, and
        ///  - It does not contain any skilled nodes.


        // TODO: Decide what needs to be a member and what should be passed
        // around as parameters.


        SkillTree tree;

        SearchGraph searchGraph;

        List<GraphNode> searchSpaceBase;


        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        DistanceLookup distances = new DistanceLookup();

        public Steiner(SkillTree tree)
        {
            this.tree = tree;

        }

        public void InitializeSolver(HashSet<ushort> targets)
        {
            // TODO: Update comment
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            ///  
            buildSearchGraph(targets);

            buildSearchSpaceBase();
        }

        public void ASD()
        {
            MinimalSpanningTree mst = findBestMst();

            return SpannedMstToSkillnodes(mst);

        }
        /// <summary>
        ///  Preprocesses the SkillTree graph into a simplified graph that omits
        ///  any isolated node groups (single pass) and contracts all skilled nodes
        ///  into a single node.
        /// </summary>
        /// <param name="targets">A set of node IDs representing the target nodes.</param>
        /// <returns>A SearchGraph representing the simplified SkillTree</returns>
        void buildSearchGraph(HashSet<ushort> targets)
        {
            searchGraph = new SearchGraph();

            // Add the start nodes to the graph.
            startNodes = searchGraph.SetStartNodes(tree.SkilledNodes);

            targetNodes = new HashSet<GraphNode>();
            // Add the target nodes to the graph.
            foreach (ushort nodeId in targets)
            {
                // Don't add nodes that are already skilled.
                if (searchGraph.nodeDict.ContainsKey(SkillTree.Skillnodes[nodeId]))
                    continue;
                // Add target node to the graph.
                GraphNode node = searchGraph.AddNodeId(nodeId);
                targetNodes.Add(node);
            }


            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool mustInclude = false;

                SkillNode firstNeighbor = null;

                // Find out if this node group can be omitted.
                foreach (SkillNode node in ng.Nodes)
                {
                    /// If the group contains a skilled node or a target node,
                    /// it can't be omitted.
                    if (searchGraph.nodeDict.ContainsKey(node))
                    {
                        mustInclude = true;
                        break;
                    }

                    /// If the group is adjacent to more than one node, it must
                    /// also be fully included (since it's not isolated and could
                    /// be part of a path to other nodes).
                    foreach (SkillNode neighbor in node.Neighbor.Where(neighbor => neighbor.SkillNodeGroup != ng))
                    {
                        if (firstNeighbor == null)
                            firstNeighbor = neighbor;

                        // Does the group have more than one neighbor?
                        if (neighbor != firstNeighbor)
                        {
                            mustInclude = true;
                            break;
                        }
                    }
                    if (mustInclude) break;
                }

                if (mustInclude)
                {
                    // Add the group's nodes individually
                    foreach (SkillNode node in ng.Nodes)
                    {
                        /// Don't add nodes that are already in the graph (as
                        /// target or start nodes).
                        if (!searchGraph.nodeDict.ContainsKey(node))
                            searchGraph.AddNode(node);
                    }
                }
            }
        }
        /// <summary>
        ///  Finds the nodes in the search graph that can be potential steiner
        ///  nodes. Those form the search space base.
        /// </summary>
        /// <param name="searchGraph"></param>
        void buildSearchSpaceBase()
        {
            searchSpaceBase = new List<GraphNode>();
            // Find potential steiner points that are in reasonable vicinity.
            foreach (GraphNode node in searchGraph.nodeDict.Values)
            {
                // This can be a steiner node only if it has more than 2 neighbors.
                if (node.Adjacent.Count > 2)
                {
                    /// If every target node is closer to the start than to a certain
                    /// steiner node, that node can't be needed for the steiner tree.
                    bool add = false;
                    foreach (GraphNode targetNode in targetNodes)
                        if (distances.GetDistance(targetNode, node) < distances.GetDistance(targetNode, startNodes))
                            add = true;
                    if (add)
                        searchSpaceBase.Add(node);
                }
            }
            /* ONLY FOR WHEN NODES HAVE INDIVIDUAL WEIGHTS
             * foreach (ushort nodeId in targetSkillnodes)
            {
                searchSpaceBase.Add(SkillTree.Skillnodes[nodeId]);
            }*/
        }

        /// <summary>
        ///  Employs a genetic algorithm to search the search space (spanned by
        ///  the search space base) for a skill tree with lowest possible point
        ///  investment.
        /// </summary>
        /// <returns></returns>
        MinimalSpanningTree findBestMst(int? randomSeed = null)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            if (randomSeed == null) randomSeed = DateTime.Now.GetHashCode();
            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction, new Random(randomSeed.Value));

            Console.WriteLine("Search space dimension: " + searchSpaceBase.Count);
            ga.StartEvolution(populationSize: searchSpaceBase.Count, dnaLength: searchSpaceBase.Count);

            // TODO: Better termination criteria.
            
            while (ga.GenerationCount < 200)
            {
                ga.NewGeneration();
                Thread.Yield();
            }
            timer.Stop();
            Console.WriteLine("Optimization time: " + timer.ElapsedMilliseconds + " ms");

            BitArray bestDna = ga.BestDNA();
            MinimalSpanningTree mst = dnaToMst(bestDna);
            mst.Span(startNodes);

            return mst;
        }

        HashSet<ushort> SpannedMstToSkillnodes(MinimalSpanningTree mst)
        {
            HashSet<ushort> newSkilledNodes = new HashSet<ushort>();
            foreach (GraphEdge edge in mst.SpanningEdges)
            {
                ushort target = edge.outside.Id;

                HashSet<ushort> start;
                if (edge.inside is Supernode)
                    start = tree.SkilledNodes;
                else
                    start = new HashSet<ushort>() { edge.inside.Id };

                var path = tree.GetShortestPathTo(target, start);

                newSkilledNodes = new HashSet<ushort>(newSkilledNodes.Concat(path));
            }
            
            foreach (GraphNode steinerNode in mst.mstNodes)
                tree._nodeHighlighter.ToggleHighlightNode(SkillTree.Skillnodes[steinerNode.Id], NodeHighlighter.HighlightState.FromAttrib);

            tree.DrawHighlights(tree._nodeHighlighter);
            return newSkilledNodes;
        }

        MinimalSpanningTree dnaToMst(BitArray dna)
        {
            List<GraphNode> usedSteinerPoints = new List<GraphNode>();
            for (int i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    usedSteinerPoints.Add(searchSpaceBase[i]);
            }

            HashSet<GraphNode> mstNodes = new HashSet<GraphNode>(usedSteinerPoints);
            mstNodes.Add(startNodes);

            foreach (GraphNode targetNode in targetNodes)
            {
                mstNodes.Add(targetNode);
            }

            return new MinimalSpanningTree(mstNodes, distances);
        }

        double fitnessFunction(BitArray representation)
        {
            MinimalSpanningTree mst = dnaToMst(representation);

            // This is the bottleneck, quite obviously.
            mst.Span(startFrom: startNodes);

            int usedNodes = mst.UsedNodeCount;

            // TODO: Improve fitness function
            return 2000 - usedNodes;
        }
    }
}