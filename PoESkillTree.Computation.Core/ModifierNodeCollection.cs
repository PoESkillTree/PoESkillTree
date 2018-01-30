using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class ModifierNodeCollection : ISuspendableEventViewProvider<INodeCollection<Modifier>>
    {
        private readonly ISuspendableEventViewProvider<NodeCollection<Modifier>> _viewProvider;

        private readonly Lazy<SuspendableEventsComposite> _suspenderComposite;

        private readonly Dictionary<Modifier, Stack<ISuspendableEventViewProvider<ICalculationNode>>> _items
            = new Dictionary<Modifier, Stack<ISuspendableEventViewProvider<ICalculationNode>>>();

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

        public void Add(Modifier modifier, ISuspendableEventViewProvider<ICalculationNode> node)
        {
            _viewProvider.DefaultView.Add(node.DefaultView, modifier);
            _viewProvider.SuspendableView.Add(node.SuspendableView, modifier);
            _suspenderComposite.Value.Add(node.Suspender);
            _items.GetOrAdd(modifier, k => new Stack<ISuspendableEventViewProvider<ICalculationNode>>()).Push(node);
        }

        [CanBeNull]
        public ISuspendableEventViewProvider<ICalculationNode> Remove(Modifier modifier)
        {
            if (!TryGetNodeProvider(modifier, out var node))
            {
                return null;
            }

            _viewProvider.DefaultView.Remove(node.DefaultView);
            _viewProvider.SuspendableView.Remove(node.SuspendableView);
            _suspenderComposite.Value.Remove(node.Suspender);
            return node;
        }

        private bool TryGetNodeProvider(
            Modifier modifier, out ISuspendableEventViewProvider<ICalculationNode> nodeProvider)
        {
            if (!_items.TryGetValue(modifier, out var stack))
            {
                nodeProvider = null;
                return false;
            }

            nodeProvider = stack.Pop();
            if (stack.IsEmpty())
            {
                _items.Remove(modifier);
            }
            return true;
        }
    }
}