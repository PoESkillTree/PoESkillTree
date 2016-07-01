namespace POESKillTree.Model
{
    public interface IMemento<in T>
    {
        IMemento<T> Restore(T target);
    }
}