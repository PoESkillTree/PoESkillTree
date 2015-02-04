using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

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


        List<SkillNode> potentialSteinerPoints;


        List<SkillNode> targetNodes = new List<SkillNode>();


        public void SkillHighlightedNodes(SkillTree tree)
        {
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLooku
            
            

            SearchGraph searchGraph = new SearchGraph();

            // Add the skilled nodes as a single hypernode.
            var skilledNodes = tree.SkilledNodes;
            searchGraph.AddNodeIdSet(skilledNodes);


            // Add all target nodes to the graph individdually
            var targetSkillnodes = tree.HighlightedNodes;
            foreach (ushort nodeId in targetSkillnodes)
            {
                // Add target node to the graph.
                searchGraph.AddNodeIdSet(new HashSet<ushort>() { nodeId });
            }


            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool includeNodes = true;
                
                HashSet<SkillNode> adjacent = new HashSet<SkillNode>();

                foreach (SkillNode node in ng.Nodes)
                {
                    // Don't 
                    if (searchGraph.relevantNodes.Contains(node))
                    {
                        includeNodes = false;
                        break;
                    }

                    foreach (SkillNode neighbor in node.Neighbor)
                    {
                        if (neighbor.SkillNodeGroup != ng)
                            adjacent.Add(neighbor);
                    }
                }

                // Can be contracted?
                if ((adjacent.Count == 1) && (!includeNodes))
                {
                    // Add nodes contraced
                    //contractedGraph.AddHypernode(new Hypernode(new HashSet<SkillNode>(ng.Nodes)));
                }
                else
                {
                    // Add nodes individually
                    foreach (SkillNode node in ng.Nodes)
                    {
                        searchGraph.AddNodeIdSet(new HashSet<ushort>() { node.Id });
                    }
                }
            }


            potentialSteinerPoints = new List<SkillNode>();

            // Find potential steiner points (> 2 neighbors)
            foreach (SkillNode node in searchGraph.relevantNodes)
            {
                if (node.Neighbor.Count > 2)
                    potentialSteinerPoints.Add(node);
            }

            foreach (ushort nodeId in targetSkillnodes)
            {
                potentialSteinerPoints.Add(SkillTree.Skillnodes[nodeId]);
            }




            GeneticAlgorithm ga = new GeneticAlgorithm(fitnessFunction);

            ga.StartEvolution(100, potentialSteinerPoints.Count);


        }

        private double fitnessFunction(BitArray representation)
        {
            List<SkillNode> usedSteinerPoints = new List<SkillNode>();
            for (int i = 0; i < representation.Length; i++)
            {
                if (representation[i])
                    usedSteinerPoints.Add(potentialSteinerPoints[i]);
            }


            List<SkillNode> mstNodes = usedSteinerPoints;
            mstNodes.AddRange(targetNodes);

            // TODO: MST of the points




            throw new NotImplementedException();
        }


        class SteinerNode : SkillNode
        {

        }


        class SearchGraph
        {
            public List<SkillNode> relevantNodes;

            public HashSet<SkillNode> startNodes;

            public SearchGraph()
            {
                relevantNodes = new List<SkillNode>();
            }

            public void AddHypernode(SkillNode hypernode)
            {
                relevantNodes.Add(hypernode);
            }

            public void AddNodeIdSet(HashSet<ushort> nodes)
            {
                HashSet<SkillNode> nodeSet = new HashSet<SkillNode>();
                foreach (ushort nodeId in nodes)
                {
                    relevantNodes.Add(SkillTree.Skillnodes[nodeId]);
                }
            }

            public void LinkNodes()
            {
                foreach (SkillNode node in relevantNodes)
                {

                }
            }
        }

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
        }

        // Summary:
        //  Calculates and caches distances between nodes
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
