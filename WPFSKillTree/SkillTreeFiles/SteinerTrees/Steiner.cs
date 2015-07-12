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
    /// <summary>
    ///  A class controlling the interaction between the skill tree data and the
    ///  genetic algorithm used to find (hopefully) optimal solutions.
    ///  Please see the code documentation inside the class for more information.
    /// </summary>
    public class SteinerSolver
    {
        ///////////////////////////////////////////////////////////////////////////
        /// This code is a heuristic solution to the Steiner tree problem (STP).
        /// (The reader's knowledge of the STP and commonly associated terms is
        /// assumed.)
        /// 
        /// The main code logic is found in these classes:
        ///  - SteinerSolver:
        ///         Handles all model knowledge and converts between the SkillNode
        ///         and preprocessed GraphNode tree versions. Also defines the fit-
        ///         ness function used in the genetic algorithm.
        ///  - GeneticAlgorithm:
        ///         The actual optimization "engine". It searches an n-dimensional
        ///         binary search space (bitstrings of length n) for an argument
        ///         that maximizes the value of the provided fitness function.
        /// 
        /// 
        /// Model and solution idea:
        ///   Every steiner tree can be described by its used steiner nodes (plus
        ///   the set of target nodes). Conversion back to a tree is possible by
        ///   building the minimal spanning tree (MST) of those (combined) point
        ///   sets. (The resulting tree may be different, but will have the same
        ///   total weight, which is all we care about).
        ///   Since the problem is to find a minimum weight steiner tree with fixed
        ///   target nodes, the search can be restricted to the space of all pos-
        ///   sible steiner node sets: A steiner node is either included or not in-
        ///   cluded in a particular solution, the search space is thus binary with
        ///   a dimension equal to the amount of potential steiner nodes under 
        ///   consideration.
        ///   
        ///   The actual implementation preprocesses the skill tree graph into an
        ///   alternate representation in order to reduce the search space size
        ///   (and improve pathfinding speed).
        ///   To clarify: "skill node" and "skill tree" will refer to the represen-
        ///   tation used in the main view (SkillNode, SkillTree), while "graph
        ///   nodes" and "graph" will refer to this processed, reduced version that
        ///   is used in most parts of this code (GraphNode, SearchGraph & similar).
        /// 
        /// 
        /// Description of the main algorithm:
        ///  0. The input received is the set of target points that shall be reached
        ///     by a connected graph, a subset of the skill tree net (as found by
        ///     examining the static SkillNodes dictionary in SkillTree). A SkillTree
        ///     instance is also passed to find the currently skilled nodes (and for
        ///     debugging convenience).
        ///     
        ///  1. A search graph is built, which simplifies the skill tree net in two
        ///     ways:
        ///         - All currently skilled nodes are contracted to a single
        ///           GraphNode.
        ///         - Any clusters that are adjacent to only one node (e.g. Fingers
        ///           of Frost) are omitted, unless they contain target or skilled
        ///           nodes.
        ///     Unless otherwise specified, "node" refers to GraphNode instances
        ///     from here on.
        ///     
        ///  2. Potential steiner nodes are determined. Only nodes with three or
        ///     more neighbors qualify for this.
        ///     In addition, nodes too far away from the start or target nodes also
        ///     do not qualify. The precise criteria for this are not exactly clear
        ///     yet, but the current approach seems to work.
        /// 
        /// Steps 1 and 2 greatly reduce the dimension of the search space, which
        /// enables an optimal (or near-optimal) solution to be found within quite
        /// short time.
        ///  
        ///  3. A genetic algorithm is employed to find the maximum of the fitness
        ///     function over the search space (see "Model and solution idea"). Its
        ///     inner workings won't be elaborated here, it is to note though that
        ///     scaling population size and maximum generation (as termination cri-
        ///     terium) linearly with the search space dimensions seems to ensure
        ///     finding optimal results, based on the tests so far.
        ///     Provided to the GA are only the search space dimension and the fit-
        ///     ness function, which will subsequently be discussed.
        ///     
        /// Fitness function:
        ///     In order to save computation time, not every bitstring (or DNA)
        ///     that the GA requests to be evaluated is converted back to an actual
        ///     skill tree. Instead, an MST on the steiner node set represented by
        ///     the DNA is built, based on the steiner node - steiner node distance
        ///     values and shortest paths (which are cached and thus inexpensive to
        ///     access).
        ///     
        ///     Since the claim of this solver is to actually provide optimal solu-
        ///     tions in as many cases as possible (anything that is "only close"
        ///     could easily be done by the user himself), while the fitness func-
        ///     tion is "nicely" shaped (since the skill tree is, well, quite
        ///     structured), trading usability outside the optimum for speed is a
        ///     reasonable approach.
        ///     
        ///     Obviously, the evaluation of the fitness function is the bottleneck
        ///     for the computation speed, so the use of a fast MST algorithm is
        ///     preferred. Refer to the MinimalSpanningTree class.
        /// 
        ///  4. The resulting high-fitness DNA is converted back to a steiner node
        ///     set, the target nodes and start node are added and the MST is built
        ///     (all of this is similarly part of the fitness function).
        ///     Since the result of the MST algorithm is a set of edges between
        ///     (graph) nodes, what is left is finding shortest paths for those
        ///     edges between the equivalent skill nodes of the graph nodes. These
        ///     paths are also cached in DistanceLookup.
        ///     
        /// Step 4 is performed after every generation of the GA (for giving the
        /// user visual feedback about the optimization progress), as well as after
        /// the termination of the GA optimization.
        /// The conversion back to the skill tree completes an optimization run.
        /// 
        ///
        /// Example 1: Necessity of steiner nodes
        /// Highlight Void Barrier and Coldhearted Calculation as a shadow without
        /// any skilled nodes.
        /// Observe how the optimal tree involves branching at a particular dex
        /// node, which will not be introduced by a greedy algorithm: The edges of
        /// the triangle formed by the two nodes and the shadow start all have dif-
        /// ferent lengths, and start - void barrier is the longest one. Taking the
        /// shortest path from start to Coldhearted Calculation and then taking the
        /// shortest path to Void Barrier results in a tree with 1 more point spent.

        SkillTree tree;

        SearchGraph searchGraph;
        DistanceLookup distances = new DistanceLookup();
        List<GraphNode> searchSpaceBase;

        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        GeneticAlgorithm ga;

        //  The base modifier for population is higher than generation because a far higher
        // population than maxGeneration gives much better results from the testing I've done.
        //  Also the duration is higher for lower dimensions so duration * population is
        // the same for each searchSpace < 150. From testing lower dimension need more duration
        // to produce good results, so this acomplishes it.
        /// <summary>
        ///  Returns the maxGeneration for initializeGA depending on the searchSpace size.
        ///  Returns 0.3 * 20000/searchSpace if searchSpace is < 150 and 0.3 * searchSpace
        ///  otherwise. This means duration * population is constant for searchSpace < 150.
        ///  If searchSpace is < 10 (and not 0) it is treated as being 10. This stops the value from
        ///  becoming to high for very low searchSpaces.
        /// </summary>
        private Func<int, double> durationFct = (searchSpace) =>
            (0.3 * (searchSpace < 150 && searchSpace > 0 ? 20000.0 / Math.Max(searchSpace, 10) : searchSpace));
        /// <summary>
        ///  Returns the populationSize for initializeGA depending on
        ///  the searchSpace size. Returns 1.5 * searchSpace at the moment.
        /// </summary>
        private Func<int, double> populationFct = (searchSpace) =>
            1.5 * searchSpace;

        double durationModifier;
        double populationModifier;

        private bool _initialized = false;
        public bool IsInitialized
        { get { return _initialized; } }

        private BitArray _bestDNA;
        public HashSet<ushort> BestSolution;

        // This is kinda crude but should work...
        public bool IsConsideredDone
        { get { return (_initialized ? CurrentGeneration >= MaxGeneration : false); } }

        private int maxGeneration;
        public int MaxGeneration
        { get { return (_initialized ? maxGeneration : 0); } }

        public int CurrentGeneration
        { get { return (_initialized ? ga.GenerationCount : 0); } }

        /// <summary>
        ///  A new instance of the SteinerSolver optimizer that still needs to be
        ///  initialized.
        /// </summary>
        /// <param name="tree">The skill tree in which to optimize.</param>
        public SteinerSolver(SkillTree tree)
        {
            this.tree = tree;
        }

        /// <summary>
        ///  Initializes the solver so that the optimization can be run.
        /// </summary>
        /// <param name="targets">The set of target nodes that shall be connected.</param>
        /// <param name="toOmit">A set of node IDs representing the nodes that should not
        /// be skilled.</param>
        /// <param name="durationModifier">A multiplier to the amount of iterations
        /// to go through (default: 1.0).</param>
        /// <param name="populationModifier">A multiplier to the size of the
        /// algorithm's solution pool (default: 1.0).</param>
        /// <exception cref="InvalidOperationException">If the nodes to be omitted
        /// disconnect the skill tree.</exception>
        public void InitializeSolver(HashSet<ushort> targets, HashSet<ushort> toOmit = null,
            double durationModifier = 1.0, double populationModifier = 1.0)
        {
            // (This is not in the constructor since it might take a moment.)
            this.durationModifier = durationModifier;
            this.populationModifier = populationModifier;

            if (toOmit == null)
            {
                toOmit = new HashSet<ushort>();
            }

            buildSearchGraph(targets, toOmit);

            try
            {
                buildSearchSpaceBase();
            }
            catch (DistanceLookup.GraphNotConnectedException e)
            {
                throw new InvalidOperationException("The graph is disconnected, probably because of the nodes to be omitted.", e);
            }

            initializeGA();

            _initialized = true;
        }

        /// <summary>
        ///  Preprocesses the SkillTree graph into a simplified graph that omits
        ///  any isolated node groups (single pass) and contracts all skilled nodes
        ///  into a single node.
        /// </summary>
        /// <param name="targets">A set of node IDs representing the target nodes.</param>
        /// <param name="toOmit">A set of node IDs representing the nodes that should not
        /// be skilled.</param>
        /// <returns>A SearchGraph representing the simplified SkillTree</returns>
        private void buildSearchGraph(HashSet<ushort> targets, HashSet<ushort> toOmit)
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
                        // Can't path through class starts.
                        if (SkillTree.rootNodeList.Contains(node.Id))
                            continue;
                        /// Don't add nodes that are already in the graph (as
                        /// target or start nodes).
                        if (searchGraph.nodeDict.ContainsKey(node))
                            continue;
                        // Don't add nodes that should not be skilled.
                        if (toOmit.Contains(node.Id))
                            continue;
                        
                        searchGraph.AddNode(node);
                    }
                }
            }
        }
        
        /// <summary>
        ///  Finds the nodes in the search graph that can be potential steiner
        ///  nodes. Those form the search space base.
        /// </summary>
        private void buildSearchSpaceBase()
        {
            searchSpaceBase = new List<GraphNode>();

            MinimalSpanningTree leastSolution = new MinimalSpanningTree(targetNodes, distances);
            leastSolution.Span(startFrom: startNodes);
            // Saving the leastSolution as initial solution. Makes sure there is always a
            // solution even if the search space is empty.
            BestSolution = SpannedMstToSkillnodes(leastSolution, false);

            int maxEdgeDistance = 0;
            foreach (GraphEdge edge in leastSolution.SpanningEdges)
            {
                int edgeDistance = distances.GetDistance(edge);
                if (edgeDistance > maxEdgeDistance)
                    maxEdgeDistance = edgeDistance;
            }
            /*
            int maxTargetDistance = 0;
            foreach (GraphNode targetNode in targetNodes)
            {
                int targetDistance = distances.GetDistance(targetNode, startNodes);
                if (targetDistance > maxTargetDistance)
                    maxTargetDistance = targetDistance;
            }*/

            // Find potential steiner points that are in reasonable vicinity.
            /// TODO: This can surely be improved in some shape or form, but I
            /// can't figure it out right now. Since the GA also has to work well
            /// with larger input sizes, I won't investigate this right now.
            foreach (GraphNode node in searchGraph.nodeDict.Values)
            {
                // This can be a steiner node only if it has more than 2 neighbors.
                if (node.Adjacent.Count > 2)
                {
                    /* 
                     * While this would mathematically be correct, it's not a
                     * good criterium for the skill tree. I don't think the
                     * relevant cases can appear and this permits way too many
                     * nodes to be considered that will never be included in an
                     * actual solution.
                     * 
                    /// If every target node is closer to the start than to a certain
                    /// steiner node, that node can't be needed for the steiner tree.
                    bool add = false;
                    foreach (GraphNode targetNode in targetNodes)
                        if (distances.GetDistance(targetNode, node) < distances.GetDistance(targetNode, startNodes))
                            add = true;
                    if (add)
                        searchSpaceBase.Add(node);
                     */

                    /*
                    /// This is a pretty handwavy approach... If anybody figures
                    /// out a case that causes this to fail, let me know please!
                    if (distances.GetDistance(node, startNodes) < 1.2 * maxTargetDistance)
                        searchSpaceBase.Add(node);
                     */

                    // This should be a reasonable approach.
                    bool add = false;
                    foreach (GraphNode targetNode in targetNodes)
                        if (distances.GetDistance(targetNode, node) < maxEdgeDistance)
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
        ///  Sets up the genetic algorithm to be ready for the evolutionary search.
        /// </summary>
        /// <param name="randomSeed">The random seed to be used for the RNG of the
        /// genetic algorithm. When left empty, a seed is generated based on the
        /// current date and time.</param>
        void initializeGA(int? randomSeed = null)
        {
            if (randomSeed == null) randomSeed = DateTime.Now.GetHashCode();
            ga = new GeneticAlgorithm(fitnessFunction, new Random(randomSeed.Value));

            Debug.WriteLine("Search space dimension: " + searchSpaceBase.Count);

            int populationSize = (int)(populationModifier * populationFct(searchSpaceBase.Count));
            maxGeneration = (int)(durationModifier * durationFct(searchSpaceBase.Count));
            int dnaLength = searchSpaceBase.Count; // Just being verbose.

            ga.InitializeEvolution(populationSize, maxGeneration, dnaLength);
        }

        /// <summary>
        ///  Tells the genetic algorithm to advance one generation and processes
        ///  the resulting (possibly) improved solution.
        /// </summary>
        public void EvolutionStep()
        {
            if (!_initialized)
                throw new InvalidOperationException("Solver not initialized!");

            ga.NewGeneration();

            if ((_bestDNA == null) || (GeneticAlgorithm.SetBits(ga.GetBestDNA().Xor(_bestDNA)) != 0))
            {
                _bestDNA = ga.GetBestDNA();
                MinimalSpanningTree bestMst = dnaToMst(_bestDNA);
                bestMst.Span(startFrom: startNodes);

                // #DEBUG#: Pass true to show the used steiner nodes.
                BestSolution = SpannedMstToSkillnodes(bestMst, false);
            }
        }

        /// <summary>
        ///  Converts an MST spanning a set of GraphNodes back into its equivalent
        ///  as a HashSet of SkillNode IDs.
        /// </summary>
        /// <param name="mst">The spanned MinimalSpanningTree.</param>
        /// <param name="visualize">A debug parameter that highlights all used
        /// GraphNodes' SkillNode equivalents in the tree.</param>
        /// <returns>A HashSet containing the node IDs of all SkillNodes spanned
        /// by the MST.</returns>
        HashSet<ushort> SpannedMstToSkillnodes(MinimalSpanningTree mst, bool visualize)
        {
            if (!mst.IsSpanned)
                throw new Exception("The passed MST is not spanned!");

            HashSet<ushort> newSkilledNodes = new HashSet<ushort>();
            foreach (GraphEdge edge in mst.SpanningEdges)
            {
                ushort target = edge.outside.Id;

                // The paths are calculated anyway, so use them here too.
                newSkilledNodes.UnionWith(distances.GetShortestPath(edge));
                newSkilledNodes.Add(target);
            }

            if (visualize)
            {
                tree._nodeHighlighter.UnhighlightAllNodes(NodeHighlighter.HighlightState.FromAttrib);
                foreach (GraphNode steinerNode in mst.mstNodes)
                    tree._nodeHighlighter.HighlightNode(SkillTree.Skillnodes[steinerNode.Id], NodeHighlighter.HighlightState.FromAttrib);
            }

            //tree.DrawHighlights(tree._nodeHighlighter);
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

            // TODO: Investigate fitness function
            return 1500 - usedNodes;
        }
    }
}



/*/// <summary>
///  Employs a genetic algorithm to search the search space (spanned by
///  the search space base) for a skill tree with lowest possible point
///  investment.
/// </summary>
/// <returns></returns>
MinimalSpanningTree findBestMst()
{
    Stopwatch timer = new Stopwatch();
    timer.Start();
     
    while (ga.GenerationCount < 200)
    {
        ga.NewGeneration();
        Thread.Yield();
    }
    timer.Stop();
    Console.WriteLine("Optimization time: " + timer.ElapsedMilliseconds + " ms");

    BitArray bestDna = ga.GetBestDNA();
    MinimalSpanningTree mst = dnaToMst(bestDna);
    mst.Span(startNodes);

    return mst;
}*/