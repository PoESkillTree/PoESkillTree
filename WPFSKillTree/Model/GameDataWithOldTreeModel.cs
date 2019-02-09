using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Model
{
    public class GameDataWithOldTreeModel
    {
        public GameDataWithOldTreeModel()
            => Data = new GameData(new LazyPassiveNodeEnumerable(this), true);

        public GameData Data { get; }

        public IEnumerable<SkillNode> PassiveNodes { private get; set; }

        private class LazyPassiveNodeEnumerable : IEnumerable<PassiveNodeDefinition>
        {
            private readonly GameDataWithOldTreeModel _loader;

            public LazyPassiveNodeEnumerable(GameDataWithOldTreeModel loader)
                => _loader = loader;

            public IEnumerator<PassiveNodeDefinition> GetEnumerator()
            {
                if (_loader.PassiveNodes == null)
                    throw new InvalidOperationException("GameDataLoader.PassiveNodes was not yet set");
                return _loader.PassiveNodes.Select(ModelConverter.Convert).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}