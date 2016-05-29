using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles
{
    public class AscendancyClasses
    {
        public Dictionary<string, List<Class>> Classes;

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
            foreach(KeyValuePair<string, List<Class>> entry in Classes)
            {
                foreach(Class item in entry.Value)
                {
                    if (item.Name == ascClass)
                        return entry.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the number associated with the class 
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public int GetClassNumber(string ascClass)
        {
            var resClass = GetClass(ascClass);
            return resClass == null ? 0 : resClass.Order;
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
            foreach(KeyValuePair<string, string> pair in CharacterNames.NameToContent)
            {
                if (pair.Key == startingClass)
                    startingClass = pair.Value;
            }
            foreach (KeyValuePair<string, List<Class>> entry in Classes)
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
        /// Gets all Ascendancy classes associated with a starting class 
        /// </summary>
        /// <param name="startingClass"></param>
        /// <returns>A Class list or null if nothing was found</returns>
        public List<Class> GetClasses(string startingClass)
        {
            List<Class> classes;
            Classes.TryGetValue(startingClass, out classes);
            return classes;
        }

        /// <summary>
        /// Gets all Ascedancy classes
        /// </summary>
        /// <returns>A combined list of all Ascendancy classes</returns>
        public List<Class> GetClasses()
        {
            return Classes.Values.SelectMany(x => x).ToList();
        }

        public Class GetClass(string ascClass)
        {
            return GetClasses().FirstOrDefault(x => x.Name == ascClass);
        }
    }    
}
