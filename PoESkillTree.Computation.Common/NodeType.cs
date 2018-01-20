namespace PoESkillTree.Computation.Common
{
    // Node types in the main calculation building block, which is the same for each IStat.
    // Each of these NodeType has one corresponding node implementation defining its behavior.
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
        EffectiveBaseAdd,
        BaseAdd,
        Increase,
        More,
        TotalOverride
    }
}