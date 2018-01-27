using System;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Core
{
    public interface INodeCollection : IReadOnlyCollection<ICalculationNode>
    {
        event EventHandler<NodeCollectionChangeEventArgs> CollectionChanged;
    }

    public interface INodeCollection<TProperty> : INodeCollection
    {
        IReadOnlyDictionary<ICalculationNode, TProperty> NodeProperties { get; }
    }

    public enum NodeCollectionChangeAction
    {
        Add,
        Remove,
        Reset
    }

    public class NodeCollectionChangeEventArgs : EventArgs
    {
        public static readonly NodeCollectionChangeEventArgs ResetEventArgs =
            new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Reset, null);

        public NodeCollectionChangeEventArgs(NodeCollectionChangeAction action, ICalculationNode element)
        {
            Action = action;
            Element = element;
        }

        public NodeCollectionChangeAction Action { get; }
        public ICalculationNode Element { get; }
    }
}