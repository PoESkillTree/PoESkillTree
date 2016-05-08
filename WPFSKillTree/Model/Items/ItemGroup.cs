namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Defines groups of <see cref="ItemType"/>s.
    /// <para/>
    /// The <see cref="ItemType"/>s belonging to a <see cref="ItemGroup"/> can 
    /// be accessed via <see cref="ItemTypExtensions"/>.
    /// </summary>
    public enum ItemGroup
    {
        Unknown,
        OneHandWeapon,
        TwoHandWeapon,
        Belt,
        Ring,
        Amulet,
        Quiver,
        BodyArmour,
        Boots,
        Gloves,
        Helmet,
        Shield,
        Jewel
    }
}