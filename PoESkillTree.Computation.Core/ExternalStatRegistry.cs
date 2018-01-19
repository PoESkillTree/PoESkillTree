using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class ExternalStatRegistry : IExternalStatRegistry
    {
        private readonly Dictionary<IStat, double?> _registeredStats = new Dictionary<IStat, double?>();

        public void Register(IStat stat, double? defaultValue)
        {
            _registeredStats.Add(stat, defaultValue);
            RegistryChanged?.Invoke(this,
                new StatRegistryChangedEventArgs(StatRegistryChangeType.Registered, stat, defaultValue));
        }

        public void Unregister(IStat stat)
        {
            if (!_registeredStats.TryGetValue(stat, out var defaultValue))
            {
                return;
            }
            _registeredStats.Remove(stat);
            RegistryChanged?.Invoke(this,
                new StatRegistryChangedEventArgs(StatRegistryChangeType.Unregistered, stat, defaultValue));
        }

        public IReadOnlyDictionary<IStat, double?> RegisteredStats => _registeredStats;

        public event EventHandler<StatRegistryChangedEventArgs> RegistryChanged;
    }
}