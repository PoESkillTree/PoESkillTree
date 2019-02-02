using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace POESKillTree.Computation.Model
{
    public class ConfigurationStats
    {
        private readonly Dictionary<string, NodeValue?> _values =
            new Dictionary<string, NodeValue?>();

        public static ConfigurationStats Create(IEnumerable<(string, double?)> values)
        {
            var o = new ConfigurationStats();
            foreach (var (stat, value) in values)
            {
                o._values[stat] = (NodeValue?) value;
            }
            return o;
        }

        public bool TryGetValue(IStat stat, out NodeValue? value)
            => _values.TryGetValue(stat.ToString(), out value);

        public void SetValue(IStat stat, NodeValue? value)
        {
            _values[stat.ToString()] = value;
            OnValueChanged();
        }

        public IEnumerable<(string, double?)> Export()
            => _values.Select(p => (p.Key, p.Value.SingleOrNull()));

        private void OnValueChanged()
            => ValueChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler ValueChanged;
    }
}