using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Collections;

namespace WpfRangeControls
{

    [TemplatePart(Name = "PART_RangeOverlay", Type = typeof(RangeItemsControl))]
    [ContentProperty("Items")]
    public class RangeScrollbar : ScrollBar, INotifyPropertyChanged
    {

        static RangeScrollbar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeScrollbar), new FrameworkPropertyMetadata(typeof(RangeScrollbar)));
        }

        public RangeScrollbar()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _RangeControl = base.GetTemplateChild("PART_RangeOverlay") as RangeItemsControl;


            if (_RangeControl == null)
                return;

            if (_iItems != null && _iItems.Count > 0)
            {
                foreach (var item in _iItems)
                    _RangeControl.Items.Add(item);

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Items"));

                _iItems = null;

            }

            if (_iItemsSource != null)
            {
                _RangeControl.ItemsSource = _iItemsSource;

                _iItemsSource = null;


                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ItemsSource"));
            }

            if (_iItemTemplate != null)
            {
                _RangeControl.ItemTemplate = _iItemTemplate;
                _iItemTemplate = null;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ItemTemplate"));

            }

        }


        RangeItemsControl _RangeControl;
        [Bindable(false), Category("Content")]
        public RangeItemsControl RangeControl
        {
            get { return _RangeControl; }
        }


        ObservableCollection<UIElement> _iItems = new ObservableCollection<UIElement>();

        public event PropertyChangedEventHandler PropertyChanged;

        [Bindable(true), Category("Content")]
        public IList Items
        {
            get
            {
                if (_RangeControl == null)
                    ApplyTemplate();
                if (_RangeControl != null)
                    return _RangeControl.Items;

                return _iItems;
            }
        }

        IEnumerable _iItemsSource = null;
        [Bindable(true), Category("Content")]
        public IEnumerable ItemsSource
        {
            get
            {
                if (_RangeControl == null)
                    return _iItemsSource;

                return _RangeControl.ItemsSource;
            }

            set
            {
                if (_RangeControl == null)
                    _iItemsSource = value;
                else
                    _RangeControl.ItemsSource = value;
            }
        }


        DataTemplate _iItemTemplate = null;
        [Bindable(true), Category("Content")]
        public DataTemplate ItemTemplate
        {
            get
            {
                if (_RangeControl == null)
                    return _iItemTemplate;
                return _RangeControl.ItemTemplate;
            }
            set
            {
                if (_RangeControl == null)
                    _iItemTemplate = value;
                else
                    _RangeControl.ItemTemplate = value;
            }
        }

        public static readonly DependencyProperty AlternationCountProperty = ItemsControl.AlternationCountProperty.AddOwner(typeof(RangeScrollbar));
        [Bindable(true), Category("Content")]
        public int AlternationCount
        {
            get { return (int)GetValue(AlternationCountProperty); }
            set { SetValue(AlternationCountProperty, value); }
        }


    }
}
