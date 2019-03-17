namespace PoESkillTree.Model
{
    /// <summary>
    /// Represents a stored state for objects of class <typeparamref name="T"/> that can be restored on any
    /// instance of that type.
    /// </summary>
    public interface IMemento<in T>
    {
        /// <summary>
        /// Restores the stored state onto the given target.
        /// </summary>
        void Restore(T target);
    }
}