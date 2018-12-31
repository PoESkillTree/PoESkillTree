using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.Items
{
    public class BaseItemDefinitions
    {
        private readonly Lazy<IReadOnlyDictionary<string, BaseItemDefinition>> _baseItemDict;

        public BaseItemDefinitions(IReadOnlyList<BaseItemDefinition> baseItems)
        {
            BaseItems = baseItems;
            _baseItemDict = new Lazy<IReadOnlyDictionary<string, BaseItemDefinition>>(
                () => BaseItems.ToDictionary(s => s.MetadataId));
        }

        public IReadOnlyList<BaseItemDefinition> BaseItems { get; }

        public BaseItemDefinition GetBaseItemById(string metadataId) => _baseItemDict.Value[metadataId];
    }
}