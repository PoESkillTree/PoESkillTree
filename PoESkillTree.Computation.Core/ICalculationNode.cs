using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /*
       TODO: Complete implementation of ICalculator
       - Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       - Support for multiple paths and other "specialties"/behaviors in stat subgraphs (see "Stat subgraphs")
       (see the thoughts below and the thoughts scattered around in other files for details)
     */

    /*
     * Stat subgraphs:
     * - Can contain multiple "paths"
     *   - Per default, a path for each source (Global, and each Local source that has modifiers) is created.
     *     - Add a IModifierSource property to Modifier. After parsing, the actual IModifierSource is set, if parsing did
     *       not already determine a mod source condition (e.g. "from equipped shield" sets IModifierSource to Local->Item->OffHand)
     *     - Increase and More nodes on all paths link to the respective form collection of the Global path
     *     - Paths are children of the main UncappedSubtotal node
     *     - These paths use unconverted Base values
     *   - Conversions and Gains (from Bar to Foo) can add paths:
     *     - Adding a modifier with this stat adds a new path to Foo for each path of Bar
     *     - and adds a parent node for Foo to the Base nodes of Bar
     *     - the values between converting parent nodes need to be redistributed to achieve a sum of 100%
     *       (with modifiers with a skill gem source having precedence)
     *   - Implementation ideas:
     *     - IStat has a method an interface is passed to. That interface has one method for kind of path,
     *       which is called by the method it is passed to.
     *     - IStat has a subclass for each special stat and has a "void Visit(IStatVisitor)" method. Subclasses
     *       implement it by calling "IStatVisitor.Accept(this)". IStatVisitor has a method for each subclass.
     *       The subclasses need to have properties specifying where to add the path. E.g. the source and target IStat
     *       for conversions.
     *     - A more general solution, which does not fix all types of path in interfaces, would better. Similar to
     *       the idea for behaviors.
     *     - Interaction with behaviors might make things more difficult
     * -> Support needs to be implemented for:
     *    - A general concept of paths
     *    - Separating paths by Modifier.Source (which also does not yet exist) by default
     *    - Specifying further paths in IStat, mostly conversion/gains but maybe more (e.g. the different DoT from ailment types)
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
     *   and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop)
     * - A "DataType"/"ValueType" property would probably be useful in IStat. Without it, the UI can't decide how to
     *   display values or how to allow changing their value (in case of external stats).
     *   This would mostly be relevant for boolean stats. Maybe things like ranges could also be specified.
     */

    public interface ICalculationNode
    {
        // Gets the node's value based on its child nodes. It is calculated lazily.
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}