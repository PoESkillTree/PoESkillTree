namespace PoESkillTree.Computation.Common
{
    public interface IValueTransformation
    {
        // In most cases, this returns a value that either modifies the output of the original value or decorates 
        // IValueCalculationContext instances passed to it to modify the inputs.
        IValue Transform(IValue value);
    }
}