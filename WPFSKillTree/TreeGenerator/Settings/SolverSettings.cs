using System;
using System.Collections.Generic;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Settings
{
    /// <summary>
    /// Data class for the settings shared by all tree generator solvers.
    /// </summary>
    public class SolverSettings
    {

        /// <summary>
        /// Character Level that calculations are based on.
        /// </summary>
        public readonly int Level;

        /// <summary>
        /// Maximum for points spent in the result tree.
        /// May be ignored if the only goal of the solver is to minimize point count.
        /// </summary>
        public readonly int TotalPoints;

        /// <summary>
        /// Set of Nodes that must be included in the result tree.
        /// </summary>
        public readonly HashSet<SkillNode> Checked;

        /// <summary>
        /// Set of Nodes that must not be included in the result tree.
        /// </summary>
        public readonly HashSet<SkillNode> Crossed;

        /// <summary>
        /// Nodes the result tree must be a subset of. (empty means no restriction)
        /// </summary>
        public readonly ObservableSet<SkillNode> SubsetTree;

        /// <summary>
        /// Tree for the initial configuration. (empty means starting from scratch)
        /// </summary>
        public readonly ObservableSet<SkillNode> InitialTree;

        /// <summary>
        /// The number of iterations the solver should run. Each iteration is independent of each other
        /// and the best solution over all iterations is kept.
        /// </summary>
        public readonly int Iterations;

        /// <summary>
        /// Creates new SolverSettings.
        /// </summary>
        /// <param name="level">Character Level that calculations are based on. (>= 0)</param>
        /// <param name="totalPoints">Maximum for points spent in the result tree. (>= 0)</param>
        /// <param name="checked">Set of Nodes that must be included in the result tree.</param>
        /// <param name="crossed">Set of Nodes that must not be included in the result tree.</param>
        /// <param name="subsetTree">Nodes the result tree must be a subset of. (empty means no restriction)</param>
        /// <param name="initialTree">Tree for the initial configuration. (empty means starting from scratch)</param>
        /// <param name="iterations">The number of iterations the solver should run. (>= 1)</param>
        public SolverSettings(int level, int totalPoints, HashSet<SkillNode> @checked, HashSet<SkillNode> crossed,
            ObservableSet<SkillNode> subsetTree, ObservableSet<SkillNode> initialTree, int iterations)
        {
            if (level < 0) throw new ArgumentOutOfRangeException("level", level, "must be >= 0");
            if (totalPoints < 0) throw new ArgumentOutOfRangeException("totalPoints", totalPoints, "must be >= 0");
            if (iterations < 1) throw new ArgumentOutOfRangeException("iterations", iterations, "must be >= 1");

            Level = level;
            TotalPoints = totalPoints;
            Checked = @checked ?? new HashSet<SkillNode>();
            Crossed = crossed ?? new HashSet<SkillNode>();
            SubsetTree = subsetTree ?? new ObservableSet<SkillNode>();
            InitialTree = initialTree ?? new ObservableSet<SkillNode>();
            Iterations = iterations;
        }
    }
}