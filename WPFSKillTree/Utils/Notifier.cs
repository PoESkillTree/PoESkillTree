using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Abstract class that simplifies Properties using INotifyPropertyChanged.
    /// <code>set { SetProperty(ref _property, value); }</code> is enough with this class.
    /// </summary>
    public abstract class Notifier : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Sets <paramref name="backingStore"/> to <paramref name="value"/> and
        /// raises <see cref="PropertyChanging"/> before and <see cref="PropertyChanged"/>
        /// after setting the value.
        /// </summary>
        /// <param name="backingStore">Target variable</param>
        /// <param name="value">Source variable</param>
        /// <param name="onChanged">Called after changing the value but before raising <see cref="PropertyChanged"/>.</param>
        /// <param name="onChanging">Called before changing the value and before raising <see cref="PropertyChanging"/> with <paramref name="value"/> as parameter.</param>
        /// <param name="propertyName">Name of the changed property</param>
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

        /// <summary>
        /// INotifyPropertyChanged event that is called right before a property is changed.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanging(string propertyName)
        {
            var handler = PropertyChanging;
            if (handler != null) handler(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}