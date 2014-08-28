namespace POESKillTree.ViewModels
{
    public class PoEBuild
    {
        public string Description;
        public string Name;
        public string Url;
        public string Note;

        public PoEBuild()
        {
            
        }

        public PoEBuild(string name, string description, string url, string note)
        {
            Name = name;
            Description = description;
            Url = url;
            Note = note;
        }

        public override string ToString()
        {
            return Name + '\n' + Description;
        }
    }
}