using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /*
       TODO: Complete implementation of ICalculator
       - Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       - Support for multiple paths in stat subgraphs (see "Stat subgraphs")
       (see the thoughts below and the thoughts scattered around in other files for details)
     */

    /*
     * Stat subgraphs:
     * - Can contain multiple "paths"
     *   - Conversions and Gains (from Bar to Foo) can add paths:
     *     - Adding a modifier with this stat adds a new path to Foo for each path of Bar
     *     - Conversion rates can be done as behaviors to Base of Foo's new path and all of Bar's UncappedSubtotals
     *     - the values between converting parent nodes need to be redistributed to achieve a sum of 100%
     *       (with modifiers with a skill gem source having precedence)
     *   - Implementation ideas for conversion paths:
     *     - IStat has a method an interface is passed to. That interface has one method for each kind of path,
     *       which is called by the method it is passed to.
     *     - IStat has a subclass for each special stat and has a "void Visit(IStatVisitor)" method. Subclasses
     *       implement it by calling "IStatVisitor.Accept(this)". IStatVisitor has a method for each subclass.
     *       The subclasses need to have properties specifying where to add the path. E.g. the source and target IStat
     *       for conversions.
     *     - A more general solution, which does not fix all types of path in interfaces, would be better.
     * -> Support needs to be implemented for:s
     *    - Interaction with behaviors might make things more difficult
     *      (behaviors also need to apply to nodes on the non-main paths)
     *    - Specifying conversion/gain paths in IStat
     *
     * Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     *
     * UI notes:
     * - The ValueChanged events can easily be used by the UI (transformed to PropertyChanged events in ViewModels)
     *   - They (and other events exposed by ICalculator) are only raised at the end of updates.
     * - ICalculator can be used pull- or push-based: Call Value on interesting nodes yourself after Update or
     *   only call it on nodes you are subscribed to that raise events.
     *   - For pull-based usage: If it leads to performance improvements, ICalculator could have a property to
     *     disable it calling SuspendEvents/ResumeEvents in Update. (sending events doesn't take time if no one is
     *     subscribed, but calling SuspendEvents/ResumeEvents on all nodes may take too long)
     * - Values for user specified conditions/stats can be set using modifiers with TotalOverride form
     *   and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop).
     *   IStat.DataType, .Minimum and .Maximum can be used to determine how to display the input field.
     */

    public interface ICalculationNode
    {
        // Gets the node's value based on its child nodes. It is calculated lazily.
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}