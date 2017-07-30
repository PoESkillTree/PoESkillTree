using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Entities;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface ILeechStatProvider
    {
        IStatProvider Of(IDamageStatProvider damage);

        IStatProvider RateLimit { get; }
        IStatProvider Rate { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Leech property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatProvider AppliesTo(IPoolStatProvider stat);

        // This is the entity that deals the damage by default. Can be changed leech to a different
        // target, e.g. Chieftain's "1% of Damage dealt by your Totems is Leeched to you as Life".
        ILeechStatProvider To(IEntityProvider entity);

        // If set, all DamageStats from "Of(damage)" have their DamageType changed to the parameter
        IFlagStatProvider BasedOn(IDamageTypeProvider damageType);
    }
}