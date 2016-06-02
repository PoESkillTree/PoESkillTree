using System;

namespace POESKillTree.Model.ItemFilter
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
