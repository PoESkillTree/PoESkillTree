using System.Collections;
using System.Collections.Generic;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinitionExtensionCollection : IEnumerable<(string, SkillDefinitionExtension)>
    {
        private readonly List<(string, SkillDefinitionExtension)> _collection =
            new List<(string, SkillDefinitionExtension)>();

        public IEnumerator<(string, SkillDefinitionExtension)> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(string skillId,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), parts);

        public void Add(string skillId,
            SkillPartDefinitionExtension commonExtension,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, commonExtension, new Dictionary<string, IReadOnlyList<Entity>>(), parts);

        public void Add(string skillId,
            IReadOnlyDictionary<string, IReadOnlyList<Entity>> buffStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), buffStats, parts);

        public void Add(string skillId,
            IEnumerable<string> passiveStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), passiveStats, parts);

        public void Add(string skillId,
            SkillPartDefinitionExtension commonExtension, IReadOnlyDictionary<string, IReadOnlyList<Entity>> buffStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, commonExtension, buffStats, new string[0], parts);

        public void Add(string skillId,
            SkillPartDefinitionExtension commonExtension, IEnumerable<string> passiveStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, commonExtension, new Dictionary<string, IReadOnlyList<Entity>>(), passiveStats, parts);

        public void Add(string skillId,
            IReadOnlyDictionary<string, IReadOnlyList<Entity>> buffStats, IEnumerable<string> passiveStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), buffStats, passiveStats, parts);

        public void Add(string skillId,
            SkillPartDefinitionExtension commonExtension,
            IReadOnlyDictionary<string, IReadOnlyList<Entity>> buffStats, IEnumerable<string> passiveStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => _collection.Add((skillId,
                new SkillDefinitionExtension(commonExtension, buffStats, passiveStats, parts)));
    }
}