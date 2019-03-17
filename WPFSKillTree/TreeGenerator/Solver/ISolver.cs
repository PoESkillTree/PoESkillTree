using System.Collections.Generic;

namespace PoESkillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Interface of solver classes for use without generic parameter.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Gets whether all steps where executed for each iteration.
        /// </summary>
        bool IsConsideredDone { get; }

        /// <summary>
        /// Gets the maximum number of steps the solver executes per iteration.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        int Steps { get; }

        /// <summary>
        /// Gets the number of steps executed up to this point.
        /// Return value is 0 until <see cref="Initialize"/> got called.
        /// </summary>
        int CurrentStep { get; }

        /// <summary>
        /// Gets the number of iterations that should be executed.
        /// </summary>
        int Iterations { get; }

        /// <summary>
        /// Gets the number of the iteration that is currently executed.
        /// This will only increase once the first step of the next iteration is being run.
        /// </summary>
        int CurrentIteration { get; }

        /// <summary>
        /// Gets the best solution generated up to this point as
        /// HashSet of <see cref="SkillTreeFiles.SkillNode"/> ids.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// 
        /// If these are counted, <see cref="UncountedNodes"/> has to be subtracted from the result.
        /// </summary>
        IEnumerable<ushort> BestSolution { get; }
        
        /// <summary>
        /// The number of nodes that are not counted as points in the result. Includes the
        /// hidden character start node and ascendancy nodes.
        /// </summary>
        int UncountedNodes { get; }

        /// <summary>
        /// Initializes the solver. Must be called before <see cref="Step"/>.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Progresses execution of the solver by one step.
        /// Call this function in a loop until <see cref="IsConsideredDone"/> is true.
        /// <see cref="Initialize"/> must be called before this.
        /// </summary>
        void Step();

        /// <summary>
        /// Optionally applies final algorithms after the last step to improve the solution.
        /// <see cref="Initialize"/> and <see cref="Step"/> must be called before this.
        /// </summary>
        void FinalStep();
    }
}