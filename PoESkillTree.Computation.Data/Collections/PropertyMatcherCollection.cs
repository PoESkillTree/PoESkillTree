using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class PropertyMatcherCollection : MatcherCollection
    {
        public PropertyMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        public void Add([RegexPattern] string regex)
        {
            Add(regex, ModifierBuilder);
        }

        public void Add([RegexPattern] string regex, IStatBuilder stat)
        {
            Add(regex, ModifierBuilder.WithStat(stat));
        }

        public void Add([RegexPattern] string regex, IStatBuilder stat, ValueFunc converter)
        {
            var builder = ModifierBuilder
                .WithStat(stat)
                .WithValueConverter(converter);
            Add(regex, builder);
        }
    }
}