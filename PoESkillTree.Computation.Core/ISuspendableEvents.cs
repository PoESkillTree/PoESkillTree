namespace PoESkillTree.Computation.Core
{
    public interface ISuspendableEvents
    {
        void SuspendEvents();
        void ResumeEvents();
    }
}