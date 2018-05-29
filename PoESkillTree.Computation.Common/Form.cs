namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// The forms modifiers can have. These set how their values are used in stat calculations.
    /// </summary>
    /// <remarks>
    /// All modifiers have one of the forms, forms like "reduced" or "less" can be mapped to these and a value
    /// multiplier.
    /// </remarks>
    public enum Form
    {
        BaseOverride,
        BaseSet,
        BaseAdd,
        Increase,
        More,
        TotalOverride
    }
}