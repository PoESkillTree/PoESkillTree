using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface ILeechStatBuilder : IResolvable<ILeechStatBuilder>
    {
        IStatBuilder Of(IDamageStatBuilder damage);

        IStatBuilder RateLimit { get; }
        IStatBuilder Rate { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Leech property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatBuilder AppliesTo(IPoolStatBuilder stat);

        // This is the entity that deals the damage by default. Can be changed leech to a different
        // target, e.g. Chieftain's "1% of Damage dealt by your Totems is Leeched to you as Life".
        ILeechStatBuilder To(IEntityBuilder entity);

        // If set, all DamageStats from "Of(damage)" have their DamageType changed to the parameter
        IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType);
    }
}