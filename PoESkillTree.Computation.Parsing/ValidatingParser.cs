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
            var ret = _inner.TryParse(stat, out remaining, out result);

            if (remaining.EndsWith(ItemConstants.HiddenStatSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                remaining = remaining.Remove(remaining.Length - ItemConstants.HiddenStatSuffix.Length);
            }

            remaining = remaining.Trim();
            return ret && remaining == string.Empty;
        }
    }
}