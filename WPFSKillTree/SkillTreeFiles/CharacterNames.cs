using System.Collections.Generic;
using System.Linq;
using POESKillTree.Utils.UrlProcessing;

namespace POESKillTree.SkillTreeFiles
{
    public enum CharacterClasses
    {
        Scion = 0,
        Marauder = 1,
        Ranger = 2,
        Witch = 3,
        Duelist = 4,
        Templar = 5,
        Shadow = 6
    }

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
            {Scion, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Scion)},
            {Marauder, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Marauder)},
            {Ranger, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Ranger)},
            {Witch, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Witch)},
            {Duelist, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Duelist)},
            {Templar, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Templar)},
            {Shadow, SkillTreeSerializer.GetEmptyBuildUrl((byte)CharacterClasses.Shadow)},
        };

        public static string GetClassNameFromChartype(int chartype)
        {
            return NameToContent.First(x => x.Key == SkillTree.CharName[chartype]).Value;
        }
    }
}
