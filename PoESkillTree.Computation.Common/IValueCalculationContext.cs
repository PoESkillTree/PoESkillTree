namespace PoESkillTree.Computation.Common
{
    public interface IValueCalculationContext
    {
        NodeValue? GetValue(IStat stat, NodeType nodeType = NodeType.Total);
    }
}