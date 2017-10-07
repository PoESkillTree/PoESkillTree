using System;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class MatchContextStub<T> : BuilderStub, IMatchContext<T>
    {
        private readonly Func<string, T> _tFactory;

        public MatchContextStub(string stringRepresentation, Func<string, T> tFactory) 
            : base(stringRepresentation)
        {
            _tFactory = tFactory;
        }

        public T this[int index] => _tFactory($"{this}[{index}]");

        public T First => _tFactory($"{this}.First");
        public T Last => _tFactory($"{this}.Last");
        public T Single => _tFactory($"{this}.Single");
    }


    public class MatchContextsStub : IMatchContexts
    {
        public IMatchContext<IGroupConverter> Groups =>
            new MatchContextStub<IGroupConverter>("Groups",
                s => new GroupConverterStub(s));

        public IMatchContext<ValueBuilder> Values => new ValueMatchContext();


        private class ValueMatchContext : BuilderStub, IMatchContext<ValueBuilder>
        {
            public ValueMatchContext() : base("Values")
            {
            }

            public ValueBuilder this[int index] =>
                new ValueBuilder(new ValueBuilderStub($"{this}[{index}]", (_, c) => c[index]));

            public ValueBuilder First =>
                new ValueBuilder(new ValueBuilderStub($"{this}.First", (_, c) => c.First));

            public ValueBuilder Last =>
                new ValueBuilder(new ValueBuilderStub($"{this}.Last", (_, c) => c.Last));

            public ValueBuilder Single =>
                new ValueBuilder(new ValueBuilderStub($"{this}.Single", (_, c) => c.Single));
        }
    }
}