using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
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