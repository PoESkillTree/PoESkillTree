using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Actions
{
    public class ProxyActionBuilder : ICoreBuilder<string>
    {
        private readonly IActionBuilder _proxiedBuilder;

        public ProxyActionBuilder(IActionBuilder proxiedBuilder) =>
            _proxiedBuilder = proxiedBuilder;

        public ICoreBuilder<string> Resolve(ResolveContext context) =>
            new ProxyActionBuilder(_proxiedBuilder.Resolve(context));

        public string Build() => _proxiedBuilder.Build();
    }
}