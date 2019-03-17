using System.Threading.Tasks;
using PoESkillTree.Utils.UrlProcessing;

namespace PoESkillTree.Tests.Utils.UrlProcessing
{
    internal static class BuildUrlNormalizerExtensions
    {
        public static Task<string> NormalizeAsync(this BuildUrlNormalizer @this, string buildUrl)
            => @this.NormalizeAsync(buildUrl, (_, t) => t);
    }
}