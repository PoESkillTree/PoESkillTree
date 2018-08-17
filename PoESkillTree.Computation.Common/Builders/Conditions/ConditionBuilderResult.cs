using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common.Builders.Conditions
{
    public class ConditionBuilderResult
    {
        public ConditionBuilderResult(StatConverter statConverter)
            : this(statConverter, null)
        {
        }

        public ConditionBuilderResult(IValue value)
            : this(null, value)
        {
        }

        public ConditionBuilderResult(StatConverter statConverter = null, IValue value = null)
        {
            HasStatConverter = !(statConverter is null);
            StatConverter = statConverter ?? Funcs.Identity;
            HasValue = !(value is null);
            Value = value ?? new Constant(true);
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