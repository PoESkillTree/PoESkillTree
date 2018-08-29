using System.Collections.Generic;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.GameModel.StatTranslation
{
    public interface IStatTranslator
    {
        IEnumerable<string> Translate(IEnumerable<UntranslatedStat> untranslatedStats);
    }
}