using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Forms;

namespace PoESkillTree.Computation.Data.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for matcher implementations providing direct access to the properties of <see cref="IFormBuilders" />
    /// in addition to the properties provided by <see cref="UsesValueBuilders"/>.
    /// </summary>
    public abstract class UsesFormBuilders : UsesValueBuilders
    {
        protected UsesFormBuilders(IBuilderFactories builderFactories)
            : base(builderFactories)
        {
        }

        private IFormBuilders FormBuilders => BuilderFactories.FormBuilders;

        protected IFormBuilder BaseSet => FormBuilders.BaseSet;
        protected IFormBuilder PercentIncrease => FormBuilders.PercentIncrease;
        protected IFormBuilder PercentMore => FormBuilders.PercentMore;
        protected IFormBuilder BaseAdd => FormBuilders.BaseAdd;
        protected IFormBuilder PercentReduce => FormBuilders.PercentReduce;
        protected IFormBuilder PercentLess => FormBuilders.PercentLess;
        protected IFormBuilder BaseSubtract => FormBuilders.BaseSubtract;
        protected IFormBuilder TotalOverride => FormBuilders.TotalOverride;
        protected IFormBuilder BaseOverride => FormBuilders.BaseOverride;
        protected IFormBuilder MinBaseAdd => FormBuilders.MinBaseAdd;
        protected IFormBuilder MaxBaseAdd => FormBuilders.MaxBaseAdd;
    }
}