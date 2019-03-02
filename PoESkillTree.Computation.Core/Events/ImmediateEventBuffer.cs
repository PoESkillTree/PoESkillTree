namespace PoESkillTree.Computation.Core.Events
{
    public class ImmediateEventBuffer : IEventBuffer
    {
        public void Buffer<T>(IBufferableEvent<T> sender, T args)
            => sender.Invoke(args);
    }
}