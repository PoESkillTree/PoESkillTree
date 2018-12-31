using System;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders
{
    public abstract class ConstantBuilder<TResolve, TBuild> : IResolvable<TResolve> where TResolve : class
    {
        private readonly TBuild _buildResult;

        protected ConstantBuilder(TBuild buildResult)
        {
            if (!(this is TResolve))
                throw new ArgumentException("Extending class must implement TResolve", nameof(TResolve));
            _buildResult = buildResult;
        }

        public TResolve Resolve(ResolveContext context) => this as TResolve;

        public TBuild Build() => _buildResult;
        public TBuild Build(BuildParameters parameters) => _buildResult;
    }
}