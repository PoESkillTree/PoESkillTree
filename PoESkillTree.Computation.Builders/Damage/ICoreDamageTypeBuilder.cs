using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Damage
{
    public interface ICoreDamageTypeBuilder : IResolvable<ICoreDamageTypeBuilder>
    {
        IEnumerable<DamageType> Build();
    }

    internal class LeafDamageTypeBuilder : ICoreDamageTypeBuilder
    {
        private readonly IEnumerable<DamageType> _damageTypes;

        public LeafDamageTypeBuilder(params DamageType[] damageTypes) =>
            _damageTypes = damageTypes;

        public ICoreDamageTypeBuilder Resolve(ResolveContext context) => this;

        public IEnumerable<DamageType> Build() => _damageTypes;
    }

    public class ProxyDamageTypeBuilder : ICoreDamageTypeBuilder
    {
        private readonly IDamageTypeBuilder _proxiedBuilder;

        public ProxyDamageTypeBuilder(IDamageTypeBuilder proxiedBuilder) =>
            _proxiedBuilder = proxiedBuilder;

        public ICoreDamageTypeBuilder Resolve(ResolveContext context) =>
            new ProxyDamageTypeBuilder((IDamageTypeBuilder) _proxiedBuilder.Resolve(context));

        public IEnumerable<DamageType> Build() => _proxiedBuilder.BuildDamageTypes();
    }

    internal class AndDamageTypeBuilder : ICoreDamageTypeBuilder
    {
        private readonly ICoreDamageTypeBuilder _left;
        private readonly ICoreDamageTypeBuilder _right;

        public AndDamageTypeBuilder(ICoreDamageTypeBuilder left, ICoreDamageTypeBuilder right)
        {
            _left = left;
            _right = right;
        }

        public ICoreDamageTypeBuilder Resolve(ResolveContext context) =>
            new AndDamageTypeBuilder(_left.Resolve(context), _right.Resolve(context));

        public IEnumerable<DamageType> Build() =>
            _left.Build().Union(_right.Build());
    }

    internal class InvertDamageTypeBuilder : ICoreDamageTypeBuilder
    {
        private static readonly IReadOnlyList<DamageType> NonRandomDamageTypes =
            Enums.GetValues<DamageType>().Except(DamageType.RandomElement).ToList();

        private readonly ICoreDamageTypeBuilder _inner;

        public InvertDamageTypeBuilder(ICoreDamageTypeBuilder inner) => _inner = inner;

        public ICoreDamageTypeBuilder Resolve(ResolveContext context) =>
            new InvertDamageTypeBuilder(_inner.Resolve(context));

        public IEnumerable<DamageType> Build() =>
            NonRandomDamageTypes.Except(_inner.Build());
    }

    internal class ExceptDamageTypeBuilder : ICoreDamageTypeBuilder
    {
        private readonly ICoreDamageTypeBuilder _left;
        private readonly ICoreDamageTypeBuilder _right;

        public ExceptDamageTypeBuilder(ICoreDamageTypeBuilder left, ICoreDamageTypeBuilder right)
        {
            _left = left;
            _right = right;
        }

        public ICoreDamageTypeBuilder Resolve(ResolveContext context) =>
            new ExceptDamageTypeBuilder(_left.Resolve(context), _right.Resolve(context));

        public IEnumerable<DamageType> Build() =>
            _left.Build().Except(_right.Build());
    }
}