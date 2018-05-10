namespace PoESkillTree.Computation.Common
{
    // Node types in the stat subgraph of each IStat.
    // NodeTypes of parent nodes must be listed before their children. With that, iterating through NodeTypes can be 
    // used for top-down stat subgraph iteration.
    public enum NodeType
    {
        Total,
        Subtotal,
        UncappedSubtotal,
        PathTotal,
        Base,
        BaseOverride,
        BaseSet,
        BaseAdd,
        Increase,
        More,
        TotalOverride
    }
}