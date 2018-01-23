using System;

namespace PoESkillTree.Computation.Core
{
    /*
       TODO: Complete implementation of ICalculationGraph
       - INodeRepository implementation(s)
         - Need to support different "views": 
           - One returning Adapters (for graph construction and single-pass-API)
           - One returning CachingNodes (for two-pass-API)
       - ICalculationNode implementations for modifiers (using IValue to calculate a value)
       - Two-pass recalculation class
       - ICalculationGraph implementation(s) (mainly Update(), the properties should be trivial)
       - Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       - Add support for multiple paths and other "specialties" to stat subgraphs
       (see the thoughts below and the thoughts scattered around in other files for details)
     */

    /*
     * Core interface: ICalculationNode
     * - Value gets the node's value based on its child nodes
     *   - For nodes not directly used by other projects and not implementing ICachingNode, this is not cached,
     *     i.e. it calculates the value based on the children with every call
     *   - Even for ICachingNodes, the value is calculated lazily, i.e. not when ValueChanged is raised
     * - Will have many different implementations depending on how the value is calculated
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
     *
     * Stat subgraphs:
     * - For details on the main nodes, see NodeType.cs
     * - Per default, a path for each source (Global, and each Local source that has modifiers) is created.
     *   (using unconverted Base values)
     *   - The IModifierSource that was passed to ICalculationGraph.Update() together with the Modifier
     *     decides the form collection the node created from the Modifier is added to
     *     - Modifiers with a mod source condition overwrite the IModifierSource passed with them
     *     - Where that mod source condition is stored is not yet determined (either as property of IStat or Modifier)
     *       (downside of being stored in IStat: contradicts the equality concept of IStat)
     *   - Increase and More nodes on all paths link to the respective form collection of the Global path
     * - Some stats require being subscribed to from other stat's subgraphs before modifiers to them exist:
     *   (or they need to be added to other subgraph automatically)
     *   - "Modifiers to Bar also apply to Foo":
     *     - Adding a modifier with this stat leads to the BaseAdd, Increase and More nodes (of all paths) of Foo
     *       also subscribing to the respective IFormNodeCollection of Bar
     *     - The stat's total value is used as multiplier
     *   - Conversions and Gains (from Bar to Foo):
     *     - Adding a modifier with this stat leads to the UncappedSubtotal node of Foo adding a node path for each
     *       path of Bar
     *     - and to the Base nodes of Bar adding a parent node for Foo
     *     - the values between converting parent nodes need to be redistributed to achieve a sum of 100%
     *      (with modifiers with a skill gem source having precedence)
     *   This must be implemented by adding something to IStat.
     *   Ideas:
     *   - Stat subgraphs subscribe to collections in INodeRepository that contain the special stats (they are empty
     *     initially). Adding special modifiers leads to their stats being added to the respective collection, in
     *     addition to the normal behavior.
     *     - IStat has a method an interface is passed to. That interface has one method for each special stat,
     *       which is called by the method it is passed to.
     *     - IStat has a subclass for each special stat and has a "void Visit(IStatVisitor)" method. Subclasses
     *       implement it by calling "IStatVisitor.Accept(this)". IStatVisitor has a method for each subclass.
     *       The subclasses need to have properties describing the special stat. E.g. the source and target IStat
     *       for conversions.
     *     - IStat has a method/property returning a collection of "IBehavior"s. IBehavior has methods to check whether
     *       it affects a node (by IStat and NodeType, probably) and a method specifying how it affects nodes.
     *   - Nodes of stat subgraphs support a "customizable" concept. IStats can define the affected stat subgraphs
     *     and are then run over all nodes to customize them. This can be implemented using one of the ideas
     *     from above.
     *   - All types of additional behavior when adding modifiers to a stat for the first time can be modeled in this
     *     way. E.g. registering stats in IExternalStatRegistry and registering named IStats (see below).
     * - The IStat representing Damage Effectiveness needs to be passed to calculation somehow. The subgraph nodes
     *   can't access it otherwise. Either some interface containing named IStats that is passed by constructor
     *   or done as a behavior, see above.
     *   - It is used as a multiplier to the value of BaseAdd nodes
     * - Each stat can have different rounding behavior. Can be implemented either as an additional property of IStat
     *   or as a behavior, see the ideas above.
     *
     * UI notes:
     * - The ValueChanged events can easily be used by the UI (transformed to PropertyChanged events in ViewModels)
     * - Values for user specified conditions/stats can be set using modifiers with TotalOverride form
     *   and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop)
     */

    public interface ICalculationNode : IDisposable
    {
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}