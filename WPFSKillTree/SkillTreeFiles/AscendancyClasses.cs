using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles
{
    /// <summary>
    /// Represents methods for obtaining skill tree class names.
    /// </summary>
    public class AscendancyClasses : IAscendancyClasses
    {
        private readonly Dictionary<string, List<Class>> _classes = new Dictionary<string, List<Class>>();

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

                _classes.Add(ascClass.Value.name, classes);
            }
        }

        public string GetStartingClass(string ascClass)
        {
            return (from entry in _classes where entry.Value.Any(item => item.Name == ascClass) select entry.Key).FirstOrDefault();
        }

        public int GetClassNumber(string ascClass)
        {
            var resClass = GetClass(ascClass);
            return resClass?.Order ?? 0;
        }

        public IEnumerable<string> AscendancyClassesForCharacter(string characterClass)
        {
            return GetClasses(characterClass).Select(c => c.DisplayName);
        }

        public string GetClassName(string startingClass, int ascOrder)
        {
            if (ascOrder > 0)
                ascOrder -= 1;
            foreach(var pair in CharacterNames.NameToContent)
            {
                if (pair.Key == startingClass)
                    startingClass = pair.Value;
            }
            foreach (var entry in _classes)
            {
                if (entry.Key == startingClass)
                {
                    if (ascOrder < entry.Value.Count)
                        return entry.Value[ascOrder].Name;
                }
            }
            return null;
        }

        public string GetClassName(int charType, int ascOrder)
        {
            return GetClassName(CharacterNames.GetClassNameFromChartype(charType), ascOrder);
        }

        public IEnumerable<Class> GetClasses(string startingClass)
        {
            List<Class> classes;
            _classes.TryGetValue(startingClass, out classes);
            return classes;
        }

        public IEnumerable<Class> GetClasses(int startingClass)
        {
            return GetClasses(CharacterNames.GetClassNameFromChartype(startingClass));
        }

        public IEnumerable<Class> GetAllClasses()
        {
            return _classes.Values.SelectMany(x => x).ToList();
        }

        public Class GetClass(string ascClass)
        {
            return GetAllClasses().FirstOrDefault(x => x.Name == ascClass);
        }

        public Class GetClass(int ascClass)
        {
            return GetAllClasses().FirstOrDefault(x => x.Name == CharacterNames.GetClassNameFromChartype(ascClass));
        }
    }
}
