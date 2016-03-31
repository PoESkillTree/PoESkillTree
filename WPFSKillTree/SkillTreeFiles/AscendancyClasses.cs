using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree.SkillTreeFiles
{
    public class AscendancyClasses
    {
        public Dictionary<string, List<Class>> classes;

        public AscendancyClasses()
        {
            classes = new Dictionary<string, List<Class>>();
        }
        public class Class
        {
            public int order;
            public string name;
            public string displayName;
            public string flavourText;
            public Vector2D flavourTextRect;
            public int[] flavourTextColour;
        }
        /// <summary>
        /// Gets the starting class associated with an Ascendancy class
        /// </summary>
        /// <param name="ascClass"></param>
        /// <returns></returns>
        public string GetStartingClass(string ascClass)
        {
            foreach(KeyValuePair<string, List<Class>> entry in classes)
            {
                foreach(Class item in entry.Value)
                {
                    if (item.name == ascClass)
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
            foreach (KeyValuePair<string, List<Class>> entry in classes)
            {
                foreach (Class item in entry.Value)
                {
                    if (item.name == ascClass)
                        return item.order;
                }
            }
            return 0;
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
            foreach (KeyValuePair<string, List<Class>> entry in classes)
            {
                if (entry.Key == startingClass)
                {
                    if (ascOrder < entry.Value.Count)
                        return entry.Value[ascOrder].name;
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
            foreach(KeyValuePair<string, List<Class>> entry in classes)
            {
                if (entry.Key == startingClass)
                    return entry.Value;
            }
            return null;
        }
        /// <summary>
        /// Gets all Ascedancy classes
        /// </summary>
        /// <returns>A combined list of all Ascendancy classes</returns>
        public List<Class> GetClasses()
        {
            List<Class> allClasses = new List<Class>();
            foreach(KeyValuePair<string, List<Class>> entry in classes)
            {
                foreach (Class item in entry.Value)
                    allClasses.Add(item);
            }
            return allClasses;
        }
        public Class GetClass(string ascClass)
        {
            foreach (KeyValuePair<string, List<Class>> entry in classes)
            {
                foreach (Class item in entry.Value)
                {
                    if (item.name == ascClass)
                        return item;
                }
            }
            return null;
        }
    }    
}
