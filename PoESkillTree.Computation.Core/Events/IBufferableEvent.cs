using System.Collections.Generic;

namespace PoESkillTree.Computation.Core.Events
{
    /// <summary>
    /// Can use <see cref="IEventBuffer"/> to buffer event invocations.
    /// </summary>
    /// <typeparam name="T">Type of event arguments</typeparam>
    public interface IBufferableEvent<in T>
    {
        /// <summary>
        /// Invokes this event with the given list of arguments.
        /// </summary>
        void Invoke(IReadOnlyList<T> args);
    }
}