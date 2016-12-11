using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Controls
{
    // todo If focus is moved by the code here, it scrolls weirdly.
    // todo If all items are filtered out, it should still select one (some kind of default value? show all items?)
    [TemplatePart(Name = "PART_SearchBox", Type = typeof(TextBox))]
    public class SearchableComboBox : ComboBox
    {
        public static readonly DependencyProperty SearchFilterProperty = DependencyProperty.Register(
            "SearchFilter", typeof(string), typeof(SearchableComboBox),
            new PropertyMetadata(string.Empty, OnSearchFilterChanged));

        public string SearchFilter
        {
            get { return (string) GetValue(SearchFilterProperty); }
            set { SetValue(SearchFilterProperty, value); }
        }

        private static void OnSearchFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((SearchableComboBox)dependencyObject).RefreshFilter();
        }

        public static readonly DependencyProperty SearchValuePathProperty = DependencyProperty.Register(
            "SearchValuePath", typeof(string), typeof(SearchableComboBox), new PropertyMetadata(string.Empty, OnSearchValuePathChanged));

        public string SearchValuePath
        {
            get { return (string) GetValue(SearchValuePathProperty); }
            set { SetValue(SearchValuePathProperty, value); }
        }

        private static void OnSearchValuePathChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var cb = (SearchableComboBox) dependencyObject;
            cb._valuePathEvaluator = new BindingEvaluator(cb.SearchValuePath);
        }

        private BindingEvaluator _valuePathEvaluator;
        private ICollectionView _itemsSourceView;
        private Predicate<object> _oldFilter;
        private TextBox _searchBox;

        static SearchableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableComboBox),
                new FrameworkPropertyMetadata(typeof(SearchableComboBox)));
        }

        public SearchableComboBox()
        {
            _valuePathEvaluator = new BindingEvaluator(SearchValuePath);
        }

        private void RefreshFilter()
        {
            if (ItemsSource == null)
                return;
            var current = SelectedItem;
            _itemsSourceView.Refresh();
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
                SelectSelectedItem(current);
            }));
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                ItemContainerGenerator.StatusChanged -= ItemContainerGeneratorOnStatusChanged;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(FocusSelectedItem));
            }
        }

        private void SelectSelectedItem(object previousSelected)
        {
            if (previousSelected == null)
            {
                if (HasItems)
                {
                    SelectedItem = Items[0];
                }
                return;
            }

            var currentValue = ToValue(previousSelected);
            if (ToValue(SelectedItem) == currentValue)
                return;

            if (Filter(previousSelected))
            {
                foreach (var item in Items)
                {
                    if (ToValue(item) == currentValue)
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }
            else if (HasItems)
            {
                SelectedItem = Items[0];
            }
        }

        private bool Filter(object item)
        {
            if (_oldFilter != null && !_oldFilter(item))
                return false;
            if (item == null)
                return false;
            if (string.IsNullOrEmpty(SearchFilter))
                return true;
            return ToValue(item).Contains(SearchFilter, StringComparison.CurrentCultureIgnoreCase);
        }

        private string ToValue(object item)
        {
            if (item == null)
                return null;
            var comboBoxItem = item as ComboBoxItem;
            if (comboBoxItem != null)
            {
                return comboBoxItem.Content.ToString();
            }

            if (string.IsNullOrEmpty(SearchValuePath))
            {
                return item.ToString();
            }

            _valuePathEvaluator.Source = item;
            return _valuePathEvaluator.Target?.ToString();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                _itemsSourceView = CollectionViewSource.GetDefaultView(newValue);
                if (_itemsSourceView.Filter != null)
                {
                    _oldFilter = _itemsSourceView.Filter;
                }
                _itemsSourceView.Filter = Filter;
            }
            if (oldValue != null)
            {
                if (_oldFilter != null)
                {
                    _itemsSourceView.Filter = _oldFilter;
                    _oldFilter = null;
                }
                else
                {
                    _itemsSourceView.Filter = null;
                }
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _searchBox = GetTemplateChild("PART_SearchBox") as TextBox;
            if (_searchBox == null)
                throw new NullReferenceException();
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (e.OriginalSource == _searchBox)
                return;
            base.OnTextInput(e);
            var args = new TextCompositionEventArgs(e.Device, e.TextComposition)
            {
                RoutedEvent = e.RoutedEvent,
                Source = _searchBox
            };
            _searchBox.RaiseEvent(args);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.OriginalSource == _searchBox)
                return;
            base.OnPreviewTextInput(e);
            var args = new TextCompositionEventArgs(e.Device, e.TextComposition)
            {
                RoutedEvent = e.RoutedEvent,
                Source = _searchBox
            };
            _searchBox.RaiseEvent(args);
        }

        private void FocusSelectedItem()
        {
            if (SelectedItem != null)
            {
                var container = ItemContainerGenerator.ContainerFromItem(SelectedItem) as UIElement;
                container?.Focus();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (HasItems && IsDropDownOpen && (e.Key == Key.Down || e.Key == Key.Up))
            {
                var focusedBefore = Keyboard.FocusedElement as UIElement;
                base.OnKeyDown(e);

                if (focusedBefore == Keyboard.FocusedElement as UIElement)
                {
                    if (focusedBefore == null)
                    {
                        FocusSelectedItem();
                        return;
                    }

                    var index = ItemContainerGenerator.IndexFromContainer(focusedBefore);
                    if (index < 0 || index >= Items.Count)
                    {
                        FocusSelectedItem();
                        return;
                    }

                    if (e.Key == Key.Down)
                    {
                        index++;
                    }
                    else
                    {
                        index--;
                    }
                    if (index >= 0 && index < Items.Count)
                    {
                        var container = ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
                        container?.Focus();
                    }
                }
                return;
            }

            base.OnKeyDown(e);

            if (e.Key == Key.Back)
            {
                EditingCommands.Backspace.Execute(null, _searchBox);
            }
        }


        private class BindingEvaluator : DependencyObject
        {
            public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
                "Source", typeof(object), typeof(BindingEvaluator), new PropertyMetadata(default(object)));

            public object Source
            {
                get { return GetValue(SourceProperty); }
                set { SetValue(SourceProperty, value); }
            }

            public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
                "Target", typeof(object), typeof(BindingEvaluator), new PropertyMetadata(default(object)));

            public object Target
            {
                get { return GetValue(TargetProperty); }
                set { SetValue(TargetProperty, value); }
            }

            public BindingEvaluator(string propertyPath)
            {
                var binding = new Binding(nameof(Source) + "." + propertyPath)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                };
                BindingOperations.SetBinding(this, TargetProperty, binding);
            }
        }
    }
}