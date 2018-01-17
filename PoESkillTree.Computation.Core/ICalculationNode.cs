using System;

namespace PoESkillTree.Computation.Core
{
    /*
     * Core interface: ICalculationNode
     * - Value gets the node's value based on its child nodes
     *   - For nodes not directly used by other projects and not implementing ICachingNode, this is not cached,
     *     i.e. it calculates the value based on the children with every call
     *   - Even for ICachingNodes, the value is calculated lazily, i.e. not when ValueChanged is raised
     * - Will have many different implementations depending on how the value is calculated
     * - Value will not throw exceptions, they should be thrown on construction
     * - For a core node N:
     *   - N is decorated by a CachingNode, which is used instead of it
     *   - Children of N are CachingNodeAdapters to the decorators of the actual children
     *   - N subscribes to ValueChanged of its children
     *   - N raises ValueChanged when
     *     - ValueChanged of a child is raised, or
     *     - N itself changed, e.g. a child was added
     *
     * (Re-)Calculating values efficiently:
     * Event based, two pass:
     *   1. Core nodes raise "ValueChanged" when being changed directly or when a child raises this event
     *     - This leads to decorating nodes (CachingNode) raising "ValueChangeReceived", which in turn
     *       leads to their parent core nodes raising "ValueChanged" (through CachingNodeAdapter)
     *     - These events will go through the graph marking nodes as dirty
     *     - Each CachingNode raises "ValueChangeReceived" at most once
     *     - Some object subscribes to all CachingNodes and saves which nodes raised events
     *   2. That object then raises "ValueChanged" on all these nodes (through ICachingNode.RaiseValueChanged())
     *     - These events can be subscribed to by the UI to update displayed values, they will only be raised once
     *       for each node
     * - With batch updates, the second step only has to be done at the end
     * - The ValueChanged events can easily be used by the UI (transformed to PropertyChanged events in ViewModels)
     *
     * Builder implementations:
     * - "Source" needs to be passed together with Modifier: Can be Given, Tree, Skill or Item (or maybe something
     *   else). Also contains information about e.g. tree node names, "Dexterity", item slots and names, ...
     * - What IStat and IValue represent will be different from how they are used in the data
     *   - IStat defines the subgraph of the calculation graph the modifier affects
     *     (for non-data-driven modifiers like conversion: a special property of IStat defines what type of special
     *      mechanic they are)
     *   - Form defines where in the subgraph the modifier is applied
     *   - IValue defines the formula to calculate the modifier's value. This can include branches, arithmetic operators
     *     and references to other nodes (mainly to nodes defined as IStat)
     *     - This is used to calculate ICalculationNode.Value. The value is nullable, allowing the representation
     *       of modifiers that aren't applied, e.g. because of their conditions.
     * - IStat may need to include further modifiers, e.g. for aura modifiers to count towards the aura count,
     *   depending on how they are implemented (i.e. multiple stats without being modified by the same form and value).
     *
     * Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     *
     * Construction of the graph:
     * - Batch update:
     *   - pass all modifiers to construction object at once
     *   - order the passed modifiers to make construction as fast as possible
     *   - only recalculate values once batch is done
     * - will mostly be handled outside of ICalculationNode implementations
     *   (except when adding children to existing nodes)
     * - Needs to make sure events are properly unsubscribed from
     *   (ICalculationNode implements IDisposable for this reason)
     * - Core calculation node implementations need some kind of context as parameter providing information that can
     *   not be expressed as connections to other nodes, e.g. global condition specified by the user and allowing
     *   connecting to other nodes
     *   - Adding nodes for this kind of information allows other nodes to use it in a way fitting everything else
     *   - Changes to the context need to trigger ValueChanged (in the nodes representing the information)
     *   - Some "GetOrAdd" as the method to retrieve nodes from the context
     *   - Conditions can be added both ways:
     *     - In the UI. No nodes are created until it is referenced.
     *     - When adding nodes. The UI adds the condition if it can be user specified.
     * - To allow for removing modifiers, each modifier source has to store its modifiers
     * - Adding "special" modifiers (non-data-driven, e.g. conversion):
     *   - These modifiers are triggered by a special property of IStat
     *   - Implemented in whatever is responsible for creating the stat subgraph
     *   - Mod source based conditions:
     *     - Splits up the form collections
     *     - Behaves like a normal modifier with the above addition
     *     - For the main calculation graph (not preview), it might be useful to always split by source (for breakdowns)
     *   - "Modifiers to Bar also apply to Foo":
     *     - Adds links from Foo's form collection using nodes to the form collections of Bar (with a multiplier)
     *     - The modifier's form and value are irrelevant (should always be TotalOverride and 1)
     *     - The main property of IStat defines the affected subgraph (Foo)
     *   - Conversions and Gains
     *     - Affects both the source and the target stat's subgraphs (and further targets in case of chains)
     *     - The main property of IStat defines the conversion (source and target)
     *     - Need a lot of special handling (e.g. they can be conditional).
     * - Adding normal modifiers:
     *   - Get the subgraph for each stat (or create them)
     *   - Select the collection for the form
     *   - Create the rooted DAG for the modifier from the value
     *   - Add the root node to the collection
     *   This implies the following
     *   - The whole subgraph for a stat is built when creating it. This can probably be improved by referencing
     *     the form collections in the root node until it they are no longer empty and the corresponding node needs
     *     to be created.
     *   - All modifiers are added to the graph, even if their conditions are false. I think it is less
     *     complicated this way, otherwise changing conditions could require larger changes to the graph.
     *     - Conditions should be the first things checked when calculating values so the calculation can be shortcut.
     *     - Only the children necessary to calculate the value should be subscribed to and be evaluated.
     *       - This can only be decided after the value is calculated the first time. Initially, nodes must subscribe
     *         to all their children.
     *       - When calculating the value, subscribe/unsubscribe to children as required.
     *       - E.g. If a condition to the modifier's application is false and the value is therefore null,
     *         only this condition needs to be subscribed to.
     *     - This allows the entire graph being pre-built and all modifiers having conditions stating that their
     *       corresponding skill tree nodes/items/... must be selected. Though, I don't think that makes sense for
     *       anything except maybe tree nodes.
     * - Removing modifiers: The reverse of adding them.
     * - Retrieving nodes to subscribe to:
     *   - ICalculationNode GetNode(IStat stat) (stat subgraph, {Total, Subtotal, Uncapped Subtotal, Base})
     *     (with conversions, this returns the unconverted Base node)
     *   - [...] GetNodes(IStat, Form) (returns the form nodes of stat subgraph for each conversion/source path)
     *     (the caller needs to figure out when the returned collection, not the nodes in it, change by themselves)
     *   - [...] GetNodeCollection(IStat, Form) (returns the form node collection of stat)
     *     (whatever the type of node collections is, it needs to implement an interface that is returned here that
     *     supports everything the UI requires, including change notifications. Re-rendering the whole table is fine,
     *     i.e. parameterless change notifications)
     */

    public interface ICalculationNode : IDisposable
    {
        double? Value { get; }

        event EventHandler ValueChanged;
    }


    public interface ICachingNode : ICalculationNode
    {
        void RaiseValueChanged();

        event EventHandler ValueChangeReceived;
    }
}