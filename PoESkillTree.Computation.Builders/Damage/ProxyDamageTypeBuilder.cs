using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Damage
{
    public class ProxyDamageTypeBuilder : ICoreBuilder<IEnumerable<DamageType>>
    {
        private readonly IDamageTypeBuilder _proxiedBuilder;

        public ProxyDamageTypeBuilder(IDamageTypeBuilder proxiedBuilder) =>
            _proxiedBuilder = proxiedBuilder;

        public ICoreBuilder<IEnumerable<DamageType>> Resolve(ResolveContext context) =>
            new ProxyDamageTypeBuilder((IDamageTypeBuilder) _proxiedBuilder.Resolve(context));

        public IEnumerable<DamageType> Build() => _proxiedBuilder.BuildDamageTypes();
    }
}