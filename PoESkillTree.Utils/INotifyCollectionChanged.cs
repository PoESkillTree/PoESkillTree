namespace PoESkillTree.Utils
{
    public interface INotifyCollectionChanged<T>
    {
        event CollectionChangedEventHandler<T> CollectionChanged;
    }

    public delegate void CollectionChangedEventHandler<T>(object sender, CollectionChangedEventArgs<T> args);
}