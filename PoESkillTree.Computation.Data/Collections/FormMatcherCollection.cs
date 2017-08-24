using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Values;

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

        public void Add([RegexPattern] string regex, IFormBuilder form, double? value = null)
        {
            var builder = ModifierBuilder.WithForm(form);
            if (value.HasValue)
            {
                builder = builder.WithValue(_valueFactory.Create(value.Value));
            }
            Add(regex, builder);
        }
    }
}