using System;
using System.Linq;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Serializes provided instance of the <see cref="SkillTree"/> class into url representation.
    /// </summary>
    public class SkillTreeSerializer
    {
        private const int HeaderSize = 7;
        private const int Version = 4;
        private const byte Fullscreen = 0;

        private readonly SkillTree _skillTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTreeSerializer"/> class.
        /// </summary>
        /// <param name="skillTree">The skill tree.</param>
        public SkillTreeSerializer(SkillTree skillTree)
        {
            _skillTree = skillTree;
        }

        /// <summary>
        /// Serializes specified skill tree into the https://pathofexile.com url.
        /// </summary>
        public string ToUrl()
        {
            // Ordering provides nice exact Url match, but is not strictly needed.
            // Performance impact is minimal even on tree with all 1.3K nodes allocated.
            var skillNodes = _skillTree.SkilledNodes
                .Where(node => !node.IsAscendancyStart && !SkillTree.RootNodeList.Contains(node.Id))
                .OrderBy(node => node.Id);

            var bytes = new byte[HeaderSize + skillNodes.Count() * 2];

            bytes[0] = (byte)(Version >> 24 & 0xFF);
            bytes[1] = (byte)(Version >> 16 & 0xFF);
            bytes[2] = (byte)(Version >> 8 & 0xFF);
            bytes[3] = (byte)(Version >> 0 & 0xFF);
            bytes[4] = (byte)_skillTree.Chartype;
            bytes[5] = (byte)_skillTree.AscType;
            bytes[6] = Fullscreen;

            int i = HeaderSize;
            foreach (var inn in skillNodes)
            {
                bytes[i++] = (byte)(inn.Id >> 8 & 0xFF);
                bytes[i++] = (byte)(inn.Id & 0xFF);
            }

            return Constants.TreeAddress + Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-");
        }
    }
}