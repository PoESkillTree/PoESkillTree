using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Contains a collection of pairs of a title and a command. Can be used e.g. to make commands selectable
    /// in a drop-down menu and execute them afterwards.
    /// </summary>
    public class CommandCollectionViewModel : Notifier
    {
        public class Item : Notifier
        {
            private string _title;
            private ICommand _command;

            public string Title
            {
                get { return _title; }
                set { SetProperty(ref _title, value); }
            }

            public ICommand Command
            {
                get { return _command; }
                set { SetProperty(ref _command, value); }
            }

            public Item(string title, ICommand command)
            {
                _title = title;
                _command = command;
            }
        }


        private Item _selectedItem;

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public int SelectedIndex
        {
            get { return SelectedItem == null ? 0 : Items.IndexOf(SelectedItem); }
            set { SelectedItem = Items.Count > value ? Items[value] : null; }
        }

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public void Add(string title, ICommand command)
        {
            Items.Add(new Item(title, command));
        }

        public void Add(string title, Action action, Func<bool> canExeucte)
        {
            Items.Add(new Item(title, new RelayCommand(action, canExeucte)));
        }
    }
}