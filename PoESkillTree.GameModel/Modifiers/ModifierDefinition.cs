using System.Collections.Generic;

namespace PoESkillTree.GameModel.Modifiers
{
    public class ModifierDefinition
    {
        public ModifierDefinition(
            string id, ModDomain domain, ModGenerationType generationType,
            IReadOnlyList<ModifierSpawnWeight> spawnWeights, IReadOnlyList<CraftableStat> stats)
        {
            Id = id;
            Domain = domain;
            GenerationType = generationType;
            SpawnWeights = spawnWeights;
            Stats = stats;
        }

        public string Id { get; }

        public ModDomain Domain { get; }
        public ModGenerationType GenerationType { get; }
        public IReadOnlyList<ModifierSpawnWeight> SpawnWeights { get; }

        public IReadOnlyList<CraftableStat> Stats { get; }
    }

    public class ModifierSpawnWeight
    {
        public ModifierSpawnWeight(string tag, int weight)
        {
            Tag = tag;
            Weight = weight;
        }

        public string Tag { get; }
        public int Weight { get; }
    }
}