namespace PoESkillTree.Computation.Core.Events
{
    public interface ISuspendableEvents
    {
        void SuspendEvents();
        void ResumeEvents();
    }
}