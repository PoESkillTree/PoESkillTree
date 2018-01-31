namespace PoESkillTree.Computation.Common
{
    // Node types in the stat subgraph of each IStat.
    // NodeTypes of parent nodes must be listed before their children. With that, iterating through NodeTypes can be 
    // used for top-down stat subgraph iteration.
    public enum NodeType
    {
        Total,
        Subtotal,
        // UncappedSubtotal actually describes two types:
        // The root node of each conversion/mod source path, and the node summing these up.
        UncappedSubtotal,
        // On paths with a converted base value, this node simply links to the conversion output node of the source.
        Base,
        BaseOverride,
        BaseSet,
        BaseAdd,
        Increase,
        More,
        TotalOverride
    }
}