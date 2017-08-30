using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ValueConversionMatcherCollection : MatcherCollection
    {
        public ValueConversionMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        public void Add([RegexPattern] string regex, ValueFunc func)
        {
            Add(regex, ModifierBuilder.WithValueConverter(func));
        }

        public void Add([RegexPattern] string regex, ValueBuilder multiplier)
        {
            Add(regex, v => v * multiplier);
        }
    }
}