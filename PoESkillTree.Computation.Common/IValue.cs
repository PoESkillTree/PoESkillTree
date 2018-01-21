namespace PoESkillTree.Computation.Common
{
    public interface IValue
    {
        // Values can affect NodeValue.Minimum and/or .Maximum. Depending on how IValue is implement, that may need to
        // be exposed as a property.
    }
}