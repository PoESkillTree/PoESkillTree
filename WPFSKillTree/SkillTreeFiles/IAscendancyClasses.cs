using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    /// <summary>
    /// Represents methods for obtaining skill tree class names.
    /// </summary>
    public interface IAscendancyClasses
    {
        /// <summary>
        /// Gets the character class associated with an Ascendancy class.
        /// </summary>
        /// <param name="ascClass">The ascendancy class name.</param>
        /// <returns>The character class.</returns>
        string GetStartingClass(string ascClass);

        /// <summary>
        /// Returns the number associated with the ascendancy class.
        /// </summary>
        /// <param name="ascClass">The ascendancy class name.</param>
        /// <returns>The index of the specified ascendancy class.</returns>
        int GetClassNumber(string ascClass);

        /// <summary>
        /// Returns all ascendancy class names for the given character class.
        /// </summary>
        IEnumerable<string> AscendancyClassesForCharacter(string characterClass);

        /// <summary>
        /// Returns a class name based on character class and 0, 1, or 2 asc class.
        /// </summary>
        /// <param name="characterClass">The character class name.</param>
        /// <param name="ascOrder">The ascendancy class index.</param>
        /// <returns>The ascendancy class name.</returns>
        string GetClassName(string characterClass, int ascOrder);

        /// <summary>
        /// Returns a class name based on character class and 0, 1, or 2 asc class.
        /// </summary>
        /// <param name="charType">The character class id.</param>
        /// <param name="ascOrder">The ascendancy class index.</param>
        /// <returns>The ascendancy class name.</returns>
        string GetClassName(int charType, int ascOrder);

        /// <summary>
        /// Gets all Ascendancy classes associated with a character class.
        /// </summary>
        /// <param name="characterClass">The character class name.</param>
        /// <returns>A <see cref="Class"/> list or null if nothing was found</returns>
        IEnumerable<Class> GetClasses(string characterClass);

        /// <summary>
        /// Returns all Ascendancy classes by a class id.
        /// </summary>
        /// <param name="characterClass">A class id, used in encoded tree.</param>
        /// <returns>A <see cref="Class"/> list or null if nothing was found.</returns>
        IEnumerable<Class> GetClasses(int characterClass);

        /// <summary>
        /// Gets all Ascendancy classes.
        /// </summary>
        /// <returns>A combined list of all Ascendancy classes.</returns>
        IEnumerable<Class> GetAllClasses();

        /// <summary>
        /// Gets a character class for the specified ascendancy class.
        /// </summary>
        /// <param name="ascClass">The ascendancy class.</param>
        /// <returns>The character class corresponding to the provided <paramref name="ascClass"/>.</returns>
        Class GetClass(string ascClass);

        /// <summary>
        /// Gets a character class for the specified ascendancy class index.
        /// </summary>
        /// <param name="ascClass">The ascendancy class index.</param>
        /// <returns>The character class corresponding to the provided <paramref name="ascClass"/>.</returns>
        Class GetClass(int ascClass);
    }
}