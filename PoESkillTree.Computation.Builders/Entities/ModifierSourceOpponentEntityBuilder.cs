using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Builders.Entities
{
    public class ModifierSourceOpponentEntityBuilder : IEntityBuilder
    {
        public IReadOnlyCollection<Entity> Build(Entity modifierSourceEntity)
        {
            switch (modifierSourceEntity)
            {
                case Entity.Character:
                case Entity.Totem:
                case Entity.Minion:
                    return new[] { Entity.Enemy };
                case Entity.Enemy:
                    return new[] { Entity.Character, Entity.Totem, Entity.Minion };
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifierSourceEntity), modifierSourceEntity, null);
            }
        }
    }
}