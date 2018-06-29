using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IDamageSpecification
    {
        string StatIdentitySuffix { get; }

        DamageSource DamageSource { get; }

        bool IsSkillDamage { get; }
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

        public bool IsSkillDamage => true;
    }

    public class AttackDamageSpecification : IDamageSpecification
    {
        public AttackDamageSpecification(AttackDamageHand attackDamageHand) =>
            StatIdentitySuffix = $"{DamageSource.Attack}.{attackDamageHand}.Skill";

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource => DamageSource.Attack;

        public bool IsSkillDamage => true;
    }

    public class AilmentDamageSpecification : IDamageSpecification
    {
        public AilmentDamageSpecification(Ailment ailment) =>
            StatIdentitySuffix = $"{DamageSource.OverTime}.{ailment}";

        public string StatIdentitySuffix { get; }

        public DamageSource DamageSource => DamageSource.OverTime;

        public bool IsSkillDamage => false;
    }
}