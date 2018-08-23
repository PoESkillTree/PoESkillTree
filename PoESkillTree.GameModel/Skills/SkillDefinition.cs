using System;
using System.Collections.Generic;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinition
    {
        [Obsolete("Temporary constructor to support PoESkillTree.Computation.Console.SkillDefinition")]
        public SkillDefinition(string skillName, int numericId, IReadOnlyList<Keyword> keywords, bool providesBuff)
        {
            Id = skillName;
            NumericId = numericId;
            IsSupport = false;
            ActiveSkill = new ActiveSkillDefinition(skillName, keywords, providesBuff);
        }

        private SkillDefinition(string id, int numericId, bool isSupport, ActiveSkillDefinition activeSkill)
            => (Id, NumericId, IsSupport, ActiveSkill) = (id, numericId, isSupport, activeSkill);

        public static SkillDefinition CreateActive(string id, int numericId, ActiveSkillDefinition activeSkill)
            => new SkillDefinition(id, numericId, false, activeSkill);

        public string Id { get; }
        public int NumericId { get; }
        public bool IsSupport { get; }
        public ActiveSkillDefinition ActiveSkill { get; }
    }

    public class ActiveSkillDefinition
    {
        public ActiveSkillDefinition(string displayName, IReadOnlyList<Keyword> keywords, bool providesBuff)
            => (DisplayName, Keywords, ProvidesBuff) = (displayName, keywords, providesBuff);

        public string DisplayName { get; }
        public IReadOnlyList<Keyword> Keywords { get; }
        public bool ProvidesBuff { get; }
    }
}