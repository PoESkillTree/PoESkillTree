namespace PoESkillTree.ViewModels.Import
{
    public class AccountCharacterViewModel
    {
        public AccountCharacterViewModel(string name, string league, string @class, int classId, int ascendancyClass, int level)
        {
            Name = name;
            League = league;
            Class = @class;
            ClassId = classId;
            AscendancyClass = ascendancyClass;
            Level = level;
        }

        public string Name { get; }
        public string League { get; }
        public string Class { get; }
        public int ClassId { get; }
        public int AscendancyClass { get; }
        public int Level { get; }
    }
}