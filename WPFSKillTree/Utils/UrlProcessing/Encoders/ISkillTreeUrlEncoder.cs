using PoESkillTree.ViewModels.PassiveTree;
using System.Collections.Generic;

namespace PoESkillTree.Utils.UrlProcessing.Encoders
{
    public interface ISkillTreeUrlEncoder
    {
        public bool CanEncode(byte character, byte ascendancy, IEnumerable<PassiveNodeViewModel> nodes);
        public string Encode(byte character, byte ascendancy, IEnumerable<PassiveNodeViewModel> nodes);
    }
}
