using System;
using PoESkillTree.Computation.Common;

namespace POESKillTree.Computation.Model
{
    public interface IObservableNodeRepository
    {
        IObservable<NodeValue?> ObserveNode(IStat stat, NodeType nodeType = NodeType.Total);
    }
}