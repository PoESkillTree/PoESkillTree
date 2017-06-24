using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IGroupConverter
    {

        IDamageTypeProvider AsDamageType { get; }

        IStatProvider AsStat { get; }

        ISkillProvider AsSkill { get; }
    }

    public static class GroupConverters
    {
        public static IGroupConverter Group(int index)
        {
            throw new NotImplementedException();
        }
        public static IGroupConverter EvaluatedGroup(int index)
        {
            throw new NotImplementedException();
        }
    }
}