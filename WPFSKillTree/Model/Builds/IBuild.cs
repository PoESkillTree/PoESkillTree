using System.ComponentModel;

namespace POESKillTree.Model.Builds
{
    public interface IBuild : INotifyPropertyChanged, INotifyPropertyChanging, IDeepCloneable
    {
        string Name { get; }

        new IBuild DeepClone();
    }
}