using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Buffs;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.Skills;

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
            new ActionBuilder(_statFactory, CoreBuilder.UnaryOperation(_coreBuilder, d => $"{d.Id}.Cast"),
                new ModifierSourceEntityBuilder());

        public IStatBuilder Instances => CreateStatBuilder(typeof(int));

        public IStatBuilder Reservation => CreateStatBuilder(typeof(int));

        public IStatBuilder ReservationPool => CreateStatBuilder(typeof(Pool));

        private IStatBuilder CreateStatBuilder(Type dataType, [CallerMemberName] string identitySuffix = null)
            => new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<SkillDefinition>(_coreBuilder,
                (e, d) => _statFactory.FromIdentity($"{d.Id}.{identitySuffix}", e, dataType)));

        public ValueBuilder SkillId =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => new Constant(_coreBuilder.Build(ps).NumericId),
                c => Resolve(c).SkillId));

        public IBuffBuilder Buff
            => new BuffBuilder(_statFactory, CoreBuilder.UnaryOperation(_coreBuilder, SelectBuffIdentity));

        private static string SelectBuffIdentity(SkillDefinition skillDefinition)
        {
            if (skillDefinition.IsSupport || !skillDefinition.ActiveSkill.ProvidesBuff)
                throw new ParseException($"{skillDefinition.Id} does not provide a buff");
            return skillDefinition.Id;
        }

        public SkillDefinition Build(BuildParameters parameters) => _coreBuilder.Build(parameters);
    }
}