namespace PoESkillTree.Computation.Common
{
    public interface IValue
    {
        NodeValue? Calculate(IValueCalculationContext context);
    }
}