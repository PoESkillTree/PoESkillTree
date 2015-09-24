using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Abstract class that simplifies Properties using INotifyPropertyChanged.
    /// <code>set { SetProperty(ref _property, value); }</code> is enogh with this class.
    /// </summary>
    public abstract class Notifier : INotifyPropertyChanged, INotifyPropertyChanging
    {
        protected void SetProperty<T>(
            ref T backingStore, T value,
            Action onChanged = null,
            Action<T> onChanging = null,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            if (onChanging != null) onChanging(value);
            OnPropertyChanging(propertyName);

            backingStore = value;

            if (onChanged != null) onChanged();
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanging(string propertyName)
        {
            var handler = PropertyChanging;
            if (handler != null) handler(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}