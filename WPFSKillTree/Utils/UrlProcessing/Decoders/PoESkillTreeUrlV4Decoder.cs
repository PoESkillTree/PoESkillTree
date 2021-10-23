using PoESkillTree.Engine.GameModel;
using PoESkillTree.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoESkillTree.Utils.UrlProcessing.Decoders
{
    public class PoESkillTreeUrlV4Decoder : ISkillTreeUrlDecoder
    {
        private static readonly Regex UrlRegex = new Regex(@".*\/(?<build>[\w-=]+)");
        private static readonly int SupportedVersion = 4;

        public bool CanDecode(string url) => UrlRegex.IsMatch(url) && GetVersion(BytesFromUrl(url)) == SupportedVersion;
        public SkillTreeUrlData Decode(string url)
        {
            var bytes = BytesFromUrl(url);

            var version = GetVersion(bytes);
            var character = GetCharacter(bytes);
            var ascendancy = GetAscendancy(bytes);
            var _ = GetFullscreen(bytes);

            var nodeIds = new HashSet<ushort>();
            for (int j = 7; j < bytes.Length; j += 2)
            {
                if (j > bytes.Length || j + 1 > bytes.Length)
                {
                    break;
                }

                ushort nodeId = (ushort)(bytes[j] << 8 | bytes[j + 1]);
                nodeIds.Add(nodeId);
            }

            return new SkillTreeUrlData()
            {
                Version = version,
                CharacterClass = (CharacterClass)character,
                AscendancyClassId = ascendancy,
                SkilledNodesIds = nodeIds.ToList(),
                ClusterJewelNodesIds = new List<ushort>(),
                MasteryEffectPairs = new List<(ushort, ushort)>(),

                IsValid = true,
                CompatibilityIssues = new List<string>()
            };
        }

        private int GetVersion(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                return 0;
            }

            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

        private int GetCharacter(byte[] bytes)
        {
            if (bytes.Length < 5)
            {
                return 0;
            }

            return bytes[4];
        }

        private int GetAscendancy(byte[] bytes)
        {
            if (bytes.Length < 6)
            {
                return 0;
            }

            return bytes[5];
        }

        private int GetFullscreen(byte[] bytes)
        {
            if (bytes.Length < 7)
            {
                return 0;
            }

            return bytes[6];
        }

        private byte[] BytesFromUrl(string url)
        {
            var match = UrlRegex.Match(url);
            if (!match.Success)
            {
                throw new Exception(L10n.Message("Failed to load build from URL."));
            }

            var newUrl = match.Groups["build"].Value.Replace("-", "+").Replace("_", "/");
            return Convert.FromBase64String(newUrl);
        }
    }
}
