using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Represents a single node in the calculation graph.
    /// </summary>
    public interface ICalculationNode
    {
        /// <summary>
        /// Gets the node's value based on its child nodes. It is calculated lazily.
        /// </summary>
        NodeValue? Value { get; }

        /// <summary>
        /// Event that is raised when this node's value changed (or rather, when it needs to be re-calculated).
        /// </summary>
        event EventHandler ValueChanged;
    }
}