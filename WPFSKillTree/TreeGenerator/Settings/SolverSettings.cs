using System;
using System.Collections.Generic;

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
        public readonly HashSet<ushort> Checked;

        /// <summary>
        /// Set of Nodes that must not be included in the result tree.
        /// </summary>
        public readonly HashSet<ushort> Crossed;

        /// <summary>
        /// Nodes the result tree must be a subset of. (empty means no restriction)
        /// </summary>
        public readonly HashSet<ushort> SubsetTree;

        /// <summary>
        /// Tree for the initial configuration. (empty means starting from scratch)
        /// </summary>
        public readonly HashSet<ushort> InitialTree;

        /// <summary>
        /// Creates new SolverSettings.
        /// </summary>
        /// <param name="level">Character Level that calculations are based on. (>= 0)</param>
        /// <param name="totalPoints">Maximum for points spent in the result tree. (>= 0)</param>
        /// <param name="checked">Set of Nodes that must be included in the result tree.</param>
        /// <param name="crossed">Set of Nodes that must not be included in the result tree.</param>
        /// <param name="subsetTree">Nodes the result tree must be a subset of. (empty means no restriction)</param>
        /// <param name="initialTree">Tree for the initial configuration. (empty means starting from scratch)</param>
        public SolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
            HashSet<ushort> subsetTree, HashSet<ushort> initialTree)
        {
            if (level < 0) throw new ArgumentOutOfRangeException("level", level, "must be >= 0");
            if (totalPoints < 0) throw new ArgumentOutOfRangeException("totalPoints", totalPoints, "must be >= 0");

            Level = level;
            TotalPoints = totalPoints;
            Checked = @checked ?? new HashSet<ushort>();
            Crossed = crossed ?? new HashSet<ushort>();
            SubsetTree = subsetTree ?? new HashSet<ushort>();
            InitialTree = initialTree ?? new HashSet<ushort>();
        }
    }
}