using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="IReferenceConverter"/> that simply casts the referenced value passed to its
    /// constructor, i.e. it is already resolved.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="ParseException"/> if the referenced value can not be cast to the type of the called property.
    /// </remarks>
    public class ReferenceConverter : IReferenceConverter
    {
        private readonly object _referencedBuilder;

        public IDamageTypeBuilder AsDamageType => As<IDamageTypeBuilder>();
        public IChargeTypeBuilder AsChargeType => As<IChargeTypeBuilder>();
        public IAilmentBuilder AsAilment => As<IAilmentBuilder>();
        public IKeywordBuilder AsKeyword => As<IKeywordBuilder>();
        public IItemSlotBuilder AsItemSlot => As<IItemSlotBuilder>();
        public IActionBuilder AsAction => As<IActionBuilder>();
        public IStatBuilder AsStat => As<IStatBuilder>();
        public IPoolStatBuilder AsPoolStat => As<IPoolStatBuilder>();
        public IBuffBuilder AsBuff => As<IBuffBuilder>();
        public ISkillBuilder AsSkill => As<ISkillBuilder>();

        public ReferenceConverter(object referencedBuilder)
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
                $"Can't convert reference of type {_referencedBuilder.GetType()} to {typeof(TTarget)}");
        }

        private bool Equals(ReferenceConverter other)
        {
            return Equals(_referencedBuilder, other._referencedBuilder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is ReferenceConverter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _referencedBuilder.GetHashCode();
        }
    }
}
