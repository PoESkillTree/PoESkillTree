using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IFormProvider
    {

    }

    public static class FormProviders
    {
        public static readonly IFormProvider PercentIncrease;
        public static readonly IFormProvider PercentMore;
        public static readonly IFormProvider PercentRegen;
        public static readonly IFormProvider BaseAdd;
        public static readonly IFormProvider BaseSubtract;
        public static readonly IFormProvider BaseOverride;
        public static readonly IFormProvider MinBaseAdd;
        public static readonly IFormProvider MaxBaseAdd;

        public static IFormProvider ValueDependent(params IFormProvider[] formPerValueIndex)
        {
            throw new NotImplementedException();
        }
    }
}