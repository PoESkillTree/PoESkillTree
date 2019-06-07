using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class TransformedNodeModifier
    {
        public TransformedNodeModifier(string modifier, IValueBuilder valueMultiplier)
        {
            Modifier = modifier;
            ValueMultiplier = valueMultiplier;
        }

        public string Modifier { get; }

        public IValueBuilder ValueMultiplier { get; }
    }
}