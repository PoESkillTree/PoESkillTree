namespace POESKillTree.ViewModels
{
    internal class PoEBuild
    {
        public string Description;
        public string Name;
        public string Url;

        public PoEBuild(string name, string description, string url)
        {
            Name = name;
            Description = description;
            Url = url;
        }

        public override string ToString()
        {
            return Name + '\n' + Description;
        }
    }
}