using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Forms
{
    public class FormBuilders : IFormBuilders
    {
        public IFormBuilder BaseSet { get; } = new FormBuilder(Form.BaseSet);
        public IFormBuilder BaseAdd { get; } = new FormBuilder(Form.BaseAdd);
        public IFormBuilder BaseSubtract { get; } = new FormBuilder(Form.BaseAdd, v => v.Multiply(-1));
        public IFormBuilder PercentIncrease { get; } = new FormBuilder(Form.Increase);
        public IFormBuilder PercentReduce { get; } = new FormBuilder(Form.Increase, v => v.Multiply(-1));
        public IFormBuilder PercentMore { get; } = new FormBuilder(Form.More);
        public IFormBuilder PercentLess { get; } = new FormBuilder(Form.More, v => v.Multiply(-1));
        public IFormBuilder TotalOverride { get; } = new FormBuilder(Form.TotalOverride);
        public IFormBuilder BaseOverride { get; } = new FormBuilder(Form.BaseOverride);


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