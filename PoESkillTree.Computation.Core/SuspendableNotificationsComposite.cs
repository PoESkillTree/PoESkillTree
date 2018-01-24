using System.Collections.Generic;
using MoreLinq;

namespace PoESkillTree.Computation.Core
{
    public class SuspendableNotificationsComposite : ISuspendableNotifications
    {
        private readonly ICollection<ISuspendableNotifications> _children = new HashSet<ISuspendableNotifications>();

        public void SuspendNotifications()
        {
            _children.ForEach(c => c.SuspendNotifications());
        }

        public void ResumeNotifications()
        {
            _children.ForEach(c => c.ResumeNotifications());
        }

        public void Add(ISuspendableNotifications child)
        {
            _children.Add(child);
        }

        public void Remove(ISuspendableNotifications child)
        {
            _children.Remove(child);
        }
    }
}