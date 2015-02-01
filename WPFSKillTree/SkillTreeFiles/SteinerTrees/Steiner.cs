using System;
using System.Collections.Generic;
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
        ///  The search space is all SteinerSets. For a given SteinerSet, the
        ///  corresponding SteinerTree is found (greedy, in polynomial time).
        ///  Then, all target nodes are connected to their closest steiner point.
        ///  

        public void SkillHighlightedNodes(SkillTree tree)
        {
            /// Preprocessing:
            ///  - Contract "isolated" node groups.
            ///  - Find and collect potential steiner points.
            ///  - Contract current tree
            ///  - Build graph for DistanceLookup
            /// 
            /// Solution:
            ///  - Initialize population (comprised of SteinerSets)
            ///  - Repeat until optimum or time passed:
            ///     - Evaluate individuals
            ///         - Add target nodes to steiner points
            ///         - Generate MST of SteinerSet + target points,
            ///           thereby finding cost
            ///     - Build new population

            
            var targetNodes = tree.HighlightedNodes;
            var skilledNodes = tree.SkilledNodes;


            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool partlySkilled = false;
                List<SkillNode> adjacent = new List<SkillNode>();

                foreach (SkillNode node in ng.Nodes)
                {
                    if (skilledNodes.Contains(node.Id))
                        partlySkilled = true; // Don't contract this then.


                    foreach (SkillNode neighbor in node.Neighbor)
                    {
                        if (neighbor.SkillNodeGroup != ng)
                            adjacent.Add(neighbor);
                    }


                }

                // Contract?
                //TODO: CONTRACT
            }


            //foreach 



        }

        class Graph
        {
            List<Vertex> vertices;
        }

        class Vertex
        {
            ushort _id;
            public ushort Id { get { return _id; } }

            List<Vertex> neighbors;


            public Vertex()
            {
                neighbors = new List<Vertex>();
            }
        }

        class SteinerVertex : Vertex
        {
            public SteinerVertex()
            {
                ;
            }
        }

        class SteinerSet
        {
            List<SteinerVertex> steiners;

            public Graph ConstructSteinerTree()
            {
                throw new NotImplementedException("ConstructSteinerTree() not yet implemented!");
                return new Graph();
            }
        }

        // Summary:
        //  Calculates and caches distances between nodes
        class DistanceLookup
        {
            readonly Graph _graph;

            // The uint compounds both ushort indices.
            Dictionary<uint, int> _distances;


            public DistanceLookup(Graph graph)
            {
                _distances = new Dictionary<uint, int>();
                _graph = graph;
            }

            public int GetDistance(Vertex a, Vertex b)
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
