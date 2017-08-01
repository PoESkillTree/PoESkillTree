using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Forms;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesFormProviders : UsesValueProviders
    {
        private readonly IFormProviderFactory _formProviderFactory;

        protected UsesFormProviders(IProviderFactories providerFactories)
            : base(providerFactories)
        {
            _formProviderFactory = providerFactories.FormProviderFactory;
        }

        protected IFormProvider BaseSet => _formProviderFactory.BaseSet;
        protected IFormProvider PercentIncrease => _formProviderFactory.PercentIncrease;
        protected IFormProvider PercentMore => _formProviderFactory.PercentMore;
        protected IFormProvider BaseAdd => _formProviderFactory.BaseAdd;
        protected IFormProvider PercentReduce => _formProviderFactory.PercentReduce;
        protected IFormProvider PercentLess => _formProviderFactory.PercentLess;
        protected IFormProvider BaseSubtract => _formProviderFactory.BaseSubtract;
        protected IFormProvider TotalOverride => _formProviderFactory.TotalOverride;
        protected IFormProvider MinBaseAdd => _formProviderFactory.MinBaseAdd;
        protected IFormProvider MaxBaseAdd => _formProviderFactory.MaxBaseAdd;
        protected IFormProvider MaximumAdd => _formProviderFactory.MaximumAdd;
        protected IFormProvider SetFlag => _formProviderFactory.SetFlag;
        protected IFormProvider Zero => _formProviderFactory.Zero;
        protected IFormProvider Always => _formProviderFactory.Always;
    }
}