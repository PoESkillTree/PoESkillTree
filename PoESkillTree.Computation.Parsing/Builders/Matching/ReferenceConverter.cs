using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public class ReferenceConverter : IReferenceConverter
    {
        [CanBeNull] private readonly object _referencedBuilder;

        public IDamageTypeBuilder AsDamageType => As<IDamageTypeBuilder>();
        public IChargeTypeBuilder AsChargeType => As<IChargeTypeBuilder>();
        public IAilmentBuilder AsAilment => As<IAilmentBuilder>();
        public IKeywordBuilder AsKeyword => As<IKeywordBuilder>();
        public IItemSlotBuilder AsItemSlot => As<IItemSlotBuilder>();
        public ISelfToAnyActionBuilder AsAction => As<ISelfToAnyActionBuilder>();
        public IStatBuilder AsStat => As<IStatBuilder>();
        public IFlagStatBuilder AsFlagStat => As<IFlagStatBuilder>();
        public IPoolStatBuilder AsPoolStat => As<IPoolStatBuilder>();
        public IDamageStatBuilder AsDamageStat => As<IDamageStatBuilder>();
        public ISkillBuilder AsSkill => As<ISkillBuilder>();

        public ReferenceConverter([CanBeNull] object referencedBuilder)
        {
            _referencedBuilder = referencedBuilder;
        }

        private TTarget As<TTarget>() where TTarget : class
        {
            if (_referencedBuilder is TTarget target)
            {
                return target;
            }
            throw new ParseException(
                $"Can't convert reference of type {_referencedBuilder?.GetType()} to {typeof(TTarget)}");
        }
    }
}
