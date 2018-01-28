using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Holds the IStats that have externally (user) specified values.
    // These will be conditions in most cases.
    // Their value should be set using TotalOverride form and should not reference other nodes.
    // This interface will change when implementing support for external stats in ICalculator.Update().
    public interface IExternalStatRegistry
    {
        // Register/Unregister is called when the node for the respective IStat is created/disposed

        // Description of the stat to be shown in the UI will come from somewhere in IStat
        void Register(IStat stat, double? defaultValue);

        void Unregister(IStat stat);

        IReadOnlyDictionary<IStat, double?> RegisteredStats { get; }

        event EventHandler<StatRegistryChangedEventArgs> RegistryChanged;
    }


    public enum StatRegistryChangeType
    {
        Registered,
        Unregistered
    }


    public class StatRegistryChangedEventArgs : EventArgs
    {
        public StatRegistryChangedEventArgs(StatRegistryChangeType changeType, IStat stat, double? defaultValue)
        {
            ChangeType = changeType;
            Stat = stat;
            DefaultValue = defaultValue;
        }

        public StatRegistryChangeType ChangeType { get; }

        public IStat Stat { get; }

        public double? DefaultValue { get; }
    }
}