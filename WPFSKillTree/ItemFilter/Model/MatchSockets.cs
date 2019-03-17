using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchSockets : MatchNumber
    {
        public MatchSockets(Operator op, int sockets)
            : base(op, sockets, 1, 6)
        {
            Keyword = "Sockets";
            Priority = Type.Sockets;
        }
    }
}
