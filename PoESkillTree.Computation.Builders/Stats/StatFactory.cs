using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatFactory : IStatFactory
    {
        private readonly IDictionary<(string, Entity), IStat> _cache = new Dictionary<(string, Entity), IStat>();
        private readonly BehaviorFactory _behaviorFactory;

        public StatFactory()
        {
            _behaviorFactory = new BehaviorFactory(this);
        }

        public IStat FromIdentity(string identity, Entity entity, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null) =>
            GetOrAdd(identity, entity, dataType, explicitRegistrationType);

        public IStat CopyWithSuffix(IStat stat, string identitySuffix, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null) =>
            CopyWithSuffix(stat, identitySuffix, dataType, null, explicitRegistrationType);

        public IStat ChanceToDouble(IStat stat) =>
            CopyWithSuffix(stat, nameof(ChanceToDouble), typeof(int));

        public IEnumerable<IStat> ConvertTo(IStat source, IEnumerable<IStat> targets)
        {
            foreach (var target in targets)
            {
                yield return ConvertTo(source, target);
            }
            yield return Conversion(source);
            yield return SkillConversion(source);
        }

        public IEnumerable<IStat> GainAs(IStat source, IEnumerable<IStat> targets)
        {
            foreach (var target in targets)
            {
                yield return GainAs(source, target);
            }
        }

        public IStat ConvertTo(IStat source, IStat target) =>
            CopyWithSuffix(source, $"{nameof(ConvertTo)}({target.Identity})", typeof(int),
                () => _behaviorFactory.ConvertTo(source, target));

        public IStat GainAs(IStat source, IStat target) =>
            CopyWithSuffix(source, $"{nameof(GainAs)}({target.Identity})", typeof(int),
                () => _behaviorFactory.GainAs(source, target));

        public IStat Conversion(IStat source) =>
            CopyWithSuffix(source, "Conversion", typeof(int));

        public IStat SkillConversion(IStat source) =>
            CopyWithSuffix(source, "SkillConversion", typeof(int),
                () => _behaviorFactory.SkillConversion(source));

        public IStat Regen(Entity entity, Pool pool) =>
            GetOrAdd($"{pool}.Regen", entity, typeof(double), behaviors: () => _behaviorFactory.Regen(pool, entity));

        public IStat RegenTargetPool(Entity entity, Pool regenPool) =>
            GetOrAdd($"{regenPool}.Regen.TargetPool", entity, typeof(Pool));

        public IStat LeechTargetPool(Entity entity, Pool leechPool) =>
            GetOrAdd($"{leechPool}.Leech.TargetPool", entity, typeof(Pool));

        public IStat MainSkillId(Entity entity) =>
            GetOrAdd("MainSkill.Id", entity, typeof(int));

        public IStat MainSkillHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkill.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartCastSpeedHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.CastSpeed.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartDamageHasKeyword(Entity entity, Keyword keyword, DamageSource damageSource) =>
            GetOrAdd($"MainSkillPart.Damage.{damageSource}.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartAilmentDamageHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.Damage.Ailment.Has.{keyword}", entity, typeof(bool));

        public IStat BuffEffect(Entity source, Entity target, string buffIdentity) =>
            GetOrAdd($"{buffIdentity}.EffectOn({target})", source, typeof(double));

        public IStat BuffIsActive(Entity target, string buffIdentity) =>
            GetOrAdd($"{buffIdentity}.BuffActive", target, typeof(bool));

        public IStat BuffSourceIs(Entity source, Entity target, string buffIdentity) =>
            GetOrAdd($"{buffIdentity}.BuffSourceIs({source})", target, typeof(bool));

        public IStat Damage(Entity entity, DamageType damageType) =>
            GetOrAdd(damageType + ".Damage", entity, typeof(int));

        public IStat ConcretizeDamage(IStat stat, IDamageSpecification damageSpecification) =>
            CopyWithSuffix(stat, damageSpecification.StatIdentitySuffix, stat.DataType,
                () => _behaviorFactory.ConcretizeDamage(stat, damageSpecification));

        public IStat ApplyModifiersToSkillDamage(IStat stat, DamageSource damageSource, Form form) =>
            CopyWithSuffix(stat, $"ApplyModifiersToSkills({damageSource} for form {form})", typeof(int));

        public IStat ApplyModifiersToAilmentDamage(IStat stat, Form form) =>
            CopyWithSuffix(stat, $"ApplyModifiersToAilments(for form {form})", typeof(int));

        public IStat DamageTaken(IStat damage) =>
            CopyWithSuffix(damage, "Taken", damage.DataType);

        public IStat AilmentDealtDamageType(Entity entity, Ailment ailment) =>
            GetOrAdd($"{ailment}.DamageType", entity, typeof(DamageType));

        private IStat CopyWithSuffix(IStat source, string identitySuffix, Type dataType,
            Func<IReadOnlyList<Behavior>> behaviors, ExplicitRegistrationType explicitRegistrationType = null)
        {
            return GetOrAdd(source.Identity + "." + identitySuffix, source.Entity,
                dataType, explicitRegistrationType, behaviors);
        }

        private IStat GetOrAdd(string identity, Entity entity, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null, Func<IReadOnlyList<Behavior>> behaviors = null)
        {
            // Func<IReadOnlyList<Behavior>> for performance reasons: Only retrieve behaviors if necessary.
            return _cache.GetOrAdd((identity, entity), _ =>
                new Stat(identity, entity, dataType, explicitRegistrationType, behaviors?.Invoke()));
        }
    }
}