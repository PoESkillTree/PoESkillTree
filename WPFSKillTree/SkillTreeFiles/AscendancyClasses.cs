using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.GameModel;

namespace PoESkillTree.SkillTreeFiles
{
    /// <summary>
    /// Represents methods for obtaining skill tree class names.
    /// </summary>
    public class AscendancyClasses : IAscendancyClasses
    {
        private readonly Dictionary<CharacterClass, List<Class>> _classes =
            new Dictionary<CharacterClass, List<Class>>();

        internal AscendancyClasses(Dictionary<int, baseToAscClass> ascClasses)
        {
            if (ascClasses == null) return;

            _classes.Clear();

            foreach (KeyValuePair<int, baseToAscClass> ascClass in ascClasses)
            {
                var classes = new List<Class>();
                foreach (KeyValuePair<int, classes> asc in ascClass.Value.classes)
                {
                    var newClass = new Class
                    {
                        Order = asc.Key,
                        DisplayName = asc.Value.displayName,
                        Name = asc.Value.name,
                        FlavourText = asc.Value.flavourText,
                        FlavourTextColour = asc.Value.flavourTextColour.Split(',').Select(int.Parse).ToArray()
                    };
                    int[] tempPointList = asc.Value.flavourTextRect.Split(',').Select(int.Parse).ToArray();
                    newClass.FlavourTextRect = new Vector2D(tempPointList[0], tempPointList[1]);
                    classes.Add(newClass);

                }

                var characterClass = Enums.Parse<CharacterClass>(ascClass.Value.name);
                _classes.Add(characterClass, classes);
            }
        }

        public CharacterClass GetStartingClass(string ascClass)
            => (from entry in _classes where entry.Value.Any(item => item.Name == ascClass) select entry.Key)
                .FirstOrDefault();

        public int GetAscendancyClassNumber(string ascClass)
            => GetClass(ascClass)?.Order ?? 0;

        public IEnumerable<string> AscendancyClassesForCharacter(CharacterClass characterClass)
            => GetClasses(characterClass).Select(c => c.DisplayName);

        public string GetAscendancyClassName(CharacterClass characterClass, int ascOrder)
        {
            if (ascOrder > 0)
                ascOrder -= 1;
            var classes = _classes[characterClass];
            if (ascOrder < classes.Count)
                return classes[ascOrder].Name;
            return null;
        }

        public IEnumerable<Class> GetClasses(CharacterClass characterClass)
            => _classes[characterClass];

        public Class GetClass(string ascClass)
            => _classes.Values.SelectMany(x => x).FirstOrDefault(x => x.Name == ascClass);
    }
}
