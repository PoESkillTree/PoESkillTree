namespace POESKillTree.Utils.WikiApi
{
    /// <summary>
    /// Collection of names of Cargo tables and fields.
    /// </summary>
    public static class CargoConstants
    {
        public const string PageName = "_pageName";

        public const string ItemTableName = "items";

        // Relevant fields of the items table
        public const string Name = "name";
        public const string BaseItemId = "base item id";
        public const string RequiredLevel = "required level";
        public const string ExplicitMods = "explicit mods";
        public const string DropEnabled = "drop enabled";
        public const string Rarity = "rarity";
        public const string ItemClass = "class";
        public const string InventoryIcon = "inventory icon";
        
        public const string JewelTableName = "jewels";

        // Relevant fields of the jewels table
        public const string JewelLimit = "item limit";
    }
}