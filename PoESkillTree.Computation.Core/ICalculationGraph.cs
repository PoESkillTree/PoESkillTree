using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface ICalculationGraph : INodeViewProviderRepository, IModifierCollection
    {
        IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> GetNodes(IStat stat);
        void RemoveNode(IStat stat, NodeType nodeType);
        IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>> 
            GetFormNodeCollections(IStat stat);
        void RemoveFormNodeCollection(IStat stat, Form form);
        void RemoveStat(IStat stat);
    }
}