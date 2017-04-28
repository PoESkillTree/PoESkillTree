namespace POESKillTree.Utils.WikiApi
{
    /// <summary>
    /// Collection of RDF predicates used on item pages.
    /// </summary>
    public static class ItemRdfPredicates
    {
        // Generic
        public const string RdfName = "Has name";
        public const string RdfRarity = "Has rarity";
        public const string RdfItemClass = "Has item class";
        public const string RdfDropEnabled = "Is drop enabled";
        public const string RdfIcon = "Has inventory icon";
        public const string RdfInventoryHeight = "Has inventory height";
        public const string RdfInventoryWidth = "Has inventory width";
        public const string RdfMetadataId = "Has metadata id";
        public const string RdfBaseMetadataId = "Has base item metadata id";

        // Requirements
        public const string RdfLvlReq = "Has level requirement";
        public const string RdfBaseDexReq = "Has base dexterity requirement";
        public const string RdfBaseIntReq = "Has base intelligence requirement";
        public const string RdfBaseStrReq = "Has base strength requirement";

        // Stats (not properties)
        public const string RdfImplicits = "Has implicit stat text";
        public const string RdfExplicits = "Has explicit stat text";

        // Weapon properties
        public const string RdfBasePhysMin = "Has base minimum physical damage";
        public const string RdfBasePhysMax = "Has base maximum physical damage";
        public const string RdfBaseCritChance = "Has base critical strike chance";
        public const string RdfBaseAttackSpeed = "Has base attack speed";
        public const string RdfBaseWeaponRange = "Has base weapon range";

        // Armour properties
        public const string RdfBaseBlock = "Has base block";
        public const string RdfBaseArmour = "Has base armour";
        public const string RdfBaseEvasion = "Has base evasion";
        public const string RdfBaseEnergyShield = "Has base energy shield";

        // Unique jewel properties
        public const string RdfItemLimit = "Has item limit";
        public const string RdfJewelRadius = "Has jewel radius";
    }
}