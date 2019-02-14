namespace PoESkillTree.Computation.Core.Events
{
    public interface IEventBuffer
    {
        /// <summary>
        /// Buffers the invocation of sender to be executed potentially at a later time.
        /// </summary>
        void Buffer<T>(IBufferableEvent<T> sender, T args);
    }
}