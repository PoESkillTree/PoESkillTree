using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IBuffProvider
    {

    }

    public static class BuffProviders
    {
        public static readonly IBuffProvider Onslaught;

        public static IBuffProvider Buff(IBuffProvider buff, IBuffTargetProvider target)
        {
            throw new NotImplementedException();
        }

        // -> stats of the skill starting with "cursed enemies ..." are the (de)buff
        public static IBuffProvider Curse(ISkillProvider skill, IValueProvider level, IBuffTargetProvider target)
        {
            throw new NotImplementedException();
        }
    }
}