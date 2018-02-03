using System;
using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class ModifierNodeCollection : ISuspendableEventViewProvider<INodeCollection<Modifier>>
    {
        private readonly ISuspendableEventViewProvider<NodeCollection<Modifier>> _viewProvider;

        private readonly Lazy<SuspendableEventsComposite> _suspenderComposite;

        private readonly Dictionary<Modifier, Stack<ISuspendableEventViewProvider<IDisposableNode>>> _items
            = new Dictionary<Modifier, Stack<ISuspendableEventViewProvider<IDisposableNode>>>();

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

        public void Add(ISuspendableEventViewProvider<IDisposableNode> node, Modifier modifier)
        {
            _viewProvider.DefaultView.Add(node.DefaultView, modifier);
            _viewProvider.SuspendableView.Add(node.SuspendableView, modifier);
            _suspenderComposite.Value.Add(node.Suspender);
        }

        public void Remove(ISuspendableEventViewProvider<IDisposableNode> node)
        {
            _viewProvider.DefaultView.Remove(node.DefaultView);
            _viewProvider.SuspendableView.Remove(node.SuspendableView);
            _suspenderComposite.Value.Remove(node.Suspender);
        }

        public void Add(Modifier modifier, ISuspendableEventViewProvider<IDisposableNode> node)
        {
            _viewProvider.DefaultView.Add(node.DefaultView, modifier);
            _viewProvider.SuspendableView.Add(node.SuspendableView, modifier);
            _suspenderComposite.Value.Add(node.Suspender);
            _items.GetOrAdd(modifier, k => new Stack<ISuspendableEventViewProvider<IDisposableNode>>()).Push(node);
        }
    }
}