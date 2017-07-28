namespace PoESkillTree.Computation.Providers
{
    public interface IGroupConverter
    {
        T As<T>() where T : IStatProvider;

        IDamageTypeProvider AsDamageType { get; }

        IChargeTypeProvider AsChargeType { get; }

        IAilmentProvider AsAilment { get; }
        IDamagingAilmentProvider AsDamagingAilment { get; }

        IKeywordProvider AsKeyword { get; }

        IItemSlotProvider AsItemSlot { get; }

        IActionProvider<ISelfProvider, ITargetProvider> AsAction { get; }

        IStatProvider AsStat { get; }

        ISkillProvider AsSkill { get; }
    }


    public interface IGroupConverterCollection : IProviderCollection<IGroupConverter>
    {
        
    }


    public static class GroupConverters
    {
        // includes only regex groups of from ({FooMatchers})
        public static readonly IGroupConverterCollection Groups;

        public static readonly IGroupConverter Group = Groups.Single;
    }
}