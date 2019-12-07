using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Utils.UrlProcessing
{
    public class BuildUrlDataToUrlSerializer : SkillTreeSerializer
    {
        private readonly BuildUrlData _buildUrlData;
        private readonly IReadOnlyCollection<ushort> _skillNodes;

        public BuildUrlDataToUrlSerializer(BuildUrlData buildUrlData, IReadOnlyCollection<ushort> skillNodes)
        {
            _buildUrlData = buildUrlData;
            _skillNodes = skillNodes;
        }
        
        protected override byte[] ToUrlBytes()
        {
            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = _buildUrlData.SkilledNodesIds;
            skillNodes.Sort();

            var bytes = new byte[HeaderSize + skillNodes.Count * 2];
            SetCharacterBytes((byte)_buildUrlData.CharacterClass, (byte)_buildUrlData.AscendancyClassId, bytes);

            int unknownNodes = 0;
            int i = HeaderSize;
            foreach (var id in skillNodes)
            {
                if (_skillNodes.Contains(id))
                {
                    bytes[i++] = (byte)(id >> 8 & 0xFF);
                    bytes[i++] = (byte)(id & 0xFF);
                }
                else
                {
                    unknownNodes++;
                }
            }

            if (unknownNodes > 0)
            {
                var usedBytes = bytes.Length - unknownNodes * 2;
                byte[] result = new byte[usedBytes];
                Array.Copy(bytes, result, usedBytes);
                bytes = result;
            }

            return bytes;
        }
    }
}