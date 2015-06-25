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
        ///     values (which are cached and thus inexpensive to access).
        ///     This introduces considerable ambiguity, to the point where DNAs
        ///     that would map to the same output skill tree can have substantially
        ///     different fitnesses, as well as (consequently) a DNA with a better
        ///     skill tree having worse fitness than a DNA with a worse skill tree.
        ///     A more detailed explanation is found in Example 1 below.
        ///     
        ///     However, the DNA with the global maximum fitness value will always
        ///     map to the best possible skill tree (as do many other DNAs with
        ///     near-optimal fitness). It is therefore imperative that the GA is
        ///     granted enough computational budget (and is robust enough) to find
        ///     the optimum, as the DNA with the best encountered fitness value
        ///     might not even correspond to the best skill tree when far away from
        ///     the optimum.
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
        ///     paths are not cached and have to be computed anew for every conver-
        ///     sion from DNA -> graph MST -> skill tree.
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
        /// 
        /// Example 2: Ambiguity/inaccuracy of the fitness function
        /// You start as Scion (no nodes skilled) and highlight Inner Force as well
        /// as Elemental Equilibrium and let the optimizer run. (All subsequently
        /// mentioned skill nodes will have a 1:1 equivalent as graph nodes, so no
        /// distinction will be made).
        /// Assume that a DNA (DNA1) corresponds to only using the Shaper node as
        /// steiner node. When building the MST, first the scion start will be con-
        /// nected to Shaper, then Shaper to EE, then Shaper to IF. These distances
        /// are then summed up for the weight of the MST, which counts the int node
        /// above Shaper twice (and thus overestimates the cost by 1 point).
        /// A different DNA (DNA2) that would correspond to the only steiner node
        /// being the middle mana node at scion start would similarly have its cost
        /// overestimated by three points, despite mapping to the very same (optimal)
        /// skill tree in the end.
        /// 
        /// It should be obvious from this that a non-optimal DNA can thus beat the
        /// fitness of an optimal, but "badly" represented tree/DNA: An otherwise
        /// optimal-fitness DNA (see below) could also include (for the sake of the
        /// example) the strength node below Shaper as a steiner node. It would thus
        /// have a fitness of 1 below optimal and, for the GA, would be considered
        /// superior to DNA2 which has a fitness of 2 points worse.
        /// 
        /// An optimal DNA would have the above-mentioned int node as steiner node.
        /// It can potentially have more steiner nodes on the path (which won't be
        /// real steiner nodes - it doesn't matter though since they don't affect
        /// the cost in the end), so more than one optimal DNA exists. Note how the
        /// sum of the MST edge lengths of this ideal DNA will always equal the
        /// amount of points that the corresponding skill tree actually needs, so
        /// it will always be a minimum cost (maximum fitness) solution, which the
        /// GA can eventually find.
        /// 
        /// In short: The fitness function will never overestimate the fitness of
        /// a given DNA. There exists a DNA that represents the optimal skill tree
        /// and is not overestimated (by virtue of having selected the steiner nodes
        /// that the optimal skill tree involves), so any DNA representing a non-
        /// optimal tree will necessarily have worse fitness values.

        SkillTree tree;

        SearchGraph searchGraph;
        DistanceLookup distances = new DistanceLookup();
        List<GraphNode> searchSpaceBase;

        Supernode startNodes;
        HashSet<GraphNode> targetNodes;

        GeneticAnnealingAlgorithm ga;

        //  The base modifier for population is higher than generation because a far higher
        // population than maxGeneration gives much better results from the testing I've done.
        //  Also the duration got another modifier that leads to higher values for dimensions
        // < 150. Low dimensions run fast enough anyway, so it doesn't feel too long.
        // Fixes the Witch-triangle-case most of the time.
        //  Might look odd, but this is a accessible space where you can easily modify
        // these without crawling through the code. So as long as these are not set in
        // stone I like declaring it this way. Also parametrizing the SteinerSolver
        // does not feel to me like it should belong into GUI-related code.
        /// <summary>
        ///  Returns the modifier for the maxGeneration in initializeGA depending on
        ///  the searchSpace size. Returns 0.2 times the result of a polynomial function
        ///  if searchSpace &lt; 150 or just 0.2 otherwise.
        ///  The polynomial function is fitted to f(0)=40, f(100)=5 and f(150)=1.
        /// </summary>
        private Func<int, double> durationModifierFct = (searchSpace) =>
            (0.2 * (searchSpace < 150 ? 0.0018 * searchSpace * searchSpace - 0.53 * searchSpace + 40 : 1.0));
        /// <summary>
        ///  Returns the modifier for the populationSize in initializeGA depending on
        ///  the searchSpace size. Returns simply 1.5 at the moment.
        /// </summary>
        private Func<int, double> populationModifierFct = (searchSpace) =>
            1.5;

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
        /// <param name="durationModifier">A multiplier to the amount of iterations
        /// to go through (default: 1.0).</param>
        /// <param name="populationModifier">A multiplier to the size of the
        /// algorithm's solution pool (default: 1.0).</param>
        public void InitializeSolver(HashSet<ushort> targets,
            double durationModifier = 1.0, double populationModifier = 1.0)
        {
            // (This is not in the constructor since it might take a moment.)
            this.durationModifier = durationModifier;
            this.populationModifier = populationModifier;

            buildSearchGraph(targets);

            buildSearchSpaceBase();

            initializeGA();

            _initialized = true;
        }

        /// <summary>
        ///  Preprocesses the SkillTree graph into a simplified graph that omits
        ///  any isolated node groups (single pass) and contracts all skilled nodes
        ///  into a single node.
        /// </summary>
        /// <param name="targets">A set of node IDs representing the target nodes.</param>
        /// <returns>A SearchGraph representing the simplified SkillTree</returns>
        private void buildSearchGraph(HashSet<ushort> targets)
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
            ga = new GeneticAnnealingAlgorithm(fitnessFunction, new Random(randomSeed.Value));

            Console.WriteLine("Search space dimension: " + searchSpaceBase.Count);

            int populationSize = (int)(populationModifier * searchSpaceBase.Count * populationModifierFct(searchSpaceBase.Count));
            maxGeneration = (int)(durationModifier * searchSpaceBase.Count * durationModifierFct(searchSpaceBase.Count));
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

            if ((_bestDNA == null) || (GeneticAnnealingAlgorithm.SetBits(ga.GetBestDNA().Xor(_bestDNA)) != 0))
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

                //HashSet<ushort> start;
                //if (edge.inside is Supernode)
                //    start = tree.SkilledNodes;
                //else
                //    start = new HashSet<ushort>() { edge.inside.Id };

                //var path = tree.GetShortestPathTo(target, start);

                //newSkilledNodes = new HashSet<ushort>(newSkilledNodes.Concat(path));
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

            // Now accurate, not counting nodes more than once.
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