using System.Collections.Generic;
using System.Linq;

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
            {Scion, SkillTree.GetCharacterUrl(0)},
            {Marauder, SkillTree.GetCharacterUrl(1)},
            {Ranger, SkillTree.GetCharacterUrl(2)},
            {Witch, SkillTree.GetCharacterUrl(3)},
            {Duelist, SkillTree.GetCharacterUrl(4)},
            {Templar, SkillTree.GetCharacterUrl(5)},
            {Shadow, SkillTree.GetCharacterUrl(6)},
        };

        public static string GetClassNameFromChartype(int chartype)
        {
            return NameToContent.First(x => x.Key == SkillTree.CharName[chartype]).Value;
        }
    }
}
