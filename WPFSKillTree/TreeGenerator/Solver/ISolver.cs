using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Interface of solver classes for use without generic parameter.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Gets whether the maximum number of steps is executed.
        /// </summary>
        bool IsConsideredDone { get; }

        /// <summary>
        /// Gets the maximum number of steps the solver executes.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        int MaxSteps { get; }

        /// <summary>
        /// Gets the number of steps executed up to this point.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        int CurrentStep { get; }

        /// <summary>
        /// Gets the best solution generated up to this point as
        /// HashSet of <see cref="SkillTreeFiles.SkillNode"/> ids.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        IEnumerable<ushort> BestSolution { get; }

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