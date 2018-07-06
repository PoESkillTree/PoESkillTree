using System;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Actions
{
    public interface IStringBuilder : IResolvable<IStringBuilder>
    {
        string Build();
    }

    public class ConstantStringBuilder : IStringBuilder
    {
        private readonly string _string;

        public ConstantStringBuilder(string s) => _string = s;

        public IStringBuilder Resolve(ResolveContext context) => this;

        public string Build() => _string;
    }

    public class ParametrisedStringBuilder<TParameter> : IStringBuilder
        where TParameter : IResolvable<TParameter>
    {
        private readonly TParameter _parameter;
        private readonly Func<TParameter, string> _build;

        public ParametrisedStringBuilder(TParameter parameter, Func<TParameter, string> build)
        {
            _parameter = parameter;
            _build = build;
        }

        public IStringBuilder Resolve(ResolveContext context) =>
            new ParametrisedStringBuilder<TParameter>(_parameter.Resolve(context), _build);

        public string Build() => _build(_parameter);
    }
}