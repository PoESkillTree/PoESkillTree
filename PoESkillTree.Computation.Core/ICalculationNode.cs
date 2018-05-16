using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /*
     * TODO Conversion support outside of the calculation graph itself (mostly behaviors):
     * - Behavior of BarFooConversion: applies to Base of Foo (conversion paths only)
     *   Multiply original result by (Total of BarFooConversion + Total of BarFooGain)
     *   and query BarConversion to require its creation (don't use result)
     * - Behavior of BarFooGain: applies to Base of Foo (conversion paths only)
     *   Queries BarFooConversion to lead to its creation and returns original result
     *   (I'm not really satisfied with this solution, but I found no way stack both behaviors in a way that the order
     *    of application does not matter because they stack additively)
     * - Behavior of BarConversion: applies to PathTotal of Bar (all paths)
     *   Multiply original result by Total of BarConversion
     * - Another behavior of BarFooConversion makes sure all required paths exist in Foo:
     *   - Applies to UncappedSubtotal of Foo
     *   - Before calculating the value using the decorated IValue, it queries the paths of Bar and then queries
     *     Foo.PathTotal for the respective path of Foo
     *   - This makes sure that a) Foo.UncappedSubtotal is re-calculated every time the paths of Bar change and
     *     b) all required paths exist in Foo
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
     * - Make sure the dependencies between stats are Foo -> BarFooConversion -> Bar, i.e calculating BarFooConversion
     *   does not query values of Foo and calculating Bar does not query values of Foo and BarFooConversion
     *
     * TODO Data-driven Mechanics:
     * - New "CommonGivenStats" data class
     *   (can also contain things common between CharacterGivenStats and MonsterGivenStats)
     */

    public interface ICalculationNode
    {
        // Gets the node's value based on its child nodes. It is calculated lazily.
        NodeValue? Value { get; }

        event EventHandler ValueChanged;
    }
}