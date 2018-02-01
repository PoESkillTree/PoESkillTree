using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class NullNode : IDisposableNode
    {
        public NodeValue? Value => null;

        public event EventHandler ValueChanged;

        public void Dispose()
        {
        }
    }
}