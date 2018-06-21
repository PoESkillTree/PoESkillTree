using PoESkillTree.Common.Utils;

namespace PoESkillTree.Computation.Common.Builders.Conditions
{
    public class ConditionBuilderResult
    {
        public ConditionBuilderResult(StatConverter statConverter)
            : this(statConverter, new Constant(true))
        {
            HasValue = false;
        }

        public ConditionBuilderResult(IValue value)
            : this(Funcs.Identity, value)
        {
            HasStatConverter = false;
        }

        public ConditionBuilderResult(StatConverter statConverter, IValue value)
        {
            HasStatConverter = true;
            StatConverter = statConverter;
            HasValue = true;
            Value = value;
        }

        public bool HasStatConverter { get; }
        public StatConverter StatConverter { get; }

        public bool HasValue { get; }
        public IValue Value { get; }

        public void Deconstruct(out StatConverter statConverter, out IValue value) =>
            (statConverter, value) = (StatConverter, Value);

        public static implicit operator ConditionBuilderResult((StatConverter statConverter, IValue value) t) =>
            new ConditionBuilderResult(t.statConverter, t.value);
    }
}