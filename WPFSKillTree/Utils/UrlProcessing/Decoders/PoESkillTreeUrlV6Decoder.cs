using PoESkillTree.Engine.GameModel;
using PoESkillTree.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoESkillTree.Utils.UrlProcessing.Decoders
{
    public class PoESkillTreeUrlV6Decoder : ISkillTreeUrlDecoder
    {
        private static readonly Regex UrlRegex = new Regex(@".*\/(?<build>[\w-=]+)");
        private static readonly int SupportedVersion = 6;

        public bool CanDecode(string url) => UrlRegex.IsMatch(url) && GetVersion(BytesFromUrl(url)) >= SupportedVersion;
        public SkillTreeUrlData Decode(string url)
        {
            var bytes = BytesFromUrl(url);

            var version = GetVersion(bytes);
            var character = GetCharacter(bytes);
            var ascendancy = GetAscendancy(bytes);
            var i = 6;

            var nodeIds = new HashSet<ushort>();
            var nodeCount = bytes[i++];
            for (var j = 0; j < nodeCount; j++)
            {
                var id = (ushort)(bytes[i++] << 8 | bytes[i++]);
                nodeIds.Add(id);
            }

            var clusterJewelIds = new HashSet<ushort>();
            var clusterJewelCount = bytes[i++];
            for (var j = 0; j < clusterJewelCount; j++)
            {
                var id = (ushort)(bytes[i++] << 8 | bytes[i++]);
                clusterJewelIds.Add(id);
                nodeIds.Add(id);
            }

            var masteryEffectPairs = new HashSet<(ushort, ushort)>();
            var masteryEffectPairsCount = bytes[i++];
            for (var j = 0; j < masteryEffectPairsCount; j++)
            {
                var effect = (ushort)(bytes[i++] << 8 | bytes[i++]);
                var id = (ushort)(bytes[i++] << 8 | bytes[i++]);
                masteryEffectPairs.Add((id, effect));
                nodeIds.Add(id);
            }

            return new SkillTreeUrlData()
            {
                Version = version,
                CharacterClass = (CharacterClass)character,
                AscendancyClassId = ascendancy,
                SkilledNodesIds = nodeIds.ToList(),
                ClusterJewelNodesIds = clusterJewelIds.ToList(),
                MasteryEffectPairs = masteryEffectPairs.ToList(),

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
