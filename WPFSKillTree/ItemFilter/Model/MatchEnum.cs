using System;

namespace POESKillTree.ItemFilter.Model
{
    public class MatchEnum : Match
    {
        public enum Operator
        {
            Equal, GreaterOrEqual, GreaterThan, LessOrEqual, LessThan
        }

        private Operator Op;

        private string Value;

        protected MatchEnum(MatchEnum copy)
            : base(copy)
        {
            Op = copy.Op;
            Value = copy.Value;
        }

        public MatchEnum(Operator op, string value)
        {
            Op = op;
            Value = value;
        }

        public override Match Clone()
        {
            return new MatchEnum(this);
        }

        public override bool Equals(Match match)
        {
            return base.Equals(match) && (match as MatchEnum).Op == Op && (match as MatchEnum).Value == Value;
        }

        public override string ToString()
        {
            switch (Op)
            {
                case Operator.GreaterOrEqual:
                    return Keyword + " >= " + Value;

                case Operator.GreaterThan:
                    return Keyword + " > " + Value;

                case Operator.LessOrEqual:
                    return Keyword + " <= " + Value;

                case Operator.LessThan:
                    return Keyword + " < " + Value;

                default: // Operator.Equal
                    return Keyword + " " + Value;
            }
        }
    }
}
