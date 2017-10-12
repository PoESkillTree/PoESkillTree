using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Forms;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesFormProviders : UsesValueProviders
    {
        protected UsesFormProviders(IBuilderFactories builderFactories)
            : base(builderFactories)
        {
        }

        public virtual IReadOnlyList<string> ReferenceNames { get; } = new string[0];

        private IFormBuilders FormBuilders => BuilderFactories.FormBuilders;

        protected IFormBuilder BaseSet => FormBuilders.BaseSet;
        protected IFormBuilder PercentIncrease => FormBuilders.PercentIncrease;
        protected IFormBuilder PercentMore => FormBuilders.PercentMore;
        protected IFormBuilder BaseAdd => FormBuilders.BaseAdd;
        protected IFormBuilder PercentReduce => FormBuilders.PercentReduce;
        protected IFormBuilder PercentLess => FormBuilders.PercentLess;
        protected IFormBuilder BaseSubtract => FormBuilders.BaseSubtract;
        protected IFormBuilder TotalOverride => FormBuilders.TotalOverride;
        protected IFormBuilder MinBaseAdd => FormBuilders.MinBaseAdd;
        protected IFormBuilder MaxBaseAdd => FormBuilders.MaxBaseAdd;
        protected IFormBuilder MaximumAdd => FormBuilders.MaximumAdd;
    }
}