using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Collection of <see cref="Modifier"/>s and their nodes
    /// </summary>
    public class ModifierNodeCollection : ISuspendableEventViewProvider<INodeCollection<Modifier>>
    {
        private readonly ISuspendableEventViewProvider<NodeCollection<Modifier>> _viewProvider;

        private readonly Lazy<SuspendableEventsComposite> _suspenderComposite;

        public ModifierNodeCollection(ISuspendableEventViewProvider<NodeCollection<Modifier>> viewProvider)
        {
            _viewProvider = viewProvider;
            _suspenderComposite = new Lazy<SuspendableEventsComposite>(CreateSuspender);
        }

        private SuspendableEventsComposite CreateSuspender()
        {
            var s = new SuspendableEventsComposite();
            s.Add(_viewProvider.Suspender);
            return s;
        }

        public INodeCollection<Modifier> DefaultView => _viewProvider.DefaultView;
        public INodeCollection<Modifier> SuspendableView => _viewProvider.SuspendableView;
        public ISuspendableEvents Suspender => _suspenderComposite.Value;
        public int SubscriberCount => _viewProvider.SubscriberCount;

        public void Add(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            _viewProvider.DefaultView.Add(node.DefaultView, modifier);
            _viewProvider.SuspendableView.Add(node.SuspendableView, modifier);
            _suspenderComposite.Value.Add(node.Suspender);
        }

        public void Remove(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            _viewProvider.DefaultView.Remove(node.DefaultView, modifier);
            _viewProvider.SuspendableView.Remove(node.SuspendableView, modifier);
            _suspenderComposite.Value.Remove(node.Suspender);
        }
    }
}