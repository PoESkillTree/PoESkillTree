namespace POESKillTree.Model
{
    public class Options
    {
        public string Theme { get; set; }
        public string Accent { get; set; } //Controlled by Menu Accent Headers
        public bool AttributesBarOpened  { get; set; }
        public bool BuildsBarOpened { get; set; }

        public Options()
        {
            Theme = "Dark";
            Accent = "Steel";
            AttributesBarOpened = false;
            BuildsBarOpened = false;
        }
    }
}
