using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Console.Builders
{


    public class FormBuildersStub : IFormBuilders
    {
        private static IFormBuilder Create(string s, Form form, ValueConverter valueConverter = null)
            => new FormBuilderStub(s, form, valueConverter);

        private static IFormBuilder CreateNegated(string s, Form form)
            => new FormBuilderStub(s, form, v => v.Multiply(-1));

        public IFormBuilder BaseSet => Create("Base set", Form.BaseSet);
        public IFormBuilder PercentIncrease => Create("Percent increase", Form.Increase);
        public IFormBuilder PercentMore => Create("Percent more", Form.More);
        public IFormBuilder BaseAdd => Create("Base add", Form.BaseAdd);
        public IFormBuilder PercentReduce => CreateNegated("Percent reduce", Form.Increase);
        public IFormBuilder PercentLess => CreateNegated("Percent less", Form.More);
        public IFormBuilder BaseSubtract => CreateNegated("Base subtract", Form.BaseAdd);
        public IFormBuilder TotalOverride => Create("Total override", Form.TotalOverride);
        public IFormBuilder BaseOverride => Create("Base override", Form.BaseOverride);
        public IFormBuilder MinBaseAdd => Create("Minimum base add", Form.BaseAdd, v => v.MinimumOnly);
        public IFormBuilder MaxBaseAdd => Create("Maximum base add", Form.BaseAdd, v => v.MaximumOnly);


        private class FormBuilderStub : BuilderStub, IFormBuilder
        {
            private readonly Form _form;
            private readonly ValueConverter _valueConverter;

            public FormBuilderStub(string stringRepresentation, Form form, ValueConverter valueConverter = null)
                : base(stringRepresentation)
            {
                _form = form;
                _valueConverter = valueConverter ?? Funcs.Identity;
            }

            public IFormBuilder Resolve(ResolveContext context) => this;

            public (Form form, ValueConverter valueConverter) Build() => (_form, _valueConverter);
        }
    }
}