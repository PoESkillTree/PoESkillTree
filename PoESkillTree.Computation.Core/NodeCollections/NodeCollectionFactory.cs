using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class NodeCollectionFactory : INodeCollectionFactory
    {
        public ModifierNodeCollection Create()
        {
            var defaultView = new NodeCollection<Modifier>();
            var suspendableView = new SuspendableNodeCollection<Modifier>();
            var viewProvider = SuspendableEventViewProvider.Create(defaultView, suspendableView);
            return new ModifierNodeCollection(viewProvider);
        }
    }
}