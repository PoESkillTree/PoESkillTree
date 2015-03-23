using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;

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
        List<GraphNode> searchSpaceBase;

        SkillTree tree;


        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        DistanceLookup distances;

        public Steiner(SkillTree tree)
        {
            this.tree = tree;
        }

        public HashSet<ushort> ConnectNodes(HashSet<ushort> targets)
        {
            // TODO: Update comment
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            ///  
            SearchGraph searchGraph = buildSearchGraph(targets);

            distances = new DistanceLookup();

            constructSearchSpace(searchGraph);

            MinimalSpanningTree mst = findBestMst();

            return SpannedMstToSkillnodes(mst);

        }

        // Tired of kludging things for unit tests, let it be internal.
        internal SearchGraph buildSearchGraph(HashSet<ushort> targets)
        {
            SearchGraph searchGraph = new SearchGraph();

            startNodes = searchGraph.SetStartNodes(tree.SkilledNodes);

            targetNodes = new HashSet<GraphNode>();
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
                    /// also be fully included (since it's not isolated).
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
                    // Add nodes individually
                    foreach (SkillNode node in ng.Nodes)
                    {
                        // Don't add nodes that are already in the graph.
                        if (!searchGraph.nodeDict.ContainsKey(node))
                            searchGraph.AddNode(node);
                    }
                }
            }

            return searchGraph;
        }

        internal void constructSearchSpace(SearchGraph searchGraph)
        {
            searchSpaceBase = new List<GraphNode>();
            // Find potential steiner points that are in reasonable vicinity.
            foreach (GraphNode node in searchGraph.nodeDict.Values)
            {
                // This can be a steiner node only if it has more than 2 neighbors.
                if (node.Adjacent.Count > 2)
                {
                    bool add = false;

                    /// If every target node is closer to the start than to a certain
                    /// steiner node, that node can't be needed for the steiner tree.
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


        internal MinimalSpanningTree findBestMst()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction, new Random(DateTime.Now.GetHashCode()));

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