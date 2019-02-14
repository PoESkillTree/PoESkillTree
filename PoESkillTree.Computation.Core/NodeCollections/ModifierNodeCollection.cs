using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Collection of <see cref="Modifier"/>s and their nodes
    /// </summary>
    public class ModifierNodeCollection : IBufferingEventViewProvider<INodeCollection<Modifier>>
    {
        private readonly IBufferingEventViewProvider<NodeCollection<Modifier>> _viewProvider;

        public ModifierNodeCollection(IBufferingEventViewProvider<NodeCollection<Modifier>> viewProvider)
            => _viewProvider = viewProvider;

        public INodeCollection<Modifier> DefaultView => _viewProvider.DefaultView;
        public INodeCollection<Modifier> BufferingView => _viewProvider.BufferingView;
        public int SubscriberCount => _viewProvider.SubscriberCount;

        public void Add(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            _viewProvider.DefaultView.Add(node.DefaultView, modifier);
            _viewProvider.BufferingView.Add(node.BufferingView, modifier);
        }

        public void Remove(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            _viewProvider.DefaultView.Remove(node.DefaultView, modifier);
            _viewProvider.BufferingView.Remove(node.BufferingView, modifier);
        }
    }
}