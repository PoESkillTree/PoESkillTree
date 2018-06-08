using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class Stat : IStat
    {
        public Stat(
            string identity, Entity entity, bool isRegisteredExplicitly = false, Type dataType = null,
            IReadOnlyCollection<Behavior> behaviors = null, bool hasRange = true)
        {
            _identity = identity;
            _hasRange = hasRange;
            Entity = entity;
            IsRegisteredExplicitly = isRegisteredExplicitly;
            DataType = dataType;
            Behaviors = behaviors ?? new Behavior[0];
        }
        
        private readonly bool _hasRange;
        private readonly string _identity;
        public Entity Entity { get; }
        public bool IsRegisteredExplicitly { get; }
        public Type DataType { get; }
        public IReadOnlyCollection<Behavior> Behaviors { get; }

        public IStat Minimum => MinOrMax(_identity + ".Minimum");
        public IStat Maximum => MinOrMax(_identity + ".Maximum");

        private Stat MinOrMax(string identity) =>
            _hasRange ? new Stat(identity, Entity, IsRegisteredExplicitly, DataType, Behaviors, false) : null;

        public override string ToString() => _identity;

        public override bool Equals(object obj) =>
            (obj == this) || (obj is IStat other && Equals(other));

        public bool Equals(IStat other) =>
            (other != null) && ToString().Equals(other.ToString()) && Entity == other.Entity;

        public override int GetHashCode() =>
            (_identity, Entity).GetHashCode();
    }
}