using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class TransformedNodeModifier
    {
        public TransformedNodeModifier(string modifier, IConditionBuilder condition, IValue valueMultiplier)
        {
            Modifier = modifier;
            Condition = condition;
            ValueMultiplier = valueMultiplier;
        }

        public string Modifier { get; }

        public IConditionBuilder Condition { get; }

        public IValue ValueMultiplier { get; }
    }
}