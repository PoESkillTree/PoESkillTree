using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    class Steiner
    {
        /// Data structures:
        ///  - SkillTree: A "real" skilltree. All edges have weight one.
        ///  - SteinerSet: A set of steiner points.
        ///  - SteinerTree: A minimal spanning tree of a SteinerSet. Edge weights
        ///    equal the node distance in the corresponding SkillTree.
        ///  - DistanceLookup: Calculates and caches distances between nodes.

        /// Algorithm:
        ///  The search space is all SteinerSets. A GA optimizes the solution
        ///  vectors (bitstrings encoding the SteinerSets).
        ///  The fitness function for a given SteinerSet is computed by:
        ///   - Finding the corresponding SteinerTree (greedy, in polynomial time).
        ///   - All target nodes are connected to their closest steiner point.
        ///   - The total nodes used are calculated and used as a fitness measure.
        ///  
        /// For the later goal of using weighted nodes (or even a tree-wide
        /// fitness function like "dps"), the search space would be extended to
        /// include the potential (weighted) target nodes.
        ///
        /// It would make sense to limit the amount of steiner points under
        /// consideration when the target nodes aren't spread out too much.

        List<Hypernode> potentialSteinerPoints;

        public void SkillHighlightedNodes(SkillTree tree)
        {
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            /// 
            /// Solution:
            ///  - Let the GA handle it.

            
            var targetNodes = tree.HighlightedNodes;

            var skilledNodes = tree.SkilledNodes;
            
            
            /// Contract node groups.
            //List<Hypernode> hypernodes;

            HypernodeGraph graph = new HypernodeGraph();

            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool partlySkilled = false;
                
                HashSet<SkillNode> adjacent = new HashSet<SkillNode>();

                foreach (SkillNode node in ng.Nodes)
                {
                    if (skilledNodes.Contains(node.Id))
                    {
                        // Don't contract this then.
                        partlySkilled = true;
                        break;
                    }

                    foreach (SkillNode neighbor in node.Neighbor)
                    {
                        if (neighbor.SkillNodeGroup != ng)
                            adjacent.Add(neighbor);
                    }
                }

                /// TODO: Contract to single node
                if ((adjacent.Count == 1) && (!partlySkilled))
                {
                    graph.AddHypernode(new Hypernode(new HashSet<SkillNode>(ng.Nodes)));
                }
            }

            // Add the skilled nodes as a single hypernode.
            graph.AddNodeIdSet(skilledNodes);

            foreach (ushort nodeId in targetNodes)
            {
                // Target node is already contained in a contracted node.
                if (graph.nodeIdToHypernodes.ContainsKey(nodeId)) continue;

                // Target node needs to be added to the graph.
                graph.AddNodeIdSet(new HashSet<ushort>() { nodeId });
            }

            // Find potential steiner points (> 2 neighbors)
            foreach (Hypernode hypernode in graph.hypernodes)
            {
                if (hypernode.neighborCount > 2)
                    potentialSteinerPoints.Add(hypernode);
            }


            potentialSteinerPoints = new List<Hypernode>();

            foreach (var node in targetNodes)
            {
                Hypernode hypernode = new Hypernode(new HashSet<SkillNode> { SkillTree.Skillnodes[node] });
                potentialSteinerPoints.Add(hypernode);
            }




            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction);

            ga.StartEvolution(100, potentialSteinerPoints.Count);


        }

        private double fitnessFunction(BitArray representation)
        {
            List<Hypernode> usedSteinerPoints = new List<Hypernode>();
            for (int i = 0; i < representation.Length; i++)
            {
                if (representation[i])
                    usedSteinerPoints.Add(potentialSteinerPoints[i]);
            }

            throw new NotImplementedException();
        }


        class HypernodeGraph
        {
            public List<Hypernode> hypernodes;
            public Dictionary<ushort, Hypernode> nodeIdToHypernodes;

            public HypernodeGraph()
            {
                hypernodes = new List<Hypernode>();
                nodeIdToHypernodes = new Dictionary<ushort, Hypernode>();
            }

            public void AddHypernode(Hypernode hypernode)
            {
                hypernodes.Add(hypernode);
                foreach (SkillNode node in hypernode.nodes)
                {
                    nodeIdToHypernodes.Add(node.Id, hypernode);
                }
            }

            public void AddNodeIdSet(HashSet<ushort> nodes)
            {
                HashSet<SkillNode> nodeSet = new HashSet<SkillNode>();
                foreach (ushort nodeId in nodes)
                {
                    nodeSet.Add(SkillTree.Skillnodes[nodeId]);
                }
                Hypernode hypernode = new Hypernode(nodeSet);
                AddHypernode(hypernode);
            }
        }

        class Hypernode
        {
            ushort _id;
            public ushort Id { get { return _id; } }

            public HashSet<SkillNode> nodes;

            List<Hypernode> neighbors;

            public int neighborCount;


            public Hypernode(HashSet<SkillNode> nodes)
            {
                neighbors = new List<Hypernode>();
                this.nodes = nodes;
            }
        }

        // Summary:
        //  Calculates and caches distances between nodes
        class DistanceLookup
        {
            readonly HypernodeGraph _graph;

            // The uint compounds both ushort indices.
            Dictionary<uint, int> _distances;


            public DistanceLookup(HypernodeGraph graph)
            {
                _distances = new Dictionary<uint, int>();
                _graph = graph;
            }

            public int GetDistance(Hypernode a, Hypernode b)
            {
                ushort aI = a.Id;
                ushort bI = b.Id;
                uint index = (uint)(Math.Max(aI, bI) + Math.Min(aI, bI) << 16);
                if (_distances.ContainsKey(index))
                    return _distances[index];
                
                //return tree.GetShortestPathTo()
                return 0;
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
