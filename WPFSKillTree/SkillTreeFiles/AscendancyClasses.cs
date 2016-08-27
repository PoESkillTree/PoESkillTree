using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles
{
    public class AscendancyClasses
    {
        public readonly Dictionary<string, List<Class>> Classes;

        public AscendancyClasses()
        {
            Classes = new Dictionary<string, List<Class>>();
        }

        public class Class
        {
            public int Order;
            public string Name;
            public string DisplayName;
            public string FlavourText;
            public Vector2D FlavourTextRect;
            public int[] FlavourTextColour;
        }

        /// <summary>
        /// Gets the starting class associated with an Ascendancy class
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public string GetStartingClass(string ascClass)
        {
            return (from entry in Classes where entry.Value.Any(item => item.Name == ascClass) select entry.Key).FirstOrDefault();
        }

        /// <summary>
        /// Returns the number associated with the class 
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public int GetClassNumber(string ascClass)
        {
            var resClass = GetClass(ascClass);
            return resClass?.Order ?? 0;
        }

        /// <summary>
        /// Returns a class name based on starting class and 0, 1, or 2 asc class
        /// </summary>
        /// <param name="startingClass"></param>
        /// <param name="ascOrder">Get this from the tree decoding</param>
        /// <returns></returns>
        public string GetClassName(string startingClass, int ascOrder)
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
        public string GetClassName(int charType, int ascOrder)
        {
            return GetClassName(CharacterNames.GetClassNameFromChartype(charType), ascOrder);
        }

        /// <summary>
        /// Gets all Ascendancy classes associated with a starting class 
        /// </summary>
        /// <param name="startingClass"></param>
        /// <returns>A Class list or null if nothing was found</returns>
        public IEnumerable<Class> GetClasses(string startingClass)
        {
            List<Class> classes;
            Classes.TryGetValue(startingClass, out classes);
            return classes;
        }

        public IEnumerable<Class> GetClasses(int startingClass)
        {
            return GetClasses(CharacterNames.GetClassNameFromChartype(startingClass));
        }

        /// <summary>
        /// Gets all Ascedancy classes
        /// </summary>
        /// <returns>A combined list of all Ascendancy classes</returns>
        public IEnumerable<Class> GetClasses()
        {
            return Classes.Values.SelectMany(x => x).ToList();
        }

        public Class GetClass(string ascClass)
        {
            return GetClasses().FirstOrDefault(x => x.Name == ascClass);
        }

        public Class GetClass(int ascClass)
        {
            return GetClasses().FirstOrDefault(x => x.Name == CharacterNames.GetClassNameFromChartype(ascClass));
        }
    }    
}
