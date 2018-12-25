using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.PassiveTree
{
    public class PassiveTreeDefinition
    {
        private readonly Lazy<IReadOnlyDictionary<ushort, PassiveNodeDefinition>> _nodeDict;

        public PassiveTreeDefinition(IReadOnlyList<PassiveNodeDefinition> nodes)
        {
            Nodes = nodes;
            _nodeDict = new Lazy<IReadOnlyDictionary<ushort, PassiveNodeDefinition>>(
                () => Nodes.ToDictionary(s => s.Id));
        }

        public IReadOnlyList<PassiveNodeDefinition> Nodes { get; }

        public PassiveNodeDefinition GetNodeById(ushort id) => _nodeDict.Value[id];
    }
}