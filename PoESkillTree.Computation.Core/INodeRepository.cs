using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation

        // stat selects the stat subgraph, nodeType the node in it.
        // With conversions and/or sources:
        // - Increase, More: the node on the unconverted, Global path.
        // - Base, BaseOverride, BaseSet, Base Add: the unconverted base node.
        // - UncappedSubtotal: The node that sums all paths.
        // - Subtotal, TotalOverride, Total: There should only be one.
        // If stat is null, this returns a node that always has a value of null
        ICalculationNode GetNode([CanBeNull] IStat stat, NodeType nodeType = NodeType.Total);

        // stat selects the stat subgraph, nodeType the node in it.
        // Only one NodeType from Total, Subtotal and UncappedSubtotal make sense, probably Uncapped Subtotal as
        // that's where these path subgraphs end up. BaseOverride, BaseSet, BaseAdd and TotalOverride don't make sense.
        // Returns all nodes by conversion path and source.
        //INodeCollection<NodePathProperty> GetPathNodes(IStat stat, NodeType nodeType = NodeType.Total);
        // NodePathProperty: Contains the path's definition
        // - Its IModifierSource (only with the information that is the same for all modifiers of the path)
        // - The IStats on its conversion path (the node's IStat itself if unconverted)

        // Returns the form node collection of stat
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form);
    }

    // Should probably be split up into multiple classes (it even needs to be split up, currently NodeFactory and this
    // would need to be passed as constructor parameters to each other)
    public class MainNodeRepository : ISuspendableEventViewProvider<INodeRepository>
    {
        private readonly Dictionary<IStat, int> _modifierCounts;
        private readonly HashSet<IStat> _statsWithoutModifiers;

        private readonly Dictionary<IStat, Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>>
            _subgraphNodes;

        private readonly Dictionary<IStat, Dictionary<Form, ModifierNodeCollection>> _formCollections;

        /*
         * CalculationNode GetNode(IStat, NodeType):
         * - If entry does not exist in _subgraphNodes:
         *   - Create nodes with INodeFactory.Create(IStat, NodeType
         *   - Add dictionary entry
         * - Return node from entry that is relevant to the view
         * INodeCollection<FormNodeCollectionItem> GetFormNodes(IStat, Form)
         * - If entry does not exist in _formCollections:
         *   - Create ModifierNodeCollection
         *   - Add dictionary entry
         * - Return relevant view to ModifierNodeCollection
         */
        public INodeRepository DefaultView { get; }
        public INodeRepository SuspendableView { get; }
        public ISuspendableEvents Suspender { get; }

        public void AddModifier(Modifier modifier)
        {
            /* - For each stat in Modifier.Stats
             *   - Retrieve (and potentially create, see above) ModifierNodeCollection for stat and Modifier.Form
             *   - Create nodes with INodeFactory.Create(Modifier.Value)
             *   - Call ModifierNodeCollection.AddModifier
             *   - Increment _modifierCounts
             *   - Remove stat from _statsWithoutModifiers
             */
        }

        public void RemoveModifier(Modifier modifier)
        {
            /* - For each stat in Modifier.Stats
             *   - Retrieve  ModifierNodeCollection for stat and Modifier.Form
             *   - Call ModifierNodeCollection.RemoveModifier
             *   - Dispose nodes
             *   - Decrement _modifierCounts
             *   - Add stat to _statsWithoutModifiers if _modifierCounts[stat] == 0
             */
        }

        public void RemoveUnusedNodes()
        {
            /* - For each stat in _statsWithoutModifiers
             *   - Go top-down through the stat subgraph nodes
             *     - Nodes from the (IStat, NodeType) dictionary:
             *       - Sum CachingNodeAdapter. and CachingNode.ValueChanged.GetInvocationList().Length
             *       - If it is 0, the client and no stat subgraph references the node. It can be removed.
             *     - Form collections (they should all be empty):
             *       - Sum CachingNodeView. and CachingNodeAdapterView.ItemsChanged.GetInvocationList().Length
             *       - If it is 0, it can be removed
             *     - Removing means calling Dispose and removing from the dictionary
             *   - If the stat no longer has nodes and collections, remove it from _statsWithoutModifiers
             * (needs a sub interface for ICalculationNode and INodeCollection that counts the ValueChanged/ItemsChanged subscribers
             */
        }
    }
}