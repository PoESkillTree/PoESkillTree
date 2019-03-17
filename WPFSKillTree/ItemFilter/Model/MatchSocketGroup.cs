using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchSocketGroup : MatchStrings
    {
        public MatchSocketGroup(string[] socketGroup)
            : base(socketGroup)
        {
            Keyword = "SocketGroup";
            Priority = Type.SocketGroup;
        }
    }
}
