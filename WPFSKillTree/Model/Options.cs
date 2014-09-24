using POESKillTree.ViewModels;

namespace POESKillTree.Model
{
    public enum OptionsTheme
    {
        Light,
        Dark
    }
    public class Options
    {
        public OptionsTheme OptionsTheme { get; set; }
        public bool AttributesBarOpened  { get; set; }
        public bool BuildsBarOpened { get; set; }

        public Options()
        {
            OptionsTheme = OptionsTheme.Dark;
            AttributesBarOpened = false;
            BuildsBarOpened = false;
        }
    }
}
