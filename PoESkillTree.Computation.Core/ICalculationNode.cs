using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /*
       TODO: Complete implementation of ICalculationGraph
       - INodeRepository implementation(s)
         - Need to support different "views": 
           - One returning Adapters (for graph construction and single-pass ICalculationGraph.NodeRepository)
           - One returning CachingNodes (for two-pass ICalculationGraph, both .NodeRepository and to delay events in two-pass updates)
       - Two-pass recalculation class (see "(Re-)Calculating values efficiently")
       - Construction class and ICalculationGraph implementation(s)
         (see "(Re-)Calculating values efficiently" and "Construction of the graph")
       - Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       - Support for multiple paths and other "specialties"/behaviors in stat subgraphs (see "Stat subgraphs")
       (see the thoughts below and the thoughts scattered around in other files for details)
     */

    /*
     * Core interface: ICalculationNode
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
     *   1. SuspendNotifications() is called
     *     - on all ICachingNodes and other API surface events, i.e. those of collections in
     *      ICalculationGraph.NodeRepository and .ExternalStatRegistry
     *   2. Modifiers are added/removed
     *     - Core nodes raise ValueChanged when being changed directly or when a child raises this event
     *     - These events will go through the graph marking nodes as dirty (through CachingNode and CachingNodeAdapter)
     *     - Each CachingNode raises ValueChangeReceived at most once. Therefore each core node receives and raises
     *       ValueChanged at most once per child.
     *   3. ResumeNotifications() is called
     *     - All CachingNodes that received ValueChanged events raise their ValueChanged events
     *
     * Construction of the graph:
     * - Needs to make sure events are properly unsubscribed from
     *   (ICalculationNode implements IDisposable for this reason)
     * - Adding modifiers:
     *   - Call INodeRepository.GetFormNodes(stat, form)
     *   - Create a ValueNode from value
     *   - Add the node to the collection
     * - Removing modifiers:
     *   - The reverse of adding them.
     *   - If a stat is neither referenced nor modified (has empty form collections), its subgraph can be removed
     *     - How to check whether a node is referenced or not? -> ValueChanged.GetInvocationList().Length
     *
     * Stat subgraphs:
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
     *   - When removing a stat subgraph, behaviors caused by it must also be removed
     * - The IStat representing Damage Effectiveness needs to be passed to calculation somehow. The subgraph nodes
     *   can't access it otherwise. Either some interface containing named IStats that is passed by constructor
     *   or done as a behavior, see above.
     *   - It is used as a multiplier to the value of BaseAdd nodes
     * - Each stat can have different rounding behavior. Can be implemented either as an additional property of IStat
     *   or as a behavior, see the ideas above.
     * - Conditions are handled the same way as other stats. They have a stat subgraph and it can be referenced from
     *   value calculations.
     * - User entered conditions/stats must be able to register themselves, i.e. IExternalStatRegistry and IStat must be
     *   connected in some way. This doesn't seem to fit the behavior concept.
     *
     * Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     *
     * UI notes:
     * - The ValueChanged events can easily be used by the UI (transformed to PropertyChanged events in ViewModels)
     * - Values for user specified conditions/stats can be set using modifiers with TotalOverride form
     *   and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop)
     */

    public interface ICalculationNode : IDisposable
    {
        // Gets the node's value based on its child nodes
        // This value is not cached, except for ICachingNodes, in which case it is still calculated lazily.
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}