using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PoESkillTree.Utils;
using PoESkillTree.Common.ViewModels;

namespace PoESkillTree.ViewModels
{
    /// <summary>
    /// Contains a collection of pairs of a title and a command. Can be used e.g. to make commands selectable
    /// in a drop-down menu and execute them afterwards.
    /// </summary>
    public class CommandCollectionViewModel : Notifier
    {
        public class Item
        {
            public string Title { get; }

            public ICommand Command { get; }

            public Item(string title, ICommand command)
            {
                Title = title;
                Command = command;
            }
        }


        private Item? _selectedItem;

        public Item? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public int SelectedIndex
        {
            get => SelectedItem == null ? 0 : Items.IndexOf(SelectedItem);
            set => SelectedItem = Items.Count > value ? Items[value] : null;
        }

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public void Add(string title, Action action, Func<bool> canExecute)
        {
            Items.Add(new Item(title, new RelayCommand(action, canExecute)));
        }
    }
}