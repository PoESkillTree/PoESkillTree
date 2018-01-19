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
     * - All other API-sided events, i.e. those of collections in ICalculationGraph.NodeRepository and
     *   .ExternalStatRegistry, will only be raised after both passes (or at least only after the first pass).
     *
     * Builder implementations:
     * - What IStat and IValue represent will be different from how they are used in the data
     *   - IStat defines the subgraph of the calculation graph the modifier affects
     *     (for non-data-driven modifiers like conversion: a special property of IStat defines what type of special
     *      mechanic they are)
     *   - Form defines where in the subgraph the modifier is applied
     *   - IValue defines the formula to calculate the modifier's value. This can include branches, arithmetic operators
     *     and references to other nodes (mainly to nodes defined as IStat)
     *     - This is used to calculate ICalculationNode.Value. The value is nullable, allowing the representation
     *       of modifiers that aren't applied, e.g. because of their conditions.
     *   - Conditions are IStats themselves and be referenced the same way from IValue
     *     - Their IStat implementation decides whether the value is user entered or calculated
     *     - User entered conditions can be registered using IExternalStatRegistry
     * - IStat may need to include further modifiers, e.g. for aura modifiers to count towards the aura count,
     *   depending on how they are implemented (i.e. multiple stats without being modified by the same form and value).
     *
     * Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     *
     * Construction of the graph: (see ICalculationGraph)
     * - Needs to make sure events are properly unsubscribed from
     *   (ICalculationNode implements IDisposable for this reason)
     * - When constructing nodes from a Modifier, it can use INodeRepository to access referenced nodes
     * - To allow for removing modifiers, each modifier source has to store its modifiers
     * - Adding "special" modifiers (non-data-driven, e.g. conversion):
     *   - These modifiers are triggered by a special property of IStat
     *   - Implemented in whatever is responsible for creating the stat subgraph
     *   - Mod source based conditions:
     *     - Splits up the form collections
     *     - Behaves like a normal modifier with the above addition
     *     - For the main calculation graph (not preview), it might be useful for breakdowns to always split by source
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
     *       - This can only be decided after the value is calculated the first time. Initially, nodes don't subscribe
     *         to children. They are created in a dirty state anyway.
     *       - When calculating the value, subscribe/unsubscribe to children as required.
     *       - E.g. If a condition to the modifier's application is false and the value is therefore null,
     *         only this condition needs to be subscribed to.
     *     - This allows the entire graph being pre-built and all modifiers having conditions stating that their
     *       corresponding skill tree nodes/items/... must be selected. Though, I don't think that makes sense for
     *       anything except maybe tree nodes. This will probably be useful of the tree generator.
     * - Removing modifiers: The reverse of adding them.
     *
     * UI notes:
     * - The ValueChanged events can easily be used by the UI (transformed to PropertyChanged events in ViewModels)
     * - Values for user specified conditions/stats can be set using modifiers with TotalOverride form
     *   and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop)
     */

    public interface ICalculationNode : IDisposable
    {
        // Core nodes supporting values in the format "MinValue to MaxValue" return (MinValue + MaxValue) / 2 as Value.
        // (this is the case for the building block nodes except Increase, More and TotalOverride)
        // Nodes not supporting min and max values return the same value from all three properties.
        // (this is the case for Increase, More and TotalOverride building block nodes and for modifier nodes)

        double? Value { get; }

        double? MinValue { get; }

        double? MaxValue { get; }

        event EventHandler ValueChanged;
    }
}