namespace PoESkillTree.Computation.Core
{
    public interface ISuspendableNotifications
    {
        void SuspendNotifications();
        void ResumeNotifications();
    }
}