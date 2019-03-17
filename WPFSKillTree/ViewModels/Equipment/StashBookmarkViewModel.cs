using System;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using PoESkillTree.Utils;
using PoESkillTree.Controls;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for StashBookmarks as shown as draggable lines in the stash itself.
    /// </summary>
    public class StashBookmarkViewModel : Notifier, IDragSource
    {
        public StashBookmark Bookmark { get; }

        private bool _isDragged;
        public bool IsDragged
        {
            get { return _isDragged; }
            private set { SetProperty(ref _isDragged, value); }
        }

        public StashBookmarkViewModel(StashBookmark bookmark)
        {
            Bookmark = bookmark;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Data = this;
            dragInfo.Effects = DragDropEffects.Move;
            IsDragged = true;
        }

        public bool CanStartDrag(IDragInfo dragInfo)
            => true;

        public void Dropped(IDropInfo dropInfo)
        {
            IsDragged = false;
        }

        public void DragCancelled()
        {
            IsDragged = false;
        }

        public bool TryCatchOccurredException(Exception exception)
            => false;
    }
}