using System;
using System.Collections.Generic;
using System.Text;

namespace PoESkillTree.Utils.UrlProcessing.Decoders
{
    public interface ISkillTreeUrlDecoder
    {
        public bool CanDecode(string url);
        public SkillTreeUrlData Decode(string url);
    }
}
