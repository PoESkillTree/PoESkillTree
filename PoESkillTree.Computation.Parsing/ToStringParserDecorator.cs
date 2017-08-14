namespace PoESkillTree.Computation.Parsing
{
    public class ToStringParserDecorator<T> : IParser<string>
    {
        private readonly IParser<T> _decoratedParser;

        public ToStringParserDecorator(IParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public bool TryParse(string stat, out string result)
        {
            var ret = _decoratedParser.TryParse(stat, out var innerResult);
            result = innerResult?.ToString();
            return ret;
        }
    }
}