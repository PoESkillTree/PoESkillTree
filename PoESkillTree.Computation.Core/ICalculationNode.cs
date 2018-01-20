using System;

namespace PoESkillTree.Computation.Core
{
    /*
       Overall plan: Complete implementation of ICalculationGraph
       1. ICalculationNode implementations for building block
       2. INodeRepository implementation(s)
         - Need to support different "views": 
           - One returning Adapters (for graph construction and single-pass-API)
           - One returning CachingNodes (for two-pass-API)
       3. ICalculationNode implementations for modifiers (using IValue to calculate a value)
       4. Two-pass recalculation class
       5. ICalculationGraph implementation(s) (mainly Update(), the properties should be trivial)
       6. or 7. Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       7. or 6. Add support for multiple paths to the stat subgraphs 
       (see the thoughts below and the thoughts scattered around in other files for details)
     */

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
     *   - Conditions are IStats themselves and can be referenced the same way from IValue
     *     - Their IStat implementation decides whether the value is user entered or calculated
     *     - User entered conditions can be registered using IExternalStatRegistry
     * - Mod source based conditions:
     *
     * Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     *
     * Construction of the graph: (see ICalculationGraph)
     * - Needs to make sure events are properly unsubscribed from
     *   (ICalculationNode implements IDisposable for this reason)
     * - When constructing nodes from a Modifier, it can use INodeRepository to access referenced nodes
     * - To allow removing modifiers, each modifier source has to store its modifiers
     * - Adding modifiers:
     *   - Get the subgraph for each stat (or create them)
     *   - Select the collection for the form
     *   - Create the rooted DAG for the modifier from the value
     *   - Add the root node to the collection
     *   This implies the following
     *   - The whole subgraph for a stat is built when creating it.
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
     * - For details on the subgraph of a stat, see NodeType.cs
     * - Stat subgraphs
     *   - For the nodes in them, see NodeType.cs
     *   - Per default, a path for each source (Global, and each Local source that has modifiers) is created.
     *     (using unconverted Base values)
     *     - The IModifierSource that was passed to ICalculationGraph.Update() together with the Modifier
     *       decides the form collection the node created from the Modifier is added to
     *       - Modifiers with a mod source condition overwrite the IModifierSource passed with them
     *       - Where that mod source condition is stored is not yet determined (either as property of IStat or Modifier)
     *         (downside of being stored in IStat: contradicts the equality concept of IStat)
     *     - Increase and More nodes on all paths link to the respective form collection of the Global path
     *   - Some stats require being subscribed to from other stat's subgraphs before modifiers to them exist:
     *     - "Modifiers to Bar also apply to Foo":
     *       - Adding a modifier with this stat leads to the BaseAdd, Increase and More nodes (of all paths) of Foo
     *         also subscribing to the respective IFormNodeCollection of Bar
     *       - The stat's total value is used as multiplier
     *     - Conversions and Gains (from Bar to Foo):
     *       - Adding a modifier with this stat leads to the UncappedSubtotal node of Foo adding a node path for each
     *         path of Bar
     *       - and to the Base nodes of Bar adding a parent node for Foo
     *       - the values between converting parent nodes need to be redistributed to achieve a sum of 100%
     *        (with modifiers with a skill gem source having precedence)
     *     These subscriptions can be provided similar to other collections in INodeRepository
     *     How do stats end up in these special collections? There needs to be information in IStat to decide that.
     *     - Without these special stats, IStat doesn't need properties and is completely usable just by having
     *       something that can be referenced because it can be compared properly.
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