using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchDropLevel : MatchNumber
    {
        public MatchDropLevel(Operator op, int dropLevel)
            : base(op, dropLevel, 1, 100)
        {
            Keyword = "DropLevel";
            Priority = Type.DropLevel;
        }

        public MatchDropLevel(Operator op, int dropLevel, int dropLevel2)
            : base(op, dropLevel, dropLevel2, 1, 100)
        {
            Keyword = "DropLevel";
            Priority = Type.DropLevel;
        }
   }
}
