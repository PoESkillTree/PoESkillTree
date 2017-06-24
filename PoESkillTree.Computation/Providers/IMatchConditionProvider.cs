using System;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface IMatchConditionProvider
    {
        
    }

    public static class MatchConditionProviders
    {
        public static IMatchConditionProvider MatchHas(ValueColoring valueColoring)
        {
            throw new NotImplementedException();
        }
    }
}