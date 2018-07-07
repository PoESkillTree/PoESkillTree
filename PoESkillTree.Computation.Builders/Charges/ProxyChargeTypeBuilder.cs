using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ProxyChargeTypeBuilder : ICoreBuilder<ChargeType>
    {
        private readonly IChargeTypeBuilder _proxiedBuilder;

        public ProxyChargeTypeBuilder(IChargeTypeBuilder proxiedBuilder) =>
            _proxiedBuilder = proxiedBuilder;

        public ICoreBuilder<ChargeType> Resolve(ResolveContext context) =>
            new ProxyChargeTypeBuilder(_proxiedBuilder.Resolve(context));

        public ChargeType Build() => _proxiedBuilder.Build();
    }
}