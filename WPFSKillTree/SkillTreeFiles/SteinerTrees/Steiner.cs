using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Priority_Queue;
using System.Runtime.CompilerServices;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class Steiner
    {
        // TODO: Update explanation.
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

        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        DistanceLookup distances;

        public HashSet<ushort> ConnectNodes(SkillTree tree, HashSet<ushort> targets)
        {
            // TODO: Update comment
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            
            

            SearchGraph searchGraph = new SearchGraph();
            distances = new DistanceLookup();

            var skilledNodes = tree.SkilledNodes;
            startNodes = searchGraph.SetStartNodes(skilledNodes);

            targetNodes = new HashSet<GraphNode>();
            foreach (ushort nodeId in targets)
            {
                // Add target node to the graph.
                GraphNode node = searchGraph.AddNodeId(nodeId, true);
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
                            searchGraph.AddNode(node, false);
                    }
                }
            }

            ///
            /// At this point, the graph is fully built.
            /// 

            searchSpaceBase = new List<GraphNode>();

            // Find potential steiner points (> 2 neighbors)
            foreach (GraphNode node in searchGraph.nodeDict.Values)
            {
                // This can be a steiner node.
                if (node.Adjacent.Count > 2)
                   // if (distances.GetDistance(startNodes, node) )
                    searchSpaceBase.Add(node);
            }


            /* ONLY FOR THE OTHER THING
             * foreach (ushort nodeId in targetSkillnodes)
            {
                searchSpaceBase.Add(SkillTree.Skillnodes[nodeId]);
            }*/


            //distances.



            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction);

            ga.StartEvolution(5, searchSpaceBase.Count);

            // TODO: Better termination criteria.
            while (ga.GenerationCount < 100)
            {
                ga.NewGeneration();
            }

            BitArray bestDna = ga.BestDNA();
            List<GraphEdge> mst = dnaToMst(bestDna);

            HashSet<ushort> newSkilledNodes = new HashSet<ushort>();

            foreach (GraphEdge edge in mst)
            {
                ushort target = edge.outside.Id;

                HashSet<ushort> start;
                if (edge.inside is Supernode)
                    start = skilledNodes;
                else
                    start = new HashSet<ushort>() { edge.inside.Id };

                var path = tree.GetShortestPathTo(target, start);

                newSkilledNodes = new HashSet<ushort>(newSkilledNodes.Concat(path));
            }

            
            return newSkilledNodes;
        }

        List<GraphEdge> dnaToMst(BitArray dna)
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

            return minimalSpanningTree(mstNodes, startNodes);
        }

        int nodeCountUsedByMst(List<GraphEdge> mst)
        {
            int count = 0;
            // TODO: Check for off-by-one.
            foreach (GraphEdge edge in mst)
            {
                count += distances.GetDistance(edge);
            }
            return count;
        }

        double fitnessFunction(BitArray representation)
        {
            int usedNodes = nodeCountUsedByMst(dnaToMst(representation));

            // TODO: Better cost function.
            return 1.0 / usedNodes;
        }

        List<GraphEdge> minimalSpanningTree(HashSet<GraphNode> mstNodes, GraphNode start)
        {
            // We will have at most one adjacent edge to each node.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);

            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(start);
            toAdd.Remove(start);

            foreach (GraphNode otherNode in toAdd)
            {
                GraphEdge adjacentEdge = new GraphEdge(start, otherNode);
                // Priority is set to negative distance.
                adjacentEdgeQueue.Enqueue(adjacentEdge, -distances.GetDistance(adjacentEdge));
            }

            // TODO: Fails if the graph is not connected
            while (toAdd.Count > 0)
            {
                GraphEdge shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                GraphNode newIn = shortestEdge.outside;

                mstNodes.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                // TODO: This will break because of "collection modified".
                foreach (GraphEdge obsoleteEdge in adjacentEdgeQueue.Where(e => e.outside == newIn))
                {
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }

                // Find all newly adjacent edges and enqueue them.
                foreach (GraphNode otherNode in toAdd)
                {
                    GraphEdge adjacentEdge = new GraphEdge(startNodes, otherNode);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                }
            }

            return mstEdges;
        }
    }
}