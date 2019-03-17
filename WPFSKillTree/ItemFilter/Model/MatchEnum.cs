using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchEnum : Match, IMergeableMatch
    {
        public enum Operator : int
        {
            Between, Equal, GreaterOrEqual, GreaterThan, LessOrEqual, LessThan
        }

        private int Max { get { return Values.Length - 1; } }

        private const int Min = 0;

        private int Ordinal;

        private int Ordinal2;

        private string Value { get { return Values[Ordinal]; } }

        private string Value2 { get { return Values[Ordinal2]; } }

        private string[] Values;

        protected MatchEnum(MatchEnum copy)
            : base(copy)
        {
            Ordinal = copy.Ordinal;
            Ordinal2 = copy.Ordinal2;
            Values = copy.Values;
        }

        public MatchEnum(Operator op, int ordinal, string[] values)
        {
            Values = values;

            // Translate ordinal value based on operator into range between Ordinal and Ordinal2.
            switch (op)
            {
                case Operator.Equal:
                    Ordinal = ordinal;
                    Ordinal2 = ordinal;
                    break;

                case Operator.GreaterOrEqual:
                    Ordinal = ordinal;
                    Ordinal2 = Max;
                    break;

                case Operator.GreaterThan:
                    Ordinal = ordinal + 1;
                    if (Ordinal > Max)
                        throw new ArgumentOutOfRangeException();
                    Ordinal2 = Max;
                    break;

                case Operator.LessOrEqual:
                    Ordinal = Min;
                    Ordinal2 = ordinal;
                    break;

                case Operator.LessThan:
                    Ordinal = Min;
                    Ordinal2 = ordinal - 1;
                    if (Ordinal2 < Min)
                        throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        // Overriden due to more restrictive conditions.
        public override bool CanMerge(Match match)
        {
            if (!base.CanMerge(match)) return false;

            MatchEnum that = match as MatchEnum;

            // This match subsets, is subsetted by, or complements match.
            return Subsets(that) || that.Subsets(this) || Complements(that);
        }

        public override Match Clone()
        {
            return new MatchEnum(this);
        }

        // Comparison is done in way that, less specific match follows more specific one.
        public override int CompareTo(Match match)
        {
            int result = base.CompareTo(match);
            if (result != 0) return result;

            // Test for equality first, due to Subsets treating equal matches as subsettable too.
            if (Equals(match)) return 0;
            else // If we subset match, put us ahead if it.
                if (Subsets(match)) return -1;
            else // If match subsets us, put us after match.
                if ((match as MatchEnum).Subsets(this)) return 1;

            return 0;
        }

        public bool Complements(Match match)
        {
            // The different priorities cannot complement each other (e.g. "Quality" cannot complement "ItemLevel" and vice versa).
            if (Priority != match.Priority) return false;

            MatchEnum that = match as MatchEnum;

            // Complements from left or right.
            // XXX: Don't have to take operator into account at all, due to conversion of all operators into Between ranges in constructor.
            return Ordinal2 + 1 == that.Ordinal || Ordinal == that.Ordinal2 + 1;
        }

        public override bool Equals(Match match)
        {
            return base.Equals(match) && (match as MatchEnum).Ordinal == Ordinal && (match as MatchEnum).Ordinal2 == Ordinal2;
        }

        public override bool IsImplicit()
        {
            // The match is implicit if it matches whole <Min, Max> range.
            return Ordinal == Min && Ordinal2 == Max;
        }

        public void Merge(Match match)
        {
            MatchEnum that = match as MatchEnum;

            if (Complements(that))
            {
                // Complements from left.
                if (Ordinal2 + 1 == that.Ordinal)
                    Ordinal2 = that.Ordinal2;
                else // Ordinal == that.Ordinal2 + 1
                    Ordinal = that.Ordinal;
            }
            else // If we subset the match.
                if (Subsets(that))
            {
                Ordinal = that.Ordinal;
                Ordinal2 = that.Ordinal2;
            } // Otherwise match subsets us, nothing to do.
        }

        public bool Subsets(Match match)
        {
            if (Priority != match.Priority) return false;

            MatchEnum that = match as MatchEnum;

            return Ordinal >= that.Ordinal && Ordinal2 <= that.Ordinal2;
        }

        public override string ToString()
        {
            if (Ordinal == Ordinal2) // Equals
                return Keyword + " " + Value;
            else
                if (Ordinal == Min) // LessOrEqual
                    return Keyword + " <= " + Value2;
            else
                if (Ordinal2 == Max) // GreaterOrEqual
                    return Keyword + " >= " + Value;
            else // Between
                return Keyword + " >= " + Value + MultilineMatchDelimiter + Keyword + " <= " + Value2;
        }
    }
}
