using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Factory for creating nodes (<see cref="IDisposableNodeViewProvider"/>) that calculate their value using
    /// <see cref="IValue"/>.
    /// </summary>
    public interface INodeFactory
    {
        IDisposableNodeViewProvider Create(IValue value);
    }
}