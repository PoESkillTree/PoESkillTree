using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /*
       TODO: Complete implementation of ICalculator
       - Usage from Console and/or integration tests (not using Data and Parsing, just example implementation of some builders)
       - Support for multiple paths in stat subgraphs (see "Stat subgraphs")
     */

    /*
     * Stat subgraphs:
     * - Conversion paths
     *   (Conversions and Gains, from Bar to Foo; stats identifier used below: BarFooConversion, BarFooGain)
     *   - Adding a modifier with this stat adds a new path to Foo for each path of Bar
     *     - Afterwards, adding a new path to Bar adds a new one to Foo
     *       (same for removing paths, but Bar's paths probably won't be removed anymore as they are used from Foo)
     *     - Removing the stat again probably won't happen until Foo (and maybe Bar) is removed
     *     - These things could be triggered by adding a `ConversionStats? TriggeredConversions` property
     *       to IStat (with ConversionStats containing From and To IStat properties)
     *
     * Conversion support outside of the calculation graph itself (mostly behaviors):
     * - Behavior of BarFooConversion: applies to Base of Foo (conversion paths only)
     *   Multiply original result by (Total of BarFooConversion + Total of BarFooGain)
     *   and query BarConversion to require its creation (don't use result)
     * - Behavior of BarFooGain: applies to Base of Foo (conversion paths only)
     *   Queries BarFooConversion to lead to its creation and returns original result
     *   (I'm not really satisfied with this solution, but I found no way stack both behaviors in a way that the order
     *    of application does not matter because they stack additively)
     * - Behavior of BarConversion: applies to PathTotal of Bar (all paths)
     *   Multiply original result by Total of BarConversion
     * - The values between converting parent nodes need to be redistributed to achieve a sum of 100%
     *   (with modifiers with a skill gem source having precedence)
     *   - Each modifier to BarFooConversion also has FromBarConversion and FromBarSkillConversion in its stats
     *     - UncappedSubtotal of FromBarSkillConversion has a behavior that sets all PathTotals to 0 that don't come
     *       from skills
     *   - The behavior applying to UncappedSubtotal of BarFooConversion sums all PathTotals with
     *     - no multiplier if FromBarConversion < 1,
     *     - a multiplier of 0 to non-skill PathTotals and 1 / FromBarSkillConversion if FromBarSkillConversion >= 1
     *     - a multiplier of 1 / FromBarConversion otherwise
     *   - The behavior applying to UncappedSubtotal of BarConversion returns Max(1 - FromBarConversion, 0)
     *     (these behaviors can be part of the BarFooConversion and BarConversion stats)
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