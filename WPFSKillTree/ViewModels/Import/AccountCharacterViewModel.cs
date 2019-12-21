namespace PoESkillTree.ViewModels.Import
{
    public class AccountCharacterViewModel
    {
        public AccountCharacterViewModel(string name, string league, string @class, int level)
        {
            Name = name;
            League = league;
            Class = @class;
            Level = level;
        }

        public string Name { get; }
        public string League { get; }
        public string Class { get; }
        public int Level { get; }
    }
}