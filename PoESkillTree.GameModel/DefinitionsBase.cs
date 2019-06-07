using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel
{
    public abstract class DefinitionsBase<TId, TDefinition>
        where TDefinition : IDefinition<TId>
    {
        private readonly Lazy<IReadOnlyDictionary<TId, TDefinition>> _definitionDict;

        protected DefinitionsBase(IReadOnlyList<TDefinition> definitions)
        {
            Definitions = definitions;
            _definitionDict = new Lazy<IReadOnlyDictionary<TId, TDefinition>>(
                () => Definitions.ToDictionary(d => d.Id));
        }

        protected IReadOnlyList<TDefinition> Definitions { get; }

        protected TDefinition GetDefinitionById(TId id) => _definitionDict.Value[id];
    }
}