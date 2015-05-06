using Microsoft.Win32;
using POESKillTree.ViewModels.Items;
using Raven.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for Stash.xaml
    /// </summary>
    public partial class Stash : UserControl
    {

        public ObservableCollection<Item> Items
        {
            get { return (ObservableCollection<Item>)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<Item>), typeof(Stash), new PropertyMetadata(null, ItemsPropertyChanged));

        private static void ItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stash = d as Stash;

            if (e.OldValue != null)
            {
                var list = (ObservableCollection<Item>)e.OldValue;
                list.CollectionChanged -= stash.items_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                var list = (ObservableCollection<Item>)e.NewValue;
                list.CollectionChanged += stash.items_CollectionChanged;
            }

            stash.ReloadItem();
        }


        Dictionary<Item, ItemVisualizer> _usedVisualizers = new Dictionary<Item, ItemVisualizer>();

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Item it in e.NewItems)
                {
                    var iv = new ItemVisualizer() 
                    { 
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                    };

                    iv.Item = it;

                    
                    _usedVisualizers.Add(it,iv);

                    Thickness m = new Thickness(it.X*GridSize,it.Y*GridSize,0,0);
                    iv.Margin = m;
                    iv.Width = it.W * GridSize;
                    iv.Height = it.H * GridSize;
                    gcontent.Children.Add(iv);
                }

            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Item it in e.OldItems)
                {
                    ItemVisualizer iv;
                    if (_usedVisualizers.TryGetValue(it, out iv))
                    {
                        iv.Item = null;
                        _usedVisualizers.Remove(it);
                    }
                }
            }
            else
                throw new NotImplementedException();
        }

        private void ReloadItem()
        {
            //throw new NotImplementedException();
        }




        public double GridSize
        {
            get { return (double)GetValue(GridSizeProperty); }
            set { SetValue(GridSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register("GridSize", typeof(double), typeof(Stash), new PropertyMetadata(47.0));




        public Stash()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog of = new OpenFileDialog();
                if (of.ShowDialog() == true)
                {
                    var data = File.ReadAllText(of.FileName);

                    var json = RavenJObject.Parse(data);
                    var items = (json["items"] as RavenJArray).Select(i => new Item((RavenJObject)i));

                    //get free line
                    var y = (Items.Count >0)?Items.Max(i => i.Y + i.H) + 3:0;
                    int x = 0;
                    int maxh = 0;
                    foreach (var item in items)
                    {
                        if (x + item.W > 12) //next line
                        {
                            x = 0;
                            y += maxh;
                            maxh = 0;
                        }

                        item.X = x;
                        x += item.W;

                        if (maxh < item.H)
                            maxh = item.H;

                        item.Y = y;
                        Items.Add(item);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
