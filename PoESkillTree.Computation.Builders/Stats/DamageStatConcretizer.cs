using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class DamageStatConcretizer
    {
        private readonly IStatFactory _statFactory;

        public DamageStatConcretizer(IStatFactory statFactory) => _statFactory = statFactory;

        public IEnumerable<StatBuilderResult> Concretize(StatBuilderResult result)
        {
            yield return ConcretizeToAttackDamage(result);
            foreach (var damageSource in Enums.GetValues<DamageSource>().Except(DamageSource.Attack))
            {
                yield return ConcretizeToSkillDamage(result, damageSource);
            }
            foreach (var ailment in Enums.GetValues<Ailment>())
            {
                yield return ConcretizeToAilmentDamage(result, ailment);
            }
        }

        private StatBuilderResult ConcretizeToAttackDamage(StatBuilderResult result)
        {
            var specs = Enums.GetValues<AttackDamageHand>()
                .Select(attackDamageHand => new AttackDamageSpecification(attackDamageHand));
            return ConcretizeDamage(result, specs);
        }

        private StatBuilderResult ConcretizeToSkillDamage(StatBuilderResult result, DamageSource damageSource) =>
            ConcretizeDamage(result, new SkillDamageSpecification(damageSource));

        private StatBuilderResult ConcretizeToAilmentDamage(StatBuilderResult result, Ailment ailment) =>
            ConcretizeDamage(result, new AilmentDamageSpecification(ailment));

        private StatBuilderResult ConcretizeDamage(StatBuilderResult result, IDamageSpecification spec) =>
            ConcretizeDamage(result, new[] { spec });

        private StatBuilderResult ConcretizeDamage(StatBuilderResult result, IEnumerable<IDamageSpecification> specs)
        {
            specs = specs.ToList();
            var stats = result.Stats.SelectMany(SelectStats).ToList();
            return new StatBuilderResult(stats, result.ModifierSource, result.ValueConverter);

            IEnumerable<IStat> SelectStats(IStat stat) =>
                specs.Select(spec => _statFactory.ConcretizeDamage(stat, spec));
        }
    }
}