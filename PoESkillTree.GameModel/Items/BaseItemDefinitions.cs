using System.Collections.Generic;

namespace PoESkillTree.GameModel.Items
{
    public class BaseItemDefinitions : DefinitionsBase<string, BaseItemDefinition>
    {
        public BaseItemDefinitions(IReadOnlyList<BaseItemDefinition> baseItems) : base(baseItems)
        {
        }

        public IReadOnlyList<BaseItemDefinition> BaseItems => Definitions;

        public BaseItemDefinition GetBaseItemById(string metadataId) => GetDefinitionById(metadataId);
    }
}