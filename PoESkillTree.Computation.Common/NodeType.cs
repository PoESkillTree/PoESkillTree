namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// The possible types of nodes in <see cref="IStat"/> calculation subgraphs (not including nodes from modifiers,
    /// those have <see cref="Form"/> as their type).
    /// </summary>
    public enum NodeType
    {
        // NodeTypes of parent nodes must be listed before their children. With that, iterating through NodeTypes can be 
        // used for top-down stat subgraph iteration.
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