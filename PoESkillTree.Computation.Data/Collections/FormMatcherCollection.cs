using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FormMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public FormMatcherCollection(IModifierBuilder modifierBuilder, IValueBuilders valueFactory)
            : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, double value)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(_valueFactory.Create(value));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value);
            Add(regex, builder);
        }
    }
}