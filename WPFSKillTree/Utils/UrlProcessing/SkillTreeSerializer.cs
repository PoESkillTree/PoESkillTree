using System;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Serializes provided instance of the <see cref="SkillTree"/> class into url representation.
    /// </summary>
    public abstract class SkillTreeSerializer
    {
        protected const int HeaderSize = 7;
        private const byte Version = 4;
        private const byte Fullscreen = 0;

        /// <summary>
        /// Serializes specified skill tree into the https://pathofexile.com url.
        /// </summary>
        public string ToUrl()
        {
            var urlSuffix = Convert.ToBase64String(ToUrlBytes())
                .Replace("/", "_").Replace("+", "-");
            return Constants.TreeAddress + urlSuffix;
        }

        protected abstract byte[] ToUrlBytes();

        protected static void SetCharacterBytes(byte characterClassId, byte ascendancyClassId, byte[] target)
        {
            target[0] = 0;
            target[1] = 0;
            target[2] = 0;
            target[3] = Version;
            target[4] = characterClassId;
            target[5] = ascendancyClassId;
            target[6] = Fullscreen;
        }
    }
}