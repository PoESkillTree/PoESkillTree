using System;

namespace POESKillTree.Model.ItemFilter
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
