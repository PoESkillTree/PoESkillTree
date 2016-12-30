using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Controls
{
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => SelectSelectedItem(current)));
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
            if (newValue != null)
            {
                _itemsSourceView = CollectionViewSource.GetDefaultView(newValue);
                if (_itemsSourceView.Filter != null)
                {
                    _oldFilter = _itemsSourceView.Filter;
                }
                _itemsSourceView.Filter = Filter;
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
            if (!IsDropDownOpen)
                return;

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
            if (!IsDropDownOpen)
                return;

            var args = new TextCompositionEventArgs(e.Device, e.TextComposition)
            {
                RoutedEvent = e.RoutedEvent,
                Source = _searchBox
            };
            _searchBox.RaiseEvent(args);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (HasItems && IsDropDownOpen && (e.Key == Key.Down || e.Key == Key.Up))
            {
                var focused = Keyboard.FocusedElement as UIElement;
                var index = -1;
                if (focused != null)
                {
                    index = ItemContainerGenerator.IndexFromContainer(focused);
                }
                if (index < 0 || index >= Items.Count)
                {
                    index = SelectedItem == null ? 0 : Items.IndexOf(SelectedItem);
                }

                if (e.Key == Key.Down)
                {
                    if (index < Items.Count - 1)
                        index++;
                }
                else
                {
                    if (index > 0)
                        index--;
                }

                var container = ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
                container?.Focus();
                e.Handled = true;
                return;
            }

            if (HasItems && IsDropDownOpen && e.Key == Key.Enter)
            {
                var focused = Keyboard.FocusedElement as UIElement;
                if (focused != null)
                {
                    var item = ItemContainerGenerator.ItemFromContainer(focused);
                    if (item != null && item != DependencyProperty.UnsetValue)
                    {
                        SelectedItem = item;
                    }
                }

                IsDropDownOpen = false;
                e.Handled = true;
            }

            base.OnKeyDown(e);

            if (e.Key == Key.Back)
            {
                EditingCommands.Backspace.Execute(null, _searchBox);
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            SearchFilter = string.Empty;
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