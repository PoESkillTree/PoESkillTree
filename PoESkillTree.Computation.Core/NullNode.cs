using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class NullNode : ICalculationNode
    {
        public NodeValue? Value => null;

        public event EventHandler ValueChanged;

        public void Dispose()
        {
        }
    }
}