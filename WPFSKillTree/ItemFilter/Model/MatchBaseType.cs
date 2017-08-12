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

            // BaseType can subset Class match based on learnt strings, while respecting narrowing.
            if (match is MatchClass)
            {
                MatchClass clazz = match as MatchClass;

                // If Class match is narrowed by BaseType, perform BaseType vs. BaseType narrowing the Class subset test.
                if (clazz.NarrowedBy != null && !base.Subsets(clazz.NarrowedBy))
                    return false;

                // BaseType match subsets Class match, if all its strings are defined in BaseTypesOfClass for Class match.
                if (clazz.Contains(this))
                    return true;
            }

            return false;
        }
    }
}
