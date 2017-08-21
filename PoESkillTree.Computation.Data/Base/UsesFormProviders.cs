using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Forms;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesFormProviders : UsesValueProviders
    {
        protected UsesFormProviders(IProviderFactories providerFactories)
            : base(providerFactories)
        {
        }

        private IFormProviderFactory FormProviderFactory => ProviderFactories.FormProviderFactory;

        protected IFormProvider BaseSet => FormProviderFactory.BaseSet;
        protected IFormProvider PercentIncrease => FormProviderFactory.PercentIncrease;
        protected IFormProvider PercentMore => FormProviderFactory.PercentMore;
        protected IFormProvider BaseAdd => FormProviderFactory.BaseAdd;
        protected IFormProvider PercentReduce => FormProviderFactory.PercentReduce;
        protected IFormProvider PercentLess => FormProviderFactory.PercentLess;
        protected IFormProvider BaseSubtract => FormProviderFactory.BaseSubtract;
        protected IFormProvider TotalOverride => FormProviderFactory.TotalOverride;
        protected IFormProvider MinBaseAdd => FormProviderFactory.MinBaseAdd;
        protected IFormProvider MaxBaseAdd => FormProviderFactory.MaxBaseAdd;
        protected IFormProvider MaximumAdd => FormProviderFactory.MaximumAdd;
        protected IFormProvider SetFlag => FormProviderFactory.SetFlag;
        protected IFormProvider Zero => FormProviderFactory.Zero;
        protected IFormProvider Always => FormProviderFactory.Always;
    }
}