using System.ComponentModel;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class SuspendableObservableCollection<T> : ObservableCollection<T>, ISuspendableEvents
    {
        private bool _suppressEvents;
        private CollectionChangeEventArgs _suppressedArgs;

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

        protected override void OnCollectionChanged(CollectionChangeEventArgs e)
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

        private void SuppressEvent(CollectionChangeEventArgs e)
        {
            _suppressedArgs = _suppressedArgs == null
                ? e
                : new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);
        }
    }
}