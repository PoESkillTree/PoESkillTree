using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;

namespace POESKillTree.Computation.ViewModels
{
    internal class StatStub : IStat
    {
        public StatStub(string identity, Entity entity, Type dataType)
        {
            Identity = identity;
            Entity = entity;
            DataType = dataType;
        }

        public bool Equals(IStat other) => Equals((object) other);

        public string Identity { get; }
        public Entity Entity { get; }
        public IStat Minimum { get; } = null;
        public IStat Maximum { get; } = null;
        public ExplicitRegistrationType ExplicitRegistrationType { get; } = null;
        public Type DataType { get; }
        public IReadOnlyList<Behavior> Behaviors { get; } = new Behavior[0];
    }
}