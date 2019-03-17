using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchNumber : Match, IMergeableMatch
    {
        public enum Operator
        {
            Between, Equal, GreaterOrEqual, GreaterThan, LessOrEqual, LessThan, NotEqual
        }

        private int Max;

        private int Min;

        private Operator Op;

        private int Value;

        private int Value2;

        protected MatchNumber(MatchNumber copy)
            : base(copy)
        {
            Op = copy.Op;
            Value = copy.Value;
            Value2 = copy.Value2;
            Min = copy.Min;
            Max = copy.Max;
        }

        public MatchNumber(Operator op, int value, int min, int max)
        {
            if (op == Operator.Between || value < min || value > max)
                throw new ArgumentOutOfRangeException();

            Min = min;
            Max = max;

            // Translate non-between operators to between operator.
            Op = Operator.Between;
            switch (op)
            {
                case Operator.Equal:
                    Value = value;
                    Value2 = value;
                    break;

                case Operator.GreaterOrEqual:
                    Value = value;
                    Value2 = Max;
                    break;

                case Operator.GreaterThan:
                    Value = value + 1;
                    if (Value > Max)
                        throw new ArgumentOutOfRangeException();
                    Value2 = Max;
                    break;

                case Operator.LessOrEqual:
                    Value = Min;
                    Value2 = value;
                    break;

                case Operator.LessThan:
                    Value = Min;
                    Value2 = value - 1;
                    if (Value2 < Min)
                        throw new ArgumentOutOfRangeException();
                    break;

                case Operator.NotEqual: // Cannot be translated to between operator.
                    Op = op;
                    Value = value;
                    break;
            }
        }

        public MatchNumber(Operator op, int value, int value2, int min, int max)
        {
            if (op != Operator.Between || value < min || value > max || value2 < min || value2 > max)
                throw new ArgumentOutOfRangeException();

            Op = op;
            if (value < value2)
            {
                Value = value;
                Value2 = value2;
            }
            else
            {
                Value = value2;
                Value2 = value;
            }
            Min = min;
            Max = max;
        }

        // Overriden due to more restrictive conditions.
        public override bool CanMerge(Match match)
        {
            if (!base.CanMerge(match)) return false;

            MatchNumber number = match as MatchNumber;

            // Non-equality operators are not mergeable.
            if (Op == Operator.NotEqual || number.Op == Operator.NotEqual) return false;

            // This match subsets, is subsetted by match, or complements match.
            return Subsets(match) || number.Subsets(this) || Complements(match);
        }

        public override Match Clone()
        {
            return new MatchNumber(this);
        }

        public bool Complements(Match match)
        {
            // The different priorities cannot complement each other (e.g. "Quality" cannot complement "ItemLevel" and vice versa).
            if (Priority != match.Priority) return false;

            MatchNumber number = match as MatchNumber;

            // Non-equality operator cannot complement anything.
            if (Op == Operator.NotEqual || number.Op == Operator.NotEqual) return false;

            // Complements from left or right.
            // XXX: Don't have to take operator into account at all, due to conversion of all operators into Between ranges in constructor.
            return Value2 + 1 == number.Value || Value == number.Value2 + 1;
        }

        // Comparison is done in way that, less specific match follows more specific one.
        public override int CompareTo(Match match)
        {
            int result = base.CompareTo(match);
            if (result != 0) return result;

            // If we subset match, put us ahead if it.
            if (Subsets(match)) return -1;
            else // If match subsets us, put us after match.
                if ((match as MatchNumber).Subsets(this)) return 1;

            return 0;
        }

        public override bool Equals(Match match)
        {
            return base.Equals(match) && (match as MatchNumber).Op == Op && (match as MatchNumber).Value == Value && (match as MatchNumber).Value2 == Value2;
        }

        public override bool IsImplicit()
        {
            // The match is implicit if it matches whole <Min, Max> range.
            return Value == Min && Value2 == Max;
        }

        public void Merge(Match match)
        {
            MatchNumber number = match as MatchNumber;

            if (Complements(number))
            {
                // Complements from left.
                if (Value2 + 1 == number.Value)
                    Value2 = number.Value2;
                else // Value 1 == number.Value2 + 1
                    Value = number.Value;
            }
            else // If we subset the match.
                if (Subsets(number))
                {
                    Op = number.Op;
                    Value = number.Value;
                    Value2 = number.Value2;
                } // Otherwise match subsets us, nothing to do.
        }

        public bool Subsets(Match match)
        {
            if (Priority != match.Priority) return false;

            MatchNumber number = match as MatchNumber;

            // Non-equality operator cannot be subsetted.
            if (Op == Operator.NotEqual || number.Op == Operator.NotEqual) return false;

            return Value >= number.Value && Value2 <= number.Value2;
        }

        public override string ToString()
        {
            if (Op == Operator.NotEqual)
                return Keyword + " != " + Value;
            else
                if (Value == Value2) // Equals
                    return Keyword + " = " + Value;
                else
                    if (Value == Min) // LessOrEqual
                        return Keyword + " <= " + Value2;
                    else
                        if (Value2 == Max) // GreaterOrEqual
                            return Keyword + " >= " + Value;
                        else // Between
                            return Keyword + " >= " + Value + MultilineMatchDelimiter + Keyword + " <= " + Value2;
        }
    }
}
