using System.Collections.Generic;
using PoESkillTree.GameModel;

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
        CharacterClass GetStartingClass(string ascClass);

        /// <summary>
        /// Returns the number associated with the ascendancy class.
        /// </summary>
        /// <param name="ascClass">The ascendancy class name.</param>
        /// <returns>The index of the specified ascendancy class.</returns>
        int GetAscendancyClassNumber(string ascClass);

        /// <summary>
        /// Returns all ascendancy class names for the given character class.
        /// </summary>
        IEnumerable<string> AscendancyClassesForCharacter(CharacterClass characterClass);

        /// <summary>
        /// Returns a class name based on character class and 0, 1, or 2 asc class.
        /// </summary>
        /// <param name="characterClass">The character class.</param>
        /// <param name="ascOrder">The ascendancy class index.</param>
        /// <returns>The ascendancy class name.</returns>
        string GetAscendancyClassName(CharacterClass characterClass, int ascOrder);

        /// <summary>
        /// Gets all Ascendancy classes associated with a character class.
        /// </summary>
        /// <param name="characterClass">The character class.</param>
        /// <returns>A <see cref="Class"/> list</returns>
        IEnumerable<Class> GetClasses(CharacterClass characterClass);

        /// <summary>
        /// Gets a character class for the specified ascendancy class.
        /// </summary>
        /// <param name="ascClass">The ascendancy class.</param>
        /// <returns>The character class corresponding to the provided <paramref name="ascClass"/>.</returns>
        Class GetClass(string ascClass);
    }
}