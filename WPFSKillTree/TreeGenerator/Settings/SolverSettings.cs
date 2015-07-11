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
        /// Starting set of stats that calculations are based on. (for example from items)
        /// </summary>
        public readonly Dictionary<string, List<float>> InitialStats;

        /// <summary>
        /// Nodes the result tree must be a subset of. (empty means no restriction)
        /// </summary>
        public readonly HashSet<ushort> SubsetTree;

        /// <summary>
        /// Tree for the initial configuration. (empty means starting from scratch)
        /// </summary>
        public readonly HashSet<ushort> InitialTree;

        public SolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
            Dictionary<string, List<float>> initialStats, HashSet<ushort> subsetTree, HashSet<ushort> initialTree)
        {
            Level = level;
            TotalPoints = totalPoints;
            Checked = @checked ?? new HashSet<ushort>();
            Crossed = crossed ?? new HashSet<ushort>();
            InitialStats = initialStats ?? new Dictionary<string, List<float>>();
            SubsetTree = subsetTree ?? new HashSet<ushort>();
            InitialTree = initialTree ?? new HashSet<ushort>();
        }
    }
}