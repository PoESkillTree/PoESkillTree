using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FormMatcherCollection : MatcherCollection
    {
        private readonly IValueProviderFactory _valueFactory;

        public FormMatcherCollection(IMatchBuilder matchBuilder, IValueProviderFactory valueFactory)
            : base(matchBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormProvider form, double? value = null)
        {
            var builder = MatchBuilder.WithForm(form);
            if (value.HasValue)
            {
                builder = builder.WithValue(_valueFactory.Create(value.Value));
            }
            Add(regex, builder);
        }
    }
}