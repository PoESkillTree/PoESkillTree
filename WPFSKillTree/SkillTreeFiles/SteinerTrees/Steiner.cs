using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Priority_Queue;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    class Steiner
    {
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
        // around as local variable.
        List<GraphNode> searchSpaceBase;

        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        DistanceLookup distances;


        public HashSet<ushort> ConnectNodes(SkillTree tree, HashSet<ushort> targets)
        {
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            
            

            SearchGraph searchGraph = new SearchGraph();
            distances = new DistanceLookup(searchGraph);

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
                    searchSpaceBase.Add(node);
            }


            /* ONLY FOR THE OTHER THING
             * foreach (ushort nodeId in targetSkillnodes)
            {
                searchSpaceBase.Add(SkillTree.Skillnodes[nodeId]);
            }*/




            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction);

            ga.StartEvolution(100, searchSpaceBase.Count);

            // TODO: Better termination criteria.
            while (ga.GenerationCount < 1000)
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

                newSkilledNodes = (HashSet<ushort>)newSkilledNodes.Concat(path);
            }

            
            return newSkilledNodes;
        }

        private List<GraphEdge> dnaToMst(BitArray dna)
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

            return minimalSpanningTree(mstNodes);
        }

        private int nodeCountUsedByMst(List<GraphEdge> mst)
        {
            int count = 0;
            // TODO: Check for off-by-one.
            foreach (GraphEdge edge in mst)
            {
                count += distances.GetDistance(edge);
            }
            return count;
        }

        private double fitnessFunction(BitArray representation)
        {
            int usedNodes = nodeCountUsedByMst(dnaToMst(representation));

            // TODO: Better cost function.
            return 1.0 / usedNodes;
        }

        private List<GraphEdge> minimalSpanningTree(HashSet<GraphNode> mstNodes)
        {
            // We will have at most one adjacent edge to each node.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);

            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(startNodes);
            toAdd.Remove(startNodes);

            foreach (GraphNode otherNode in toAdd)
            {
                GraphEdge adjacentEdge = new GraphEdge(startNodes, otherNode);
                adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
            }

            while (adjacentEdgeQueue.Count > 0)
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

        class GraphEdge : PriorityQueueNode
        {
            public GraphNode inside, outside;

            public GraphEdge(GraphNode inside, GraphNode outside)
            {
                this.inside = inside;
                this.outside = outside;
            }

        }
        
        /// <summary>
        ///  Abstract class representing a node (or a collection thereof) in the
        ///  simplified skill tree.
        /// </summary>
        abstract class GraphNode
        {
            protected ushort id;
            public ushort Id { get { return id; } }

            public bool isTarget;

            public HashSet<GraphNode> Adjacent = new HashSet<GraphNode>();
        }

        /// <summary>
        ///  A graph node representing an actual node in the skill tree.
        /// </summary>
        class SingleNode : GraphNode
        {
            SkillNode baseNode;

            public SingleNode(SkillNode baseNode, bool isTarget = false)
            {
                this.baseNode = baseNode;
                this.id = baseNode.Id;
                this.isTarget = isTarget;
            }
        }

        /// <summary>
        ///  A graph node representing a collection of nodes of the skill tree.
        ///  This is used to group up the already skilled nodes.
        /// </summary>
        class Supernode : GraphNode
        {
            public HashSet<SkillNode> nodes = new HashSet<SkillNode>();

            public Supernode(HashSet<ushort> nodes)
            {
                foreach (ushort nodeId in nodes)
                {
                    this.nodes.Add(SkillTree.Skillnodes[nodeId]);
                }
                // For lack of a better way.
                this.id = this.nodes.First().Id;
            }
        }

        /// <summary>
        /// A graph representing a simplified skill tree.
        /// </summary>
        class SearchGraph
        {
            public Supernode startNodes;

            public Dictionary<SkillNode, GraphNode> nodeDict;

            public SearchGraph()
            {
                nodeDict = new Dictionary<SkillNode, GraphNode>();
            }

            /// <summary>
            ///  Adds a skill node to the graph. New nodes are automatically
            ///  connected to existing nodes.
            /// </summary>
            /// <param name="node">The skill node to be added.</param>
            /// <param name="isTarget">Whether or not this node is a target
            /// node.</param>
            /// <returns>The graph node that is added to the graph.</returns>
            public GraphNode AddNode(SkillNode node, bool isTarget)
            {
                SingleNode graphNode = new SingleNode(node, isTarget);
                nodeDict.Add(node, graphNode);
                CheckLinks(node);
                return graphNode;
            }

            public GraphNode AddNodeId(ushort nodeId, bool isTarget)
            {
                return AddNode(SkillTree.Skillnodes[nodeId], isTarget);
            }

            public Supernode SetStartNodes(HashSet<ushort> startNodes)
            {
                Supernode supernode = new Supernode(startNodes);
                foreach (ushort nodeId in startNodes)
                {
                    SkillNode node = SkillTree.Skillnodes[nodeId];
                    nodeDict.Add(node, supernode);
                    CheckLinks(node);
                }
                return supernode;
            }

            private void CheckLinks(SkillNode node)
            {
                if (!nodeDict.ContainsKey(node)) return;
                GraphNode currentNode = nodeDict[node];

                foreach (SkillNode neighbor in node.Neighbor)
                {
                    if (nodeDict.ContainsKey(neighbor))
                    {
                        GraphNode adjacentNode = nodeDict[neighbor];

                        if (adjacentNode == currentNode) continue;

                        adjacentNode.Adjacent.Add(currentNode);
                        currentNode.Adjacent.Add(adjacentNode);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates and caches distances between nodes
        /// </summary>
        class DistanceLookup
        {
            readonly SearchGraph _graph;

            // The uint compounds both ushort indices.
            Dictionary<uint, int> _distances;


            public DistanceLookup(SearchGraph graph)
            {
                _distances = new Dictionary<uint, int>();
                _graph = graph;
            }

            public int GetDistance(GraphEdge edge)
            {
                return GetDistance(edge.outside, edge.inside);
            }

            public int GetDistance(GraphNode a, GraphNode b)
            {
                uint index = getIndex(a, b);

                // If we already calculated the shortest path, use that.
                if (_distances.ContainsKey(index))
                    return _distances[index];
                
                // Otherwise, use pathfinding to find it...
                int pathLength = Dijkstra(a, b);

                //... and save it...
                _distances.Add(index, pathLength);

                // ...and return it.
                return pathLength;
            }

            private void setDistance(GraphNode a, GraphNode b, int distance)
            {
                uint index = getIndex(a, b);
                if (!_distances.ContainsKey(index))
                    _distances.Add(index, distance);
            }

            /// <summary>
            ///  Compounds two ushort node indices into a single uint one, which
            ///  is independent of the order of the two indices.
            /// </summary>
            /// <param name="a">The first index.</param>
            /// <param name="b">The second index.</param>
            /// <returns>The compounded index.</returns>
            private uint getIndex(GraphNode a, GraphNode b)
            {
                ushort aI = a.Id;
                ushort bI = b.Id;
                return (uint)(Math.Max(aI, bI) + Math.Min(aI, bI) << 16);
            }

            /// <summary>
            ///  Uses a djikstra-like algorithm to flood the graph from the start
            ///  node until the target node is found.
            ///  All visited nodes have their distance from the start node updated.
            /// </summary>
            /// <param name="start">The starting node.</param>
            /// <param name="target">The target node.</param>
            /// <returns>The distance from the start node to the target node.</returns>
            public int Dijkstra(GraphNode start, GraphNode target)
            {
                if (start == target) return 0;
                return dijkstraStep(start, target, new HashSet<GraphNode>() { start },
                    new HashSet<GraphNode>(), 0);
            }


            /// <summary>
            ///  Uses a djikstra-like algorithm to flood the graph from the start
            ///  node until the target node is found.
            ///  All visited nodes have their distance from the start node updated.
            /// </summary>
            /// <param name="start">The starting node.</param>
            /// <param name="target">The target node.</param>
            /// <param name="front">The last newly found nodes.</param>
            /// <param name="visited">The already visited nodes.</param>
            /// <param name="distFromStart">The traversed distance from the
            /// starting node in edges.</param>
            /// <returns>The distance from the start node to the target node.</returns>
            /// <remarks> - Currently the target node is never found if contained
            /// in front or visited.
            ///  - If front = { start }, then distFromStart should be 0.</remarks>
            public int dijkstraStep(GraphNode start, GraphNode target,
                HashSet<GraphNode> front, HashSet<GraphNode> visited, int distFromStart)
            {
                HashSet<GraphNode> newFront = new HashSet<GraphNode>();
                // Nodes that 
                HashSet<GraphNode> newVisited = new HashSet<GraphNode>(visited);
                newVisited.Concat(front);

                foreach (GraphNode node in front)
                {
                    newVisited.Add(node);
                    foreach (GraphNode adjacentNode in node.Adjacent)
                    {
                        // TODO: Off-by-one?
                        if (adjacentNode == target) return distFromStart + 1;

                        // Could be combined in newVisited...
                        if (visited.Contains(adjacentNode)) continue;
                        if (front.Contains(adjacentNode)) continue;

                        newFront.Add(adjacentNode);
                        // This must be the shortest path from start to this node.
                        setDistance(start, adjacentNode, distFromStart + 1);
                    }
                }
                // This wouldn't need recursion, but it's more convenient this way.
                return dijkstraStep(start, target, newFront, newVisited, distFromStart + 1) + 1;
            }
        }
    }
}


/*class ContractedVertex : Hypernode
{
    public SkillNodeGroup original;
    public SkillNode adjacent;

    public ContractedVertex(SkillNodeGroup original, SkillNode adjacent)
    {
        this.original = original;
        this.adjacent = adjacent;
        neighborCount = (adjacent != null ? 1 : 0);
    }
}

class TargetVertex : Hypernode
{
    public SkillNode original;

    public TargetVertex(SkillNode original)
    {
        this.original = original;
    }
}

class TreeVertex : Hypernode
{
    HashSet<ushort> skilledNodes;

    public TreeVertex(HashSet<ushort> skilledNodes)
    {
        this.skilledNodes = skilledNodes;
    }
}
class SteinerVertex : Hypernode
{
    public SteinerVertex()
    {
        ;
    }
}*/

/*class SteinerSet
{
    List<SteinerVertex> steiners;

    public Graph ConstructSteinerTree()
    {
        throw new NotImplementedException("ConstructSteinerTree() not yet implemented!");
        return new Graph();
    }
}*/

/*

        class Hypernode
        {
            ushort _id;
            public ushort Id { get { return _id; } }

            public HashSet<SkillNode> nodes;

            public List<Hypernode> neighbors;


            public Hypernode(HashSet<SkillNode> nodes)
            {
                neighbors = new List<Hypernode>();
                this.nodes = nodes;
                _id = nodes.First().Id;
            }
        }*/