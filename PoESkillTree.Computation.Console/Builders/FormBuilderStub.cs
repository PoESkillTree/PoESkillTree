using PoESkillTree.Computation.Parsing.Builders.Forms;

namespace PoESkillTree.Computation.Console.Builders
{
    public class FormBuilderStub : BuilderStub, IFormBuilder
    {
        public FormBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }
    }


    public class FormBuildersStub : IFormBuilders
    {
        public IFormBuilder BaseSet => new FormBuilderStub("Base set");
        public IFormBuilder PercentIncrease => new FormBuilderStub("Percent increase");
        public IFormBuilder PercentMore => new FormBuilderStub("Percent more");
        public IFormBuilder BaseAdd => new FormBuilderStub("Base add");
        public IFormBuilder PercentReduce => new FormBuilderStub("Percent reduce");
        public IFormBuilder PercentLess => new FormBuilderStub("Percent less");
        public IFormBuilder BaseSubtract => new FormBuilderStub("Base subtract");
        public IFormBuilder TotalOverride => new FormBuilderStub("Total override");
        public IFormBuilder MinBaseAdd => new FormBuilderStub("Minimum base add");
        public IFormBuilder MaxBaseAdd => new FormBuilderStub("Maximum base add");
        public IFormBuilder MaximumAdd => new FormBuilderStub("Maximum add");
    }
}