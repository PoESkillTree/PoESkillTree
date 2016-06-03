using System;

namespace POESKillTree.ItemFilter.Model
{
    public class MatchLinkedSockets : MatchNumber
    {
        public MatchLinkedSockets(Operator op, int linkedSockets)
            : base(op, linkedSockets, 0, 6)
        {
            Keyword = "LinkedSockets";
            Priority = Type.LinkedSockets;
        }
    }
}
