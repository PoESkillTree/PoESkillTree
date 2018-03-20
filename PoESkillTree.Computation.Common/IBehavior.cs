using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    public interface IBehavior
    {
        /*
         * "Modifiers to Foo also apply to Bar [at x% of their value]":
         * - Applies to NodeType.Base and UncappedSubtotal of Bar.
         * - When those nodes request the value of BaseAdd, Increase or More the respective value of
         *   Foo is multiplied by x/100 and added to the original value.
         *   (some do not apply to Base/BaseAdd)
         * - Requires NodeType.Increase's "1 + " to be moved to UncappedSubtotal
         * - Each Foo, Bar combination will result in one stat with the respective behavior. The value of that stat is
         *   used as the multiplier.
         *   - These stats are explicitly registered so that UI can add the newly affecting modifiers to its tables
         *   - This explicit registration is handled differently by the UI (they are not stats that need to be set by
         *     users). More specialized registration may be necessary.
         * - Also works for "Modifiers to Spell Damage apply to this Skill's Damage Over Time effect".
         *   "this Skill's" is part of the condition, everything else is the same (or not, depending on how the
         *   non-skill DoTs, Ignite and Poison, are implemented)
         * "Modifiers to Claw Foo also apply to Unarmed":
         * - Affecting form nodes open a whole bunch of new issues, e.g. the same form node can be used in different
         *   stats, so this can't be done as a behavior. Solutions need to be solely in the builder implementations.
         * Effectiveness of Added Damage:
         * - Applies to NodeType.BaseAdd of all damage stats
         * - Values of requested form nodes are multiplied by the effectiveness stat's value
         * Rounding:
         * - Each stat can have different rounding behaviors
         * - This can affect nodes of all NodeTypes
         * - Modifies the output of affected nodes by replacing the calculating IValue with another one that can use
         *   the original value.
         * Default values:
         * - Affects NodeType.BaseSet of the stat
         * - Modifies the output by changing null to the default value
         * - With this, NodeValueAggregators.CalculateBaseSet() should default to null
         *
         * => Behaviors affect stat subgraph nodes (IStat, NodeType), overwrite IValueCalculationContext or IValue
         */

        IEnumerable<IStat> AffectedStats { get; }
        IEnumerable<NodeType> AffectedNodeTypes { get; }

        IValueTransformation Transformation { get; }
    }
}