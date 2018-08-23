using System.Collections.Generic;
using System.Text.RegularExpressions;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    public class RegexCache
    {
        private readonly RegexOptions _options;
        private readonly IDictionary<string, Regex> _cache = new Dictionary<string, Regex>();

        public RegexCache(RegexOptions options)
        {
            _options = options;
        }

        public Regex this[string pattern] => _cache.GetOrAdd(pattern, p => new Regex(p, _options));
    }
}