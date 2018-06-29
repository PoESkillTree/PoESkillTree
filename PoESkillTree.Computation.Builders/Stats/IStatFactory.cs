using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IStatFactory
    {
        IStat FromIdentity(string identity, Entity entity, Type dataType, bool isExplicitlyRegistered = false);

        IStat ChanceToDouble(IStat stat);

        IEnumerable<IStat> ConvertTo(IStat sourceStat, IEnumerable<IStat> targetStats);
        IEnumerable<IStat> GainAs(IStat sourceStat, IEnumerable<IStat> targetStats);
        IStat ConvertTo(IStat source, IStat target);
        IStat GainAs(IStat source, IStat target);
        IStat Conversion(IStat source);
        IStat SkillConversion(IStat source);

        IStat Regen(Pool pool, Entity entity);
        IStat RegenTargetPool(Pool regenPool, Entity entity);

        IStat LeechPercentage(IStat damage);

        IStat ActiveSkillId(Entity entity);
        IStat ActiveSkillHasKeyword(Entity entity, Keyword keyword);
        IStat ActiveSkillCastSpeedHasKeyword(Entity entity, Keyword keyword);

        IStat ConcretizeDamage(IStat stat, IDamageSpecification damageSpecification);
        IStat ApplyModifiersToSkillDamage(IStat stat, DamageSource damageSource, Form form);
        IStat ApplyModifiersToAilmentDamage(IStat stat, Form form);
        IStat DamageTaken(IStat damage);
    }
}