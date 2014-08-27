namespace POESKillTree.Model
{
    public enum OptionsTheme
    {
        Light,
        Dark
    }
    public class Options
    {
        public string SkillTreeAddress { get; set; }
        public string CharacterLevel { get; set; }
        public OptionsTheme OptionsTheme { get; set; }

        public Options()
        {
            SkillTreeAddress = "http://www.pathofexile.com/passive-skill-tree/AAAAAgMA";
            OptionsTheme = OptionsTheme.Dark;
            CharacterLevel = "1";
        }
    }
}
