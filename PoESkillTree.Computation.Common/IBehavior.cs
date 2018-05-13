using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    public interface IBehavior
    {
        /*
         * "Modifiers to Foo also apply to Bar [at x% of their value]":
         * - Applies to NodeType.Base and PathTotal of Bar.
         * - When those nodes request the value of BaseAdd, Increase or More the respective value of
         *   Foo is multiplied by x/100 and added to the original value.
         *   (some do not apply to Base/BaseAdd)
         * - Each Foo, Bar combination will result in one stat with the respective behavior. The value of that stat is
         *   used as the multiplier.
         *   - These stats are explicitly registered so that UI can add the newly affecting modifiers to its tables
         *   - This explicit registration is handled differently by the UI (they are not stats that need to be set by
         *     users). More specialized registration may be necessary.
         * - Also works for "Modifiers to Spell Damage apply to this Skill's Damage Over Time effect".
         *   "this Skill's" is part of the condition (modifier source Local->Skill), everything else is the same
         * - Affects all paths (if behaviors applying to Base of stats partaking in conversions exist, I don't know
         *   whether they should apply to conversion paths)
         * "Modifiers to Claw Foo also apply to Unarmed":
         * - Affecting form nodes open a whole bunch of new issues, e.g. the same form node can be used in different
         *   stats, so this can't be done as a behavior. Solutions need to be solely in the builder implementations.
         * Effectiveness of Added Damage:
         * - Applies to NodeType.BaseAdd of all damage stats
         * - Values of requested form nodes are multiplied by the effectiveness stat's value
         * - Affects all paths (only non-conversion paths have BaseAdd nodes)
         * Rounding:
         * - Each stat can have different rounding behaviors
         * - This can affect nodes of all NodeTypes
         * - Modifies the output of affected nodes by rounding it.
         * - Affects all paths
         * Default values:
         * - Affects NodeType.BaseSet of the stat
         * - Modifies the output by changing null to the default value if the BaseSet nodes of non-main, non-converted
         *   paths are also null
         * - With this, NodeValueAggregators.CalculateBaseSet() should default to null
         * - Only affects the main path
         */

        IEnumerable<IStat> AffectedStats { get; }
        IEnumerable<NodeType> AffectedNodeTypes { get; }
        BehaviorPathInteraction AffectedPaths { get; }

        IValueTransformation Transformation { get; }
    }


    public enum BehaviorPathInteraction
    {
        AllPaths,
        MainPathOnly,
        ConversionPathsOnly
    }
}