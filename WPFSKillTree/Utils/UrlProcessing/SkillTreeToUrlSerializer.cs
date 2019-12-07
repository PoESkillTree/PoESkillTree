using System.Linq;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Utils.UrlProcessing
{
    public class SkillTreeToUrlSerializer : SkillTreeSerializer
    {
        private readonly SkillTree _skillTree;

        public SkillTreeToUrlSerializer(SkillTree skillTree)
        {
            _skillTree = skillTree;
        }
        
        protected override byte[] ToUrlBytes()
        {
            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = _skillTree.SkilledNodes
                .Where(node => !node.IsAscendancyStart && !node.IsRootNode)
                .Select(node => node.Id)
                .OrderBy(n => n)
                .ToList();

            var bytes = new byte[HeaderSize + skillNodes.Count * 2];
            SetCharacterBytes((byte) _skillTree.CharClass, (byte) _skillTree.AscType, bytes);

            int i = HeaderSize;
            foreach (var inn in skillNodes)
            {
                bytes[i++] = (byte)(inn >> 8 & 0xFF);
                bytes[i++] = (byte)(inn & 0xFF);
            }

            return bytes;
        }
    }
}