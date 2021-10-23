using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Utils.UrlProcessing.Encoders
{
    public class PoESkillTreeUrlEncoder : ISkillTreeUrlEncoder
    {
        public bool CanEncode(byte _, byte __, IEnumerable<PassiveNodeViewModel> ___) => true;

        public string Encode(byte character, byte ascendancy, IEnumerable<PassiveNodeViewModel> nodes)
        {
            // Nodes we know won't be included in the Url at all
            var validNodes = nodes.Where(node => !(node.IsAscendancyStart || node.IsRootNode || node.PassiveNodeGroup is null || node.IsProxy || node.PassiveNodeGroup.IsProxy));

            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = validNodes
                .Where(node => node.PassiveNodeType != PassiveNodeType.ExpansionJewelSocket)
                .Select(node => node.Id)
                .OrderBy(n => n)
                .ToList();

            var clusterJewelNodes = validNodes
                .Where(node => node.PassiveNodeType == PassiveNodeType.ExpansionJewelSocket)
                .Select(node => node.Id)
                .OrderBy(n => n)
                .ToList();

            var masteryNodes = validNodes
                .Where(node => node.PassiveNodeType == PassiveNodeType.Mastery)
                .Select(node => node.Id)
                .OrderBy(n => n)
                .ToList();

            // Count of Node Hashes and Hashes (uit16[])
            var nodeSize = 1 + skillNodes.Count * 2;

            // Count of Cluster Jewel Hashes and Hashes (uint16[])
            var clusterJewelSize = 1 + clusterJewelNodes.Count * 2;

            // Count of Mastery Hashes and Hashes (uint32[])
            var masterySize = 1 + masteryNodes.Count * 4;

            var bytes = new byte[6 + nodeSize + clusterJewelSize + masterySize];
            var i = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 6;
            bytes[i++] = character;
            bytes[i++] = ascendancy;

            bytes[i++] = (byte)skillNodes.Count;
            foreach (var inn in skillNodes)
            {
                bytes[i++] = (byte)((inn >> 8) & 0xFF);
                bytes[i++] = (byte)(inn & 0xFF);
            }

            bytes[i++] = (byte)clusterJewelNodes.Count;
            foreach (var inn in clusterJewelNodes)
            {
                bytes[i++] = (byte)((inn >> 8) & 0xFF);
                bytes[i++] = (byte)(inn & 0xFF);
            }

            bytes[i++] = (byte)masteryNodes.Count;
            foreach (var inn in masteryNodes)
            {
                var effect = nodes.First(x => x.Id == inn).Skill;
                bytes[i++] = (byte)((effect >> 8) & 0xFF);
                bytes[i++] = (byte)(effect & 0xFF);
                bytes[i++] = (byte)((inn >> 8) & 0xFF);
                bytes[i++] = (byte)(inn & 0xFF);
            }

            return BytesToUrl(bytes);
        }

        private string BytesToUrl(byte[] bytes) => Constants.TreeAddress + Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-");
    }
}
