using EnumsNET;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IDamageSpecification
    {
        string StatIdentitySuffix { get; }

        DamageSource DamageSource { get; }

        Ailment? Ailment { get; }

        IDamageSpecification ForSkills();
    }

    public static class DamageSpecificationExtensions
    {
        public static bool IsSkillDamage(this IDamageSpecification @this) =>
            !@this.Ailment.HasValue;
    }

    public class SkillDamageSpecification : DamageSpecificationBase
    {
        public SkillDamageSpecification(DamageSource damageSource)
            : base(damageSource.GetName() + ".Skill", damageSource, null)
        {
        }

        public override IDamageSpecification ForSkills() => this;
    }

    public class SkillAttackDamageSpecification : DamageSpecificationBase
    {
        public SkillAttackDamageSpecification(AttackDamageHand attackDamageHand)
            : base(DamageSource.Attack.GetName() + "." + attackDamageHand.GetName() + ".Skill",
                DamageSource.Attack, null)
        {
        }

        public override IDamageSpecification ForSkills() => this;
    }

    public class AilmentDamageSpecification : DamageSpecificationBase
    {
        public AilmentDamageSpecification(DamageSource damageSource, Ailment ailment)
            : base(damageSource.GetName() + "." + ailment.GetName(), damageSource, ailment)
        {
        }

        public override IDamageSpecification ForSkills() => new SkillDamageSpecification(DamageSource);
    }

    public class AilmentAttackDamageSpecification : DamageSpecificationBase
    {
        private readonly AttackDamageHand _attackDamageHand;

        public AilmentAttackDamageSpecification(AttackDamageHand attackDamageHand, Ailment ailment)
            : base(DamageSource.Attack.GetName() + "." + attackDamageHand.GetName() + "." + ailment.GetName(),
                DamageSource.Attack, ailment)
        {
            _attackDamageHand = attackDamageHand;
        }

        public override IDamageSpecification ForSkills() => new SkillAttackDamageSpecification(_attackDamageHand);
    }

    public abstract class DamageSpecificationBase : IDamageSpecification
    {
        protected DamageSpecificationBase(string statIdentitySuffix, DamageSource damageSource, Ailment? ailment)
        {
            StatIdentitySuffix = statIdentitySuffix;
            DamageSource = damageSource;
            Ailment = ailment;
        }

        public string StatIdentitySuffix { get; }
        public DamageSource DamageSource { get; }
        public Ailment? Ailment { get; }
        public abstract IDamageSpecification ForSkills();

        public override bool Equals(object obj) =>
            (obj == this) || (obj is IDamageSpecification other && Equals(other));

        private bool Equals(IDamageSpecification other) =>
            StatIdentitySuffix == other.StatIdentitySuffix;

        public override int GetHashCode() =>
            StatIdentitySuffix.GetHashCode();
    }
}