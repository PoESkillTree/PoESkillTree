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

        public ValueBuilder RegenTargetPoolValue(IPoolStatBuilder regenSourcePool)
        {
            return new ValueBuilder(new ValueBuilderImpl(BuildValue,
                c => RegenTargetPoolValue((IPoolStatBuilder) regenSourcePool.Resolve(c))));

            IValue BuildValue(BuildParameters parameters)
            {
                var entity = parameters.ModifierSourceEntity;
                var sourcePool = regenSourcePool.BuildPool();
                var targetPoolStat = StatFactory.RegenTargetPool(entity, sourcePool);
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
        }

        public IDamageRelatedStatBuilder AverageEffectiveDamage => DamageRelatedFromIdentity(typeof(double));

        public IStatBuilder AilmentDealtDamageType(Ailment ailment)
            => FromStatFactory(e => StatFactory.AilmentDealtDamageType(e, ailment));

        public IDamageRelatedStatBuilder EffectiveStunThreshold
            => DamageRelatedFromIdentity("Stun.EffectiveThreshold", typeof(double)).WithHits;

        public IStatBuilder StunAvoidanceWhileCasting => FromIdentity("Stun.ChanceToAvoidWhileCasting", typeof(double));
    }
}