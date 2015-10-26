using System.Collections.Generic;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Implementation of AbstractSolver that solves the Steiner tree problem.
    /// </summary>
    public class SteinerSolver : AbstractSolver<SolverSettings>
    {
        ///////////////////////////////////////////////////////////////////////////
        /// This code is a heuristic solution to the Steiner tree problem (STP).
        /// (The reader's knowledge of the STP and commonly associated terms is
        /// assumed.)
        /// 
        /// The main code logic is found in these classes:
        ///  - SteinerSolver and AbstractSolver:
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

        protected override GeneticAlgorithmParameters GaParameters
        {
            get
            {
                return new GeneticAlgorithmParameters(
                    100,
                    (int)(1.5 * SearchSpace.Count),
                    SearchSpace.Count);
            }
        }

        /// <summary>
        /// Creates a new, uninitialized instance.
        /// </summary>
        /// <param name="tree">The (not null) skill tree in which to optimize.</param>
        /// <param name="settings">The (not null) settings that describe what the solver should do.</param>
        public SteinerSolver(SkillTree tree, SolverSettings settings)
            : base(tree, settings)
        {
            // The implemented HillClimbing that swaps single nodes doesn't make much sense for
            // the Steiner tree problem.
            FinalHillClimbEnabled = false;
        }

        protected override double FitnessFunction(HashSet<ushort> skilledNodes)
        {
            // Fitness is higher for less points skilled.
            return 1500 - skilledNodes.Count;
        }
    }
}