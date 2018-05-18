using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface ICalculationNode
    {
        // Gets the node's value based on its child nodes. It is calculated lazily.
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}