using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Skills
{
    public class SkillBuilder : ISkillBuilder
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreBuilder<SkillDefinition> _coreBuilder;

        public SkillBuilder(IStatFactory statFactory, ICoreBuilder<SkillDefinition> coreBuilder)
        {
            _statFactory = statFactory;
            _coreBuilder = coreBuilder;
        }

        public ISkillBuilder Resolve(ResolveContext context) =>
            new SkillBuilder(_statFactory, _coreBuilder.Resolve(context));

        public IActionBuilder Cast =>
            new ActionBuilder(_statFactory, CoreBuilder.UnaryOperation(_coreBuilder, d => $"{d.SkillName}.Cast"),
                new ModifierSourceEntityBuilder());

        public IStatBuilder Instances =>
            new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<SkillDefinition>(_coreBuilder,
                    (e, d) => _statFactory.FromIdentity($"{d.SkillName}.Instances", e, typeof(int))));

        public ValueBuilder SkillId =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => new Constant(_coreBuilder.Build().NumericId),
                c => Resolve(c).SkillId));
    }
}