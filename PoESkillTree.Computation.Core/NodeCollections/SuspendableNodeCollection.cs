using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class SuspendableNodeCollection<TProperty> : NodeCollection<TProperty>, ISuspendableEvents
    {
        private bool _suppressEvents;
        private NodeCollectionChangeEventArgs _suppressedArgs;

        public void SuspendEvents()
        {
            _suppressEvents = true;
        }

        public void ResumeEvents()
        {
            if (!_suppressEvents) return;

            _suppressEvents = false;
            if (_suppressedArgs != null)
            {
                OnCollectionChanged(_suppressedArgs);
                _suppressedArgs = null;
            }
        }

        protected override void OnCollectionChanged(NodeCollectionChangeEventArgs e)
        {
            if (_suppressEvents)
            {
                SuppressEvent(e);
            }
            else
            {
                base.OnCollectionChanged(e);
            }
        }

        private void SuppressEvent(NodeCollectionChangeEventArgs e)
        {
            _suppressedArgs = _suppressedArgs == null
                ? e
                : NodeCollectionChangeEventArgs.ResetEventArgs;
        }
    }
}