using System;
using PoESkillTree.Common.Model.Items;

namespace PoESkillTree.Computation.Parsing
{
    public class ValidatingParser<TResult> : IParser<TResult>
    {
        private readonly IParser<TResult> _inner;

        public ValidatingParser(IParser<TResult> inner)
        {
            _inner = inner;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            if (!_inner.TryParse(stat, out remaining, out result))
            {
                return false;
            }
            var processed = remaining;
            if (processed.EndsWith(ItemConstants.HiddenStatSuffix,
                StringComparison.InvariantCultureIgnoreCase))
            {
                processed =
                    processed.Remove(processed.Length - ItemConstants.HiddenStatSuffix.Length);
            }
            return processed.Trim() == string.Empty;
        }
    }
}