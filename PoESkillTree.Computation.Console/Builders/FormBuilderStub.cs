using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Console.Builders
{
    public class FormBuilderStub : BuilderStub, IFormBuilder
    {
        private readonly Resolver<IFormBuilder> _resolver;

        public FormBuilderStub(string stringRepresentation, Resolver<IFormBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IFormBuilder Resolve(ResolveContext context) => _resolver(this, context);
    }


    public class FormBuildersStub : IFormBuilders
    {
        private static IFormBuilder Create(string s) => new FormBuilderStub(s, (c, _) => c);

        public IFormBuilder BaseSet => Create("Base set");
        public IFormBuilder PercentIncrease => Create("Percent increase");
        public IFormBuilder PercentMore => Create("Percent more");
        public IFormBuilder BaseAdd => Create("Base add");
        public IFormBuilder PercentReduce => Create("Percent reduce");
        public IFormBuilder PercentLess => Create("Percent less");
        public IFormBuilder BaseSubtract => Create("Base subtract");
        public IFormBuilder TotalOverride => Create("Total override");
        public IFormBuilder BaseOverride => Create("Base override");
        public IFormBuilder MinBaseAdd => Create("Minimum base add");
        public IFormBuilder MaxBaseAdd => Create("Maximum base add");
    }
}