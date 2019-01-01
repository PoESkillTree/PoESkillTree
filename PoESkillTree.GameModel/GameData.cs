using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.GameModel
{
    public class GameData
    {
        private readonly Lazy<Task<PassiveTreeDefinition>> _passiveTreeDefinition;

        private readonly Lazy<Task<BaseItemDefinitions>> _baseItemDefinitions =
            new Lazy<Task<BaseItemDefinitions>>(BaseItemJsonDeserializer.DeserializeAsync);

        private readonly Lazy<Task<SkillDefinitions>> _skillDefinitions =
            new Lazy<Task<SkillDefinitions>>(SkillJsonDeserializer.DeserializeAsync);

        private readonly Lazy<Task<StatTranslators>> _statTranslators =
            new Lazy<Task<StatTranslators>>(StatTranslation.StatTranslators.CreateAsync);

        public GameData(IReadOnlyList<PassiveNodeDefinition> passiveNodeDefinitions)
        {
            _passiveTreeDefinition = new Lazy<Task<PassiveTreeDefinition>>(
                () => Task.FromResult(new PassiveTreeDefinition(passiveNodeDefinitions)));
        }

        public Task<PassiveTreeDefinition> PassiveTree => _passiveTreeDefinition.Value;
        public Task<BaseItemDefinitions> BaseItems => _baseItemDefinitions.Value;
        public Task<SkillDefinitions> Skills => _skillDefinitions.Value;
        public Task<StatTranslators> StatTranslators => _statTranslators.Value;
    }
}