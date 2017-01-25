using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles
{
    public static class AscendancyClasses
    {
        public static readonly Dictionary<string, List<Class>> Classes = new Dictionary<string, List<Class>>();

        public class Class
        {
            public int Order;
            public string Name;
            public string DisplayName;
            public string FlavourText;
            public Vector2D FlavourTextRect;
            public int[] FlavourTextColour;
        }

        internal static void Initialize(Dictionary<int, baseToAscClass> ascClasses)
        {
            if (ascClasses == null) return;

            Classes.Clear();

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

                Classes.Add(ascClass.Value.name, classes);
            }
        }

        /// <summary>
        /// Gets the starting class associated with an Ascendancy class
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public static string GetStartingClass(string ascClass)
        {
            return (from entry in Classes where entry.Value.Any(item => item.Name == ascClass) select entry.Key).FirstOrDefault();
        }

        /// <summary>
        /// Returns the number associated with the class 
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public static int GetClassNumber(string ascClass)
        {
            var resClass = GetClass(ascClass);
            return resClass?.Order ?? 0;
        }

        /// <summary>
        /// Returns all ascendancy class names for the given character class.
        /// </summary>
        public static IEnumerable<string> AscendancyClassesForCharacter(string characterClass)
        {
            return GetClasses(characterClass).Select(c => c.DisplayName);
        }

        /// <summary>
        /// Returns a class name based on starting class and 0, 1, or 2 asc class
        /// </summary>
        /// <param name="startingClass"></param>
        /// <param name="ascOrder">Get this from the tree decoding</param>
        /// <returns></returns>
        public static string GetClassName(string startingClass, int ascOrder)
        {
            if (ascOrder > 0)
                ascOrder -= 1;
            foreach(var pair in CharacterNames.NameToContent)
            {
                if (pair.Key == startingClass)
                    startingClass = pair.Value;
            }
            foreach (var entry in Classes)
            {
                if (entry.Key == startingClass)
                {
                    if (ascOrder < entry.Value.Count)
                        return entry.Value[ascOrder].Name;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns a class name based on starting class and 0, 1, or 2 asc class
        /// </summary>
        /// <param name="charType"></param>
        /// <param name="ascOrder">Get this from the tree decoding</param>
        /// <returns></returns>
        public static string GetClassName(int charType, int ascOrder)
        {
            return GetClassName(CharacterNames.GetClassNameFromChartype(charType), ascOrder);
        }

        /// <summary>
        /// Gets all Ascendancy classes associated with a starting class 
        /// </summary>
        /// <param name="startingClass"></param>
        /// <returns>A Class list or null if nothing was found</returns>
        public static IEnumerable<Class> GetClasses(string startingClass)
        {
            List<Class> classes;
            Classes.TryGetValue(startingClass, out classes);
            return classes;
        }

        /// <summary>
        /// Returns all Ascendancy classes by a class id.
        /// </summary>
        /// <param name="startingClass">A class id, used in encoded tree.</param>
        /// <returns>A Class list or null if nothing was found.</returns>
        public static IEnumerable<Class> GetClasses(int startingClass)
        {
            return GetClasses(CharacterNames.GetClassNameFromChartype(startingClass));
        }

        /// <summary>
        /// Gets all Ascendancy classes
        /// </summary>
        /// <returns>A combined list of all Ascendancy classes</returns>
        public static IEnumerable<Class> GetAllClasses()
        {
            return Classes.Values.SelectMany(x => x).ToList();
        }

        public static Class GetClass(string ascClass)
        {
            return GetAllClasses().FirstOrDefault(x => x.Name == ascClass);
        }

        public static Class GetClass(int ascClass)
        {
            return GetAllClasses().FirstOrDefault(x => x.Name == CharacterNames.GetClassNameFromChartype(ascClass));
        }
    }
}
