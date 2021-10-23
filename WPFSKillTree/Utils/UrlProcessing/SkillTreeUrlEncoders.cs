using PoESkillTree.Utils.UrlProcessing.Encoders;
using PoESkillTree.ViewModels.PassiveTree;
using System.Collections.Generic;

namespace PoESkillTree.Utils.UrlProcessing
{
    public static class SkillTreeUrlEncoders
    {
        private static IEnumerable<ISkillTreeUrlEncoder> _encoders = new List<ISkillTreeUrlEncoder>()
        {
            new PoESkillTreeUrlEncoder(),
        };

        public static string? TryEncode(byte character, byte ascendancy, IEnumerable<PassiveNodeViewModel> nodes)
        {
            foreach (var encoder in _encoders)
            {
                if (encoder.CanEncode(character, ascendancy, nodes))
                {
                    return encoder.Encode(character, ascendancy, nodes);
                }
            }

            return null;
        }
    }
}
