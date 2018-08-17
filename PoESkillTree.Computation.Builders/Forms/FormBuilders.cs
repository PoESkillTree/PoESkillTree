using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Utils;

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

        private static IFormBuilder Create(Form form) => new FormBuilder(form, Funcs.Identity);

        private static IFormBuilder CreateNegating(Form form) => new FormBuilder(form, v => v.Multiply(v.Create(-1)));


        private class FormBuilder : ConstantBuilder<IFormBuilder, (Form, ValueConverter)>, IFormBuilder
        {
            public FormBuilder(Form form, ValueConverter valueConverter)
                : base((form, valueConverter))
            {
            }
        }
    }
}