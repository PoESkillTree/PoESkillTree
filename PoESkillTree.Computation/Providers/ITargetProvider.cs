namespace PoESkillTree.Computation.Providers
{
    public interface ITargetProvider
    {
        IConditionProvider HitByInPastXSeconds(IDamageTypeProvider damageType, 
            ValueProvider seconds);

        IConditionProvider HitByRecently(IDamageTypeProvider damageType);

        // Changes the context of a stat in the same way as ConditionProviders.For(target)
        T Stat<T>(T stat) where T : IStatProvider;
    }


    public interface ISelfProvider : ITargetProvider
    {
        
    }


    public interface IEnemyProvider : ITargetProvider
    {
        IConditionProvider IsNearby { get; }

        IConditionProvider IsRare { get; }
        IConditionProvider IsUnique { get; }
        IConditionProvider IsRareOrUnique { get; }
    }


    public interface ITargetFromSkillProvider : ITargetProvider
    {
        // Limits the targets this instance describes
        ITargetFromSkillProvider With(IKeywordProvider keyword);
        ITargetFromSkillProvider With(params IKeywordProvider[] keywords);
        ITargetFromSkillProvider From(ISkillProvider skill);
    }


    public static class TargetProviders
    {
        public static readonly ISelfProvider Self;
        public static readonly IEnemyProvider Enemy;
        public static readonly ITargetProvider Ally;
        public static readonly ITargetFromSkillProvider Totem;
        public static readonly ITargetFromSkillProvider Minion;

        public static readonly ITargetProvider AnyTarget;
    }
}