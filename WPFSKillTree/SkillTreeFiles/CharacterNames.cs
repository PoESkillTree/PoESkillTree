using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public static class CharacterNames
    {
        public static readonly string Scion = "SEVEN";
        public static readonly string Marauder = "MARAUDER";
        public static readonly string Ranger = "RANGER";
        public static readonly string Witch = "WITCH";
        public static readonly string Duelist = "DUELIST";
        public static readonly string Templar = "TEMPLAR";
        public static readonly string Shadow = "SIX";

        public static List<KeyValuePair<string, string>> NameToContent = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(Scion, "Scion"),
            new KeyValuePair<string, string>(Marauder, "Marauder"),
            new KeyValuePair<string, string>(Ranger, "Ranger"),
            new KeyValuePair<string, string>(Witch, "Witch"),
            new KeyValuePair<string, string>(Duelist, "Duelist"),
            new KeyValuePair<string, string>(Templar, "Templar"),
            new KeyValuePair<string, string>(Shadow, "Shadow")
        };

        public static Dictionary<string, string> NameToLink = new Dictionary<string, string>
        {
            {Scion, SkillTree.GetCharacterURL(0)},
            {Marauder, SkillTree.GetCharacterURL(1)},
            {Ranger, SkillTree.GetCharacterURL(2)},
            {Witch, SkillTree.GetCharacterURL(3)},
            {Duelist, SkillTree.GetCharacterURL(4)},
            {Templar, SkillTree.GetCharacterURL(5)},
            {Shadow, SkillTree.GetCharacterURL(6)},
        };  
    }
}
