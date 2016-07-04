namespace POESKillTree.SkillTreeFiles
{
    public static class Constants
    {
        public const float LifePerLevel = 12;
        public const float AccPerLevel = 2;
        public const float EvasPerLevel = 3;
        public const float ManaPerLevel = 6;
        public const float IntPerMana = 2;
        public const float IntPerES = 5; //%
        public const float StrPerLife = 2;
        public const float StrPerED = 5; //%
        public const float DexPerAcc = 0.5f;
        public const float DexPerEvas = 5; //%

        public const string TreeAddress = "https://www.pathofexile.com/passive-skill-tree/";
        public const string TreeRegex =
            @"(http(|s):\/\/|).*?(character(\/|)|passive-skill-tree(\/|)|fullscreen-passive-skill-tree(\/|)|#|poeplanner.com(\/|))";

        public const string DefaultTree = "https://www.pathofexile.com/passive-skill-tree/AAAABAMAAA==";

        /// <summary>
        /// The image files (node sprites and assets) come in 4 sizes.
        /// This specifies the index of PoESkillTree.imageZoomLevels to take for assets and the index
        /// of PoESkillTree.skillSprites[s] to take the sprite file from.
        /// </summary>
        public const int AssetZoomLevel = 3;
    }
}