using PoESkillTree.Localization;
using PoESkillTree.Utils.UrlProcessing.Decoders;
using System.Collections.Generic;

namespace PoESkillTree.Utils.UrlProcessing
{
    public static class SkillTreeUrlDecoders
    {
        private static IEnumerable<ISkillTreeUrlDecoder> _decoders = new List<ISkillTreeUrlDecoder>()
        {
            new PoESkillTreeUrlV6Decoder(),
            new PoESkillTreeUrlV5Decoder(),
            new PoESkillTreeUrlV4Decoder(),
            new PoESkillTreeUrlV3Decoder(),
        };

        public static SkillTreeUrlData TryDecode(string url)
        {
            foreach (var decoder in _decoders)
            {
                if (decoder.CanDecode(url))
                {
                    return decoder.Decode(url);
                }
            }

            return new SkillTreeUrlData()
            {
                IsValid = false,
                CompatibilityIssues = new List<string>()
                {
                    L10n.Message($"Could not find a valid decoder for the normalized url: {url}"),
                }
            };
        }
    }
}
