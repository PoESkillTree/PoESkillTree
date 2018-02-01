using System.Collections.Generic;
using MoreLinq;

namespace PoESkillTree.Computation.Core.Events
{
    public class SuspendableEventsComposite : ISuspendableEvents
    {
        private readonly ICollection<ISuspendableEvents> _children = new HashSet<ISuspendableEvents>();

        public void SuspendEvents()
        {
            _children.ForEach(c => c.SuspendEvents());
        }

        public void ResumeEvents()
        {
            _children.ForEach(c => c.ResumeEvents());
        }

        public void Add(ISuspendableEvents child)
        {
            _children.Add(child);
        }

        public void Remove(ISuspendableEvents child)
        {
            _children.Remove(child);
        }
    }
}