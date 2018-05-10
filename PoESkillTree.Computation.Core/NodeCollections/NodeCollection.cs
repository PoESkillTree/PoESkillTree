namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class NodeCollection<TProperty>
        : SuspendableObservableCollection<(ICalculationNode node, TProperty property)>, INodeCollection<TProperty>
    {
        // Making NodeCollection suspendable is not optimal, but everything else would lead to at least one
        // of the required classes being a duplicate because there is no multiple inheritance.
        // (SuspendableNodeCollection, NodeCollection, SuspendableObservableCollection, ObservableCollection=

        public void Add(ICalculationNode node, TProperty property) => Add((node, property));

        public void Remove(ICalculationNode node, TProperty property) => Remove((node, property));
    }
}