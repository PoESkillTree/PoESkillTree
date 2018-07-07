using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Actions
{
    public class ActionBuilders : IActionBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly IEntityBuilder _entity = new ModifierSourceEntityBuilder();

        public ActionBuilders(IStatFactory statFactory)
        {
            _statFactory = statFactory;
        }

        public IActionBuilder Kill => Create("Kill");

        public IBlockActionBuilder Block => new BlockActionBuilder(_statFactory, _entity);

        public IActionBuilder Hit => Create("Hit");

        public IActionBuilder HitWith(IDamageTypeBuilder damageType)
        {
            var stringBuilder = new ParametrisedStringBuilder<IKeywordBuilder>(damageType, BuildHitWithIdentity);
            return new ActionBuilder(_statFactory, stringBuilder, _entity);
        }

        private static string BuildHitWithIdentity(IKeywordBuilder builder)
        {
            var damageTypes = ((IDamageTypeBuilder) builder).BuildDamageTypes();
            if (damageTypes.Count != 1)
                throw new ParseException(
                    $"IDamageTypeBuilders passed to {nameof(HitWith)} must build to exactly one damage type." +
                    $" {string.Join(",", damageTypes)} given");
            return damageTypes.Single() + "Hit";
        }

        public IActionBuilder SavageHit => Create("SavageHit");

        public ICriticalStrikeActionBuilder CriticalStrike => new CriticalStrikeActionBuilder(_statFactory, _entity);

        public IActionBuilder NonCriticalStrike => Create("NonCriticalStrike");

        public IActionBuilder Shatter => Create("Shatter");

        public IActionBuilder ConsumeCorpse => Create("ConsumeCorpse");

        public IActionBuilder Stun => Create("Stun");

        public IActionBuilder SpendMana(IValueBuilder amount)
        {
            var stringBuilder = new ParametrisedStringBuilder<IValueBuilder>(amount, BuildSpendManaIdentity);
            return new ActionBuilder(_statFactory, stringBuilder, _entity);
        }

        private static string BuildSpendManaIdentity(IValueBuilder builder) =>
            $"Spend{builder.Build(default).Calculate(new ThrowingContext())}Mana";

        public IActionBuilder Unique(string description) => Create(description);

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds)
        {
            throw new System.NotImplementedException();
        }

        private IActionBuilder Create(string identity) =>
            new ActionBuilder(_statFactory, new ConstantStringBuilder(identity), _entity);

        private class ThrowingContext : IValueCalculationContext
        {
            public IEnumerable<PathDefinition> GetPaths(IStat stat) =>
                throw CreateException();

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                throw CreateException();

            public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                throw CreateException();

            private static ParseException CreateException() =>
                new ParseException(
                    $"Value builders passed to {nameof(SpendMana)} must not use the IValueCalculationContext");
        }
    }
}