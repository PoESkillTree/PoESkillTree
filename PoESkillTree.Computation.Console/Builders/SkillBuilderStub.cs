using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class SkillBuilderStub : BuilderStub, ISkillBuilder
    {
        private readonly Resolver<ISkillBuilder> _resolver;

        public SkillBuilderStub(string stringRepresentation, Resolver<ISkillBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private ISkillBuilder This => this;

        public IActionBuilder Cast =>
            Create<IActionBuilder, ISkillBuilder>(
                ActionBuilderStub.BySelf,
                This, o => $"{o} cast");

        public IStatBuilder Instances =>
            CreateStat(This, o => $"{o} instance count");

        public ValueBuilder SkillId =>
            new ValueBuilder(CreateValue(This, o => $"{o}.SkillId"));

        public ISkillBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}