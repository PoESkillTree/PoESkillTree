using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.Controls
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

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        private void Add(Item item)
        {
            Items.Add(item);
            if (SelectedItem == null)
            {
                SelectedItem = item;
            }
        }

        public void Add(string title, ICommand command)
        {
            Add(new Item(title, command));
        }

        public void Add(string title, Action action, Func<bool> canExeucte)
        {
            Add(new Item(title, new RelayCommand(action, canExeucte)));
        }
    }
}