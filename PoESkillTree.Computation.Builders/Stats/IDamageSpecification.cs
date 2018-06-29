using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IDamageSpecification
    {
        string StatIdentitySuffix { get; }

        DamageSource DamageSource { get; }

        Ailment? Ailment { get; }
    }

    public static class DamageSpecificationExtensions
    {
        public static bool IsSkillDamage(this IDamageSpecification @this) =>
            !@this.Ailment.HasValue;
    }

    public class SkillDamageSpecification : IDamageSpecification
    {
        public SkillDamageSpecification(DamageSource damageSource)
        {
            StatIdentitySuffix = $"{damageSource}.Skill";
            DamageSource = damageSource;
        }

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource { get; }

        public Ailment? Ailment => null;
    }

    public class SkillAttackDamageSpecification : IDamageSpecification
    {
        public SkillAttackDamageSpecification(AttackDamageHand attackDamageHand) =>
            StatIdentitySuffix = $"{DamageSource.Attack}.{attackDamageHand}.Skill";

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource => DamageSource.Attack;

        public Ailment? Ailment => null;
    }

    public class AilmentDamageSpecification : IDamageSpecification
    {
        public AilmentDamageSpecification(DamageSource damageSource, Ailment ailment)
        {
            StatIdentitySuffix = $"{damageSource}.{ailment}";
            DamageSource = damageSource;
            Ailment = ailment;
        }

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource { get; }

        public Ailment? Ailment { get; }
    }

    public class AilmentAttackDamageSpecification : IDamageSpecification
    {
        public AilmentAttackDamageSpecification(AttackDamageHand attackDamageHand, Ailment ailment)
        {
            StatIdentitySuffix = $"{DamageSource.Attack}.{attackDamageHand}.{ailment}";
            Ailment = ailment;
        }

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource => DamageSource.Attack;

        public Ailment? Ailment { get; }
    }
}