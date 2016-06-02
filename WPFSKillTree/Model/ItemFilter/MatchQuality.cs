using System;

namespace POESKillTree.Model.ItemFilter
{
    public class MatchQuality : MatchNumber
    {
        // Implicit match.
        public MatchQuality()
            : base(MatchNumber.Operator.Between, 0, 30, 0, 30)
        {
            Keyword = "Quality";
            Priority = Type.Quality;
        }

        public MatchQuality(Operator op, int quality)
            : base(op, quality, 0, 30)
        {
            Keyword = "Quality";
            Priority = Type.Quality;
        }

        public MatchQuality(Operator op, int quality, int quality2)
            : base(op, quality, quality2, 0, 30)
        {
            Keyword = "Quality";
            Priority = Type.Quality;
        }
    }
}
