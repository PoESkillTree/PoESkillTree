using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;

namespace PoESkillTree.SkillTreeFiles
{
    /// <summary>
    /// Represents methods for obtaining skill tree class names.
    /// </summary>
    public class AscendancyClasses : IAscendancyClasses
    {
        private readonly Dictionary<CharacterClass, List<Class>> _classes =
            new Dictionary<CharacterClass, List<Class>>();

        internal AscendancyClasses(IReadOnlyCollection<JsonPassiveTreeCharacterClass> characters)
        {
            if (characters == null) return;

            _classes.Clear();

            foreach (var character in characters)
            {
                var classes = new List<Class>();
                var i = 0;
                foreach (var ascendancy in character.AscendancyClasses)
                {
                    classes.Add(new Class(++i, ascendancy.Id, ascendancy.Name, ascendancy.FlavourText, new Vector2D(ascendancy.FlavourTextBounds.X, ascendancy.FlavourTextBounds.Y), ascendancy.FlavourTextColour));
                }

                var characterClass = Enums.Parse<CharacterClass>(character.Name);
                _classes[characterClass] = classes;
            }
        }

        public CharacterClass GetStartingClass(string ascClass)
            => (from entry in _classes where entry.Value.Any(item => item.Name == ascClass) select entry.Key)
                .FirstOrDefault();

        public int GetAscendancyClassNumber(string ascClass)
            => GetClass(ascClass)?.Order ?? 0;

        public IEnumerable<string> AscendancyClassesForCharacter(CharacterClass characterClass)
            => GetClasses(characterClass).Select(c => c.DisplayName);

        public string? GetAscendancyClassName(CharacterClass characterClass, int ascOrder)
        {
            if (_classes.Count == 0)
                return null;

            if (ascOrder > 0)
                ascOrder -= 1;
            var classes = _classes[characterClass];
            if (ascOrder < classes.Count)
                return classes[ascOrder].Name;
            return null;
        }

        public IEnumerable<Class> GetClasses(CharacterClass characterClass)
            => _classes.ContainsKey(characterClass) ? _classes[characterClass] : new List<Class>();

        public Class GetClass(string ascClass)
            => _classes.Values.SelectMany(x => x).FirstOrDefault(x => x.Name == ascClass);
    }
}
