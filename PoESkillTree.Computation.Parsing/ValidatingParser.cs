using System;
using PoESkillTree.Common.Model.Items;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that makes sure stats were parsed completely and remaining is always empty when 
    /// <see cref="TryParse"/> returns true.
    /// It removes <see cref="ItemConstants.HiddenStatSuffix"/> from the decorated parser's remaining and trims it.
    /// If remaining is still not empty, <see cref="TryParse"/> returns false even if the decorated parser returned
    /// true.
    /// </summary>
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