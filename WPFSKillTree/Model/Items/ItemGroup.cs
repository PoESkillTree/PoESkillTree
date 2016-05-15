namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Defines groups of <see cref="ItemType"/>s and defines in which slot an item can be slotted.
    /// <para/>
    /// The <see cref="ItemType"/>s belonging to a <see cref="ItemGroup"/> can 
    /// be accessed via <see cref="ItemTypExtensions"/>.
    /// </summary>
    public enum ItemGroup
    {
        Unknown,
        OneHandedWeapon,
        TwoHandedWeapon,
        Belt,
        Ring,
        Amulet,
        Quiver,
        BodyArmour,
        Boots,
        Gloves,
        Helmet,
        Shield,
        Jewel,
        Gem
    }
}