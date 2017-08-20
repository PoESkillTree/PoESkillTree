using System;

namespace PoESkillTree.Computation.Parsing.Steps
{
    public class MappingStep<TStepIn, TStepOut, TData> : IStep<TStepOut, TData>
    {
        private readonly Func<TStepIn, TStepOut> _mapper;

        public IStep<TStepIn, TData> Inner { get; }

        public MappingStep(IStep<TStepIn, TData> inner, Func<TStepIn, TStepOut> mapper)
        {
            _mapper = mapper;
            Inner = inner;
        }

        public bool Completed => Inner.Completed;
        public bool Successful => Inner.Successful;

        public TStepOut Current => _mapper(Inner.Current);

        public IStep<TStepOut, TData> Next(TData data)
        {
            return new MappingStep<TStepIn, TStepOut, TData>(Inner.Next(data), _mapper);
        }
    }
}