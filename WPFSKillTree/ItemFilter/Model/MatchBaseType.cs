using System;

namespace POESKillTree.ItemFilter.Model
{
    public class MatchBaseType : MatchStrings
    {
        protected MatchBaseType(MatchBaseType copy) : base(copy) { }

        public MatchBaseType(string[] types) : base(types)
        {
            Keyword = "BaseType";
            Priority = Type.BaseType;
        }

        public override Match Clone()
        {
            return new MatchBaseType(this);
        }

        public override bool Subsets(Match match)
        {
            if (match is MatchBaseType) return base.Subsets(match);

            if (match is MatchClass)
            {
                // BaseType matche subsets Class match, if all its strings are defined in BaseType list of Class match.
                MatchClass clazz = match as MatchClass;
                if (clazz.BaseTypes != null)
                {
                    foreach (string str in Values)
                        if (!clazz.BaseTypes.Contains(str))
                            return false;

                    return true;
                }
            }

            return false;
        }
    }
}
