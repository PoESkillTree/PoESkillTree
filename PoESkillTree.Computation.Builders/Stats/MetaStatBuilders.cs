using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class MetaStatBuilders : StatBuildersBase, IMetaStatBuilders
    {
        public MetaStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public ValueBuilder RegenTargetPoolValue(Pool sourcePool) =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => BuildTargetPoolValue(ps, StatFactory.RegenTargetPool(ps.ModifierSourceEntity, sourcePool)),
                _ => RegenTargetPoolValue(sourcePool)));

        public ValueBuilder LeechTargetPoolValue(Pool sourcePool) =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => BuildTargetPoolValue(ps, StatFactory.LeechTargetPool(ps.ModifierSourceEntity, sourcePool)),
                _ => LeechTargetPoolValue(sourcePool)));

        private IValue BuildTargetPoolValue(BuildParameters parameters, IStat targetPoolStat)
        {
            var entity = parameters.ModifierSourceEntity;
            var targetPoolValue = new StatValue(targetPoolStat);
            return new FunctionalValue(
                c => c.GetValue(TargetPoolValueStat(targetPoolValue.Calculate(c))),
                $"Value of Pool {targetPoolValue}");

            IStat TargetPoolValueStat(NodeValue? targetPool)
            {
                var targetPoolString = ((Pool) targetPool.Single()).ToString();
                return StatFactory.FromIdentity(targetPoolString, entity, typeof(int));
            }
        }

        public IStatBuilder EffectiveRegen(Pool pool) => FromIdentity($"{pool}.EffectiveRegen", typeof(int));
        public IStatBuilder EffectiveRecharge(Pool pool) => FromIdentity($"{pool}.EffectiveRecharge", typeof(int));
        public IStatBuilder RechargeStartDelay(Pool pool) => FromIdentity($"{pool}.RechargeStartDelay", typeof(double));

        public IStatBuilder EffectiveLeechRate(Pool pool) => FromIdentity($"{pool}.Leech.EffectiveRate", typeof(int));

        public IStatBuilder AbsoluteLeechRate(Pool pool) => FromIdentity($"{pool}.Leech.AbsoluteRate", typeof(double));

        public IStatBuilder AbsoluteLeechRateLimit(Pool pool)
            => FromIdentity($"{pool}.Leech.AbsoluteRateLimit", typeof(double));

        public IStatBuilder TimeToReachLeechRateLimit(Pool pool)
            => FromIdentity($"{pool}.Leech.SecondsToReachRateLimit", typeof(double));

        public IDamageRelatedStatBuilder AverageEffectiveDamage => DamageRelatedFromIdentity(typeof(double));

        public IStatBuilder AilmentDealtDamageType(Ailment ailment)
            => FromStatFactory(e => StatFactory.AilmentDealtDamageType(e, ailment));

        public IStatBuilder HitsPerSecond => FromIdentity(typeof(double));

        public IDamageRelatedStatBuilder EffectiveStunThreshold
            => DamageRelatedFromIdentity("Stun.EffectiveThreshold", typeof(double)).WithHits;

        public IStatBuilder StunAvoidanceWhileCasting => FromIdentity("Stun.ChanceToAvoidWhileCasting", typeof(double));
    }
}