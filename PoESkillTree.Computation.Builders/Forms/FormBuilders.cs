using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Forms
{
    public class FormBuilders : IFormBuilders
    {
        public IFormBuilder BaseSet { get; } = Create(Form.BaseSet);
        public IFormBuilder BaseAdd { get; } = Create(Form.BaseAdd);
        public IFormBuilder BaseSubtract { get; } = CreateNegating(Form.BaseAdd);
        public IFormBuilder PercentIncrease { get; } = Create(Form.Increase);
        public IFormBuilder PercentReduce { get; } = CreateNegating(Form.Increase);
        public IFormBuilder PercentMore { get; } = Create(Form.More);
        public IFormBuilder PercentLess { get; } = CreateNegating(Form.More);
        public IFormBuilder TotalOverride { get; } = Create(Form.TotalOverride);
        public IFormBuilder BaseOverride { get; } = Create(Form.BaseOverride);

        private static IFormBuilder Create(Form form) => new FormBuilder(form);

        private static IFormBuilder CreateNegating(Form form) => new FormBuilder(form, v => v.Multiply(v.Create(-1)));


        private class FormBuilder : IFormBuilder
        {
            private readonly Form _form;
            private readonly ValueConverter _valueConverter;

            public FormBuilder(Form form, ValueConverter valueConverter = null)
            {
                _form = form;
                _valueConverter = valueConverter ?? Funcs.Identity;
            }

            public IFormBuilder Resolve(ResolveContext context) => this;

            public (Form form, ValueConverter valueConverter) Build() => (_form, _valueConverter);
        }
    }
}