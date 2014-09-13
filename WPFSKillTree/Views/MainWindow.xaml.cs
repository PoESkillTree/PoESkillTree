using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using POESKillTree.Controls;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.ViewModels;
using Application = System.Windows.Application;
using Attribute = POESKillTree.ViewModels.Attribute;
using Clipboard = System.Windows.Clipboard;
using Control = System.Windows.Controls.Control;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ToolTip = System.Windows.Controls.ToolTip;

namespace POESKillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly PersistentData _persistentData = new PersistentData {Options = new Options()};
        private static readonly Action EmptyDelegate = delegate { };
        private readonly List<Attribute> _allAttributesList = new List<Attribute>();
        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly List<Attribute> _statisticsList = new List<Attribute>();
        private readonly List<ListGroupItem> _offenceList = new List<ListGroupItem>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
        private readonly ToolTip _noteTip = new ToolTip();
        private ListCollectionView _allAttributeCollection;
        private ListCollectionView _attibuteCollection;
        private ListCollectionView _statisticsCollection;
        private ListCollectionView _offenceCollection;
        private RenderTargetBitmap _clipboardBmp;

        private ItemAttributes _itemAttributes;
        protected SkillTree Tree;
        private const string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private Vector2D _addtransform;
        private bool _justLoaded;
        private string _lasttooltip;

        private LoadingWindow _loadingWindow;
        private Vector2D _multransform;

        private List<ushort> _prePath;
        private HashSet<ushort> _toRemove;

        readonly Stack<string> _undoList = new Stack<string>();
        readonly Stack<string> _redoList = new Stack<string>();

        private Point _dragAndDropStartPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;

        public MainWindow()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //AppDomain.CurrentDomain.AssemblyResolve += ( sender, args ) =>
            //{

            //    String resourceName = "POESKillTree." +

            //       new AssemblyName( args.Name ).Name + ".dll";

            //    using ( var stream = Assembly.GetExecutingAssembly( ).GetManifestResourceStream( resourceName ) )
            //    {

            //        Byte[] assemblyData = new Byte[ stream.Length ];

            //        stream.Read( assemblyData, 0, assemblyData.Length );

            //        return Assembly.Load( assemblyData );

            //    }

            //};

            InitializeComponent();
        }

        private string InsertNumbersInAttributes(KeyValuePair<string, List<float>> attrib)
        {
            string s = attrib.Key;
            foreach (float f in attrib.Value)
            {
                s = _backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        public static string MakePoeUrl(string url)
        {
            try
            {
                if (url.Length <= 12)
                {
                    return url;
                }
                if (!url.ToLower().StartsWith("http") && !url.ToLower().StartsWith("ftp"))
                {
                    url = "http://" + url;
                }
                var request = WebRequest.Create("http://poeurl.com/shrink.php?url=" + url);
                var res = request.GetResponse();
                string text;
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch (Exception)
            {
                return url;
            }
        }

        protected string ToPoeURLS(string txt)
        {
            var regx =
                new Regex(
                    "http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?",
                    RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(txt);

            foreach (Match match in mactches)
            {
                string pURL = MakePoeUrl(match.Value);
                txt = txt.Replace(match.Value, pURL);
            }

            return txt;
        }

        public void UpdateAllAttributeList()
        {
            if (_itemAttributes != null)
            {
                Dictionary<string, List<float>> attritemp = Tree.SelectedAttributesWithoutImplicit;
                foreach (ItemAttributes.Attribute mod in _itemAttributes.NonLocalMods)
                {
                    if (attritemp.ContainsKey(mod.TextAttribute))
                    {
                        for (int i = 0; i < mod.Value.Count; i++)
                        {
                            attritemp[mod.TextAttribute][i] += mod.Value[i];
                        }
                    }
                    else
                    {
                        attritemp[mod.TextAttribute] = mod.Value;
                    }
                }

                foreach (var a in Tree.ImplicitAttributes(attritemp))
                {
                    if (!attritemp.ContainsKey(a.Key))
                        attritemp[a.Key] = new List<float>();
                    for (int i = 0; i < a.Value.Count; i++)
                    {
                        if (attritemp.ContainsKey(a.Key) && attritemp[a.Key].Count > i)
                            attritemp[a.Key][i] += a.Value[i];
                        else
                        {
                            attritemp[a.Key].Add(a.Value[i]);
                        }
                    }
                }

                _allAttributesList.Clear();
                foreach (string item in (attritemp.Select(InsertNumbersInAttributes)))
                {
                    _allAttributesList.Add(new Attribute(item));
                }
                _allAttributeCollection.Refresh();

                UpdateStatistics(attritemp);
            }
            tbSkillURL.Text = Tree.SaveToURL();
            UpdateAttributeList();
        }

        public void UpdateAttributeList()
        {
            _attiblist.Clear();
            foreach (var item in (Tree.SelectedAttributes.Select(InsertNumbersInAttributes)))
            {
                _attiblist.Add(new Attribute(item));
            }
            _attibuteCollection.Refresh();
            tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
        }

        public void UpdateStatistics(Dictionary<string, List<float>> attrs)
        {
            Compute.Initialize(Tree, _itemAttributes);

            _statisticsList.Clear();
            foreach (var item in Tree.ComputedStatistics(attrs, _itemAttributes).Select(InsertNumbersInAttributes))
            {
                _statisticsList.Add(new Attribute(item));
            }

            _statisticsCollection.Refresh();

            _offenceList.Clear();
            foreach (ListGroup group in Compute.Offense())
            {
                foreach (var item in group.Properties.Select(InsertNumbersInAttributes))
                {
                    _offenceList.Add(new ListGroupItem(item, group.Name));
                }
            }
            _offenceCollection.Refresh();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _persistentData.Options.SkillTreeAddress = tbSkillURL.Text;
            _persistentData.Options.CharacterLevel = tbLevel.Text;
            _persistentData.Options.AttributesBarOpened = expAttributes.IsExpanded;
            _persistentData.Options.BuildsBarOpened = expSavedBuilds.IsExpanded;
            _persistentData.SavePersistentDataToFile();

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _attibuteCollection = new ListCollectionView(_attiblist);
            listBox1.ItemsSource = _attibuteCollection;
            // AttibuteCollection.CustomSort = 
            var pgd = new PropertyGroupDescription("") {Converter = new GroupStringConverter()};
            _attibuteCollection.GroupDescriptions.Add(pgd);

            _allAttributeCollection = new ListCollectionView(_allAttributesList);
            _allAttributeCollection.GroupDescriptions.Add(pgd);
            lbAllAttr.ItemsSource = _allAttributeCollection;

            _statisticsCollection = new ListCollectionView(_statisticsList);
            _statisticsCollection.GroupDescriptions.Add(new PropertyGroupDescription("") { Converter = new StatisticsGroupStringConverter() });
            listBoxStatistics.ItemsSource = _statisticsCollection;

            _offenceCollection = new ListCollectionView(_offenceList);
            _offenceCollection.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            listBoxOffence.ItemsSource = _offenceCollection;

            Tree = SkillTree.CreateSkillTree(StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);


            Tree.Chartype =
                Tree.CharName.IndexOf(((string) ((ComboBoxItem) cbCharType.SelectedItem).Content).ToUpper());
            Tree.UpdateAvailNodes();
            UpdateAllAttributeList();

            _multransform = Tree.TRect.Size / recSkillTree.RenderSize.Height;
            _addtransform = Tree.TRect.TopLeft;

            // loading last build
            _persistentData.LoadPersistentDataFromFile();
            SetTheme(_persistentData.Options.OptionsTheme);
            tbLevel.Text = _persistentData.Options.CharacterLevel;
            tbSkillURL.Text = _persistentData.Options.SkillTreeAddress;
            expAttributes.IsExpanded = _persistentData.Options.AttributesBarOpened;
            expSavedBuilds.IsExpanded = _persistentData.Options.BuildsBarOpened;

            btnLoadBuild_Click(this, new RoutedEventArgs());
            _justLoaded = false;

            // loading saved build
            lvSavedBuilds.Items.Clear();
            foreach (var lvi in _persistentData.BuildsAsListViewItems)
            {
                AddItemToSavedBuilds(lvi);
            }

            ImportLegacySavedBuilds();
        }

        void lvi_MouseLeave(object sender, MouseEventArgs e)
        {
                _noteTip.IsOpen = false;
        }

        private void lvi_MouseEnter(object sender, MouseEventArgs e)
        {
            var highlightedItem = FindListViewItem(e);
            if (highlightedItem != null)
            {
                var build = (PoEBuild) highlightedItem.Content;
                _noteTip.Content = build.Note;
                _noteTip.IsOpen = true;
            }
        }

        void lvi_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (ListViewItem)lvSavedBuilds.SelectedItem;
            var newBuild = ((PoEBuild)selectedItem.Content).Clone();
            var formBuildName = new FormBuildName(newBuild.Name, newBuild.Note);
            var show_dialog = formBuildName.ShowDialog();
            if (show_dialog != null && (bool)show_dialog)
            {
                newBuild.Name = formBuildName.GetBuildName();
                newBuild.Note = formBuildName.GetNote();
                selectedItem.Content = newBuild;
            }
            SaveBuildsToFile();
        }

        private ListViewItem FindListViewItem(MouseEventArgs e)
        {
            var visualHitTest = VisualTreeHelper.HitTest(lvSavedBuilds, e.GetPosition(lvSavedBuilds)).VisualHit;

            ListViewItem listViewItem = null;

            while (visualHitTest != null)
            {
                if (visualHitTest is ListViewItem)
                {
                    listViewItem = visualHitTest as ListViewItem;

                    break;
                }
                if (Equals(visualHitTest, lvSavedBuilds))
                {
                    return null;
                }

                visualHitTest = VisualTreeHelper.GetParent(visualHitTest);
            }

            return listViewItem;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ToggleAttributes();
                        break;
                    case Key.B:
                        ToggleBuilds();
                        break;
                    case Key.R:
                        RecSkillTree_Reset_Click(sender, e);
                        break;
                    case Key.E:
                        btnPoeUrl_Click(sender, e);
                        break;
                    case Key.D1:
                        cbCharType.SelectedIndex = 0;
                        break;
                    case Key.D2:
                        cbCharType.SelectedIndex = 1;
                        break;
                    case Key.D3:
                        cbCharType.SelectedIndex = 2;
                        break;
                    case Key.D4:
                        cbCharType.SelectedIndex = 3;
                        break;
                    case Key.D5:
                        cbCharType.SelectedIndex = 4;
                        break;
                    case Key.D6:
                        cbCharType.SelectedIndex = 5;
                        break;
                    case Key.D7:
                        cbCharType.SelectedIndex = 6;
                        break;
                    case Key.Z:
                        tbSkillURL_Undo();
                        break;
                    case Key.Y:
                        tbSkillURL_Redo();
                        break;
                    case Key.S:
                        SaveNewBuild();
                        break;
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void menu_exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void border1_Click(object sender, RoutedEventArgs e)
        {
            Point p = ((MouseEventArgs) e.OriginalSource).GetPosition(border1.Child);
            var v = new Vector2D(p.X, p.Y);

            v = v * _multransform + _addtransform;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
            {
                var node = nodes.First().Value;

                if (node.spc == null)
                {
                    if (Tree.SkilledNodes.Contains(node.id))
                    {
                        Tree.ForceRefundNode(node.id);
                        UpdateAllAttributeList();

                        _prePath = Tree.GetShortestPathTo(node.id);
                        Tree.DrawPath(_prePath);
                    }
                    else if (_prePath != null)
                    {
                        foreach (ushort i in _prePath)
                        {
                            Tree.SkilledNodes.Add(i);
                        }
                        UpdateAllAttributeList();
                        Tree.UpdateAvailNodes();

                        _toRemove = Tree.ForceRefundNodePreview(node.id);
                        if (_toRemove != null)
                            Tree.DrawRefundPreview(_toRemove);
                    }
                }
            }
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void border1_MouseLeave(object sender, MouseEventArgs e)
        {
            // We might have popped up a tooltip while the window didn't have focus,
            // so we should close tooltips whenever the mouse leaves the canvas in addition to
            // whenever we lose focus.
            _sToolTip.IsOpen = false;
        }

        private void border1_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(border1.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;
            textBox1.Text = "" + v.X;
            textBox2.Text = "" + v.Y;
            SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
                node = nodes.First().Value;

            if (node != null && node.Attributes.Count != 0)
            {
                var tooltip = node.name + "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(_sToolTip.IsOpen && _lasttooltip == tooltip))
                {
                    _sToolTip.Content = tooltip;
                    _sToolTip.IsOpen = true;
                    _lasttooltip = tooltip;
                }
                if (Tree.SkilledNodes.Contains(node.id))
                {
                    _toRemove = Tree.ForceRefundNodePreview(node.id);
                    if (_toRemove != null)
                        Tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = Tree.GetShortestPathTo(node.id);
                    Tree.DrawPath(_prePath);
                }
            }
            else
            {
                _sToolTip.Tag = false;
                _sToolTip.IsOpen = false;
                _prePath = null;
                _toRemove = null;
                if (Tree != null)
                {
                    Tree.ClearPath();
                }
            }
        }

        private void btnCopyStats_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var at in _attiblist)
            {
                sb.AppendLine(at.ToString());
            }
            System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                lvSavedBuilds.Items.Remove(lvSavedBuilds.SelectedItem);
                SaveBuildsToFile();
            }
        }

        private void btnDownloadItemData_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
            Process.Start("http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text);
        }

        private void btnDownloadItemData_Copy_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
            var fileDialog = new OpenFileDialog {Multiselect = false};
            var ftoload = fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
                _itemAttributes = new ItemAttributes(fileDialog.FileName);
                lbItemAttr.ItemsSource = _itemAttributes.Attributes;
                UpdateAllAttributeList();
            }
        }

        private void btnOverwriteBuild_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                var selectedItem = (ListViewItem) lvSavedBuilds.SelectedItem;
                var selectedBuild = (PoEBuild) selectedItem.Content;
                //SpaceOgre: I don't like this but it works. 
                //Should probably use INotifyPropertyChanged and ObservableCollection instead.
                var newBuild = new PoEBuild
                {
                    Name = selectedBuild.Name,
                    Class = cbCharType.Text,
                    PointsUsed = tbUsedPoints.Text,
                    Url = tbSkillURL.Text,
                    Note = selectedBuild.Note
                };
                selectedItem.Content = newBuild;
                SaveBuildsToFile();
            }
            else
            {
                MessageBox.Show("Please select an existing build first.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnPoeUrl_Click(object sender, RoutedEventArgs e)
        {
            string poeurl = "http://poeurl.com/" + ToPoeURLS(tbSkillURL.Text);
            System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
            MessageBox.Show("The URL below has been copied to you clipboard: \n" + poeurl, "poeurl Link",
                MessageBoxButton.OK);
        }

        private void btnPopup_OnClick(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
        }

        private void btnSaveNewBuild_Click(object sender, RoutedEventArgs e)
        {
            SaveNewBuild();
        }

        private void SaveNewBuild()
        {
            var formBuildName = new FormBuildName();
            var show_dialog = formBuildName.ShowDialog();
            if (show_dialog != null && (bool) show_dialog)
            {
                var lvi = new ListViewItem
                {
                    Content =
                        new PoEBuild{
                            Name = formBuildName.GetBuildName(),
                            Class = cbCharType.Text,
                            PointsUsed = tbUsedPoints.Text, 
                            Url = tbSkillURL.Text, 
                            Note = formBuildName.GetNote()
                        }
                };
                AddItemToSavedBuilds(lvi);
            }

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
        }

        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            const int maxsize = 3000;
            Rect2D contentBounds = Tree.picActiveLinks.ContentBounds;
            contentBounds *= 1.2;
            if (!double.IsNaN(contentBounds.Width) && !double.IsNaN(contentBounds.Height))
            {
            double aspect = contentBounds.Width / contentBounds.Height;
            double xmax = contentBounds.Width;
            double ymax = contentBounds.Height;
            if (aspect > 1 && xmax > maxsize)
            {
                xmax = maxsize;
                ymax = xmax / aspect;
            }
            if (aspect < 1 & ymax > maxsize)
            {
                ymax = maxsize;
                xmax = ymax * aspect;
            }

                _clipboardBmp = new RenderTargetBitmap((int)xmax, (int)ymax, 96, 96, PixelFormats.Pbgra32);
            var db = new VisualBrush(Tree.SkillTreeVisual);
            db.ViewboxUnits = BrushMappingMode.Absolute;
            db.Viewbox = contentBounds;
            var dw = new DrawingVisual();

            using (DrawingContext dc = dw.RenderOpen())
            {
                dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
            }
            _clipboardBmp.Render(dw);
            _clipboardBmp.Freeze();

            Clipboard.SetImage(_clipboardBmp);

            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
        }
        }

        private void ImportItems_Click(object sender, RoutedEventArgs e)
        {
            string filetoload;
            if (File.Exists("Data\\get-items"))
            {
                filetoload = "Data\\get-items";
            }
            else if (File.Exists("Data\\get-items.txt"))
            {
                filetoload = "Data\\get-items.txt";
            }
            else
            {
                popup1.IsOpen = true;
                return;
            }

            _itemAttributes = new ItemAttributes(filetoload);
            lbItemAttr.ItemsSource = _itemAttributes.Attributes;
            UpdateAllAttributeList();
        }

        private void btnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            LoadBuild();
        }

        private void tbSkillURL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoadBuild();
        }

        public void LoadBuild()
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(Tree, tbSkillURL.Text);
                    tbSkillURL.Text = Tree.SaveToURL();
                }
                else if (tbSkillURL.Text.Contains("poebuilder.com/"))
                {
                    const string poebuilderTree = "https://poebuilder.com/character/";
                    const string poebuilderTreeWWW = "https://www.poebuilder.com/character/";
                    const string poebuilderTreeOWWW = "http://www.poebuilder.com/character/";
                    const string poebuilderTreeO = "http://poebuilder.com/character/";
                    var urlString = tbSkillURL.Text;
                    urlString = urlString.Replace(poebuilderTree, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeO, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeWWW, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeOWWW, MainWindow.TreeAddress);
                    tbSkillURL.Text = urlString;
                    Tree.LoadFromURL(urlString);
                }
                else if (tbSkillURL.Text.Contains("tinyurl.com/"))
                {
                    var request = (HttpWebRequest)WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse)request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    LoadBuild();
                }
                else if (tbSkillURL.Text.Contains("poeurl.com/"))
                {
                    tbSkillURL.Text = tbSkillURL.Text.Replace("http://poeurl.com/",
                        "http://poeurl.com/redirect.php?url=");
                    var request = (HttpWebRequest)WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse)request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    LoadBuild();
                }
                else
                    Tree.LoadFromURL(tbSkillURL.Text);

                _justLoaded = true;
                //cleans the default tree on load if 2
                if (_justLoaded)
                {
                    if (_undoList.Count > 1)
                    {
                        string holder = _undoList.Pop();
                        _undoList.Pop();
                        _undoList.Push(holder);
                    }
                }
                cbCharType.SelectedIndex = Tree.Chartype;
                UpdateAllAttributeList();
            }
            catch (Exception)
            {
                MessageBox.Show("The Build you tried to load, is invalid");
            }
        }

        private void SkillHighlightedNodes_Click(object sender, RoutedEventArgs e)
        {
            Tree.SkillAllHighligtedNodes();
            UpdateAllAttributeList();
        }

        private void RecSkillTree_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (Tree == null)
                return;
            Tree.Reset();
            UpdateAllAttributeList();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_justLoaded)
            {
                _justLoaded = false;
                return;
            }

            if (Tree == null)
                return;
            SkillNode startnode =
                Tree.Skillnodes.First(
                    nd => nd.Value.name.ToUpper() == (Tree.CharName[cbCharType.SelectedIndex]).ToUpper()).Value;
            Tree.SkilledNodes.Clear();
            Tree.SkilledNodes.Add(startnode.id);
            Tree.Chartype = Tree.CharName.IndexOf((Tree.CharName[cbCharType.SelectedIndex]).ToUpper());
            Tree.UpdateAvailNodes();
            UpdateAllAttributeList();
        }

        private void ToggleAttributes()
        {
            mnuViewAttributes.IsChecked = !mnuViewAttributes.IsChecked;
            expAttributes.IsExpanded = !expAttributes.IsExpanded;
        }

        private void ToggleAttributes_Click(object sender, RoutedEventArgs e)
        {
            ToggleAttributes();
        }

        private void expAttributes_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group collapsion events.
            {
                mnuViewAttributes.IsChecked = false;
            }
        }

        private void expAttributes_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group expansion events.
            {
                mnuViewAttributes.IsChecked = true;

                if (expSheet.IsExpanded) expSheet.IsExpanded = false;
            }
        }

        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedAttr = Regex.Replace(Regex.Match(listBox1.SelectedItem.ToString(), @"(?!\d)\w.*\w").Value.Replace(@"+", @"\+").Replace(@"-", @"\-").Replace(@"%", @"\%"), @"\d+", @"\d+");
            Tree.HighlightNodes(selectedAttr, true, Brushes.Azure);
        }

        private void expAttributes_MouseLeave(object sender, MouseEventArgs e)
        {
            SearchUpdate();
        }

        private void expSheet_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group expansion events.
            {
                if (expAttributes.IsExpanded) ToggleAttributes();
            }
        }

        private void ToggleBuilds()
        {
            mnuViewBuilds.IsChecked = !mnuViewBuilds.IsChecked;
            expSavedBuilds.IsExpanded = !expSavedBuilds.IsExpanded;
        }

        private void ToggleBuilds_Click(object sender, RoutedEventArgs e)
        {
            ToggleBuilds();
        }

        private void expSavedBuilds_Collapsed(object sender, RoutedEventArgs e)
        {
            mnuViewBuilds.IsChecked = false;
        }

        private void expSavedBuilds_Expanded(object sender, RoutedEventArgs e)
        {
            mnuViewBuilds.IsChecked = true;
        }

        private void image1_LostFocus(object sender, MouseEventArgs e)
        {
            _sToolTip.IsOpen = false;
        }


        private void lvSavedBuilds_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                lvSavedBuilds.SelectedIndex > 0)
            {
                MoveBuildInList(-1);
            }

            else if (e.Key == Key.Down && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                     lvSavedBuilds.SelectedIndex < lvSavedBuilds.Items.Count - 1)
            {
                MoveBuildInList(1);
            }
        }

        private void MoveBuildInList(int direction)
        {
            var obj = lvSavedBuilds.Items[lvSavedBuilds.SelectedIndex];
            var selectedIndex = lvSavedBuilds.SelectedIndex;
            lvSavedBuilds.Items.RemoveAt(selectedIndex);
            lvSavedBuilds.Items.Insert(selectedIndex + direction, obj);
            lvSavedBuilds.SelectedItem = lvSavedBuilds.Items[selectedIndex + direction];
            lvSavedBuilds.SelectedIndex = selectedIndex + direction;
            lvSavedBuilds.Items.Refresh();

            SaveBuildsToFile();
        }

        private void AddItemToSavedBuilds(Control lvi)
        {
            lvi.MouseDoubleClick += lvi_MouseDoubleClick;
            lvi.MouseRightButtonUp += lvi_MouseRightButtonUp;
            lvi.MouseEnter += lvi_MouseEnter;
            lvi.MouseLeave += lvi_MouseLeave;
            lvSavedBuilds.Items.Add(lvi);
        }

        private void SaveBuildsToFile()
        {
            _persistentData.SaveBuilds(lvSavedBuilds.Items);
        }

        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lvi = (ListViewItem) sender;
            tbSkillURL.Text = ((PoEBuild) lvi.Content).Url;
            btnLoadBuild_Click(this, null); // loading the build
        }


        #region LoadingWindow

        private void StartLoadingWindow()
        {
            _loadingWindow = new LoadingWindow();
            _loadingWindow.Show();
            Thread.Sleep(400);
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        private void UpdateLoadingWindow(double c, double max)
        {
            _loadingWindow.progressBar1.Maximum = max;
            _loadingWindow.progressBar1.Value = c;
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            if(Equals(c, max))
                Thread.Sleep(100);
        }

        private void CloseLoadingWindow()
        {
            _loadingWindow.Close();
        }

        #endregion


        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbCharLink.Text = "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchUpdate();
        }
        
        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            SearchUpdate();
        }

        private void SearchUpdate()
        {
            Tree.HighlightNodes(tbSearch.Text, checkBox1.IsChecked != null && checkBox1.IsChecked.Value);
        }


        private void tbSkillURL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbSkillURL.SelectAll();
        }
    
        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            _undoList.Push(tbSkillURL.Text);
        }

        private void tbSkillURL_Undo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Undo();
        }

        private void tbSkillURL_Undo()
        {
            if (_undoList.Count <= 0) return;
            if (_undoList.Peek() == tbSkillURL.Text && _undoList.Count > 1)
            {
                _undoList.Pop();
                tbSkillURL_Undo();
            }
            else if (_undoList.Peek() != tbSkillURL.Text)
            {
                _redoList.Push(tbSkillURL.Text);
                tbSkillURL.Text = _undoList.Pop();
                Tree.LoadFromURL(tbSkillURL.Text);
                tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
            }
        }

        private void tbSkillURL_Redo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Redo();
        }

        private void tbSkillURL_Redo()
        {
            if (_redoList.Count <= 0) return;
            if (_redoList.Peek() == tbSkillURL.Text && _redoList.Count > 1)
            {
                _redoList.Pop();
                tbSkillURL_Redo();
            }
            else if (_redoList.Peek() != tbSkillURL.Text)
            {
                tbSkillURL.Text = _redoList.Pop();
                Tree.LoadFromURL(tbSkillURL.Text);
                tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
            }
        }

        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lvl;
            if (!int.TryParse(tbLevel.Text, out lvl)) return;
            Tree.Level = lvl;
            UpdateAllAttributeList();
        }

        #region Builds DragAndDrop

        private void ListViewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragAndDropStartPoint = e.GetPosition(lvSavedBuilds);
        }

        private void ListViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(lvSavedBuilds);

                if (Math.Abs(position.X - _dragAndDropStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragAndDropStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    BeginDrag(e);
                }
            }
        }

        private void BeginDrag(MouseEventArgs e)
        {
            var listView = lvSavedBuilds;
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject) e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            var name = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;

            var data = new DataObject("myFormat", name);
            DragDrop.DoDragDrop(lvSavedBuilds, data, DragDropEffects.Move);

            //cleanup 
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
                SaveBuildsToFile();
            }
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }


        private void ListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                var name = e.Data.GetData("myFormat");
                ListView listView = lvSavedBuilds;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    var nameToReplace = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    int index = listView.Items.IndexOf(nameToReplace);

                    if (index >= 0)
                    {
                        listView.Items.Remove(name);
                        listView.Items.Insert(index, name);
                    }
                }
                else
                {
                    listView.Items.Remove(name);
                    listView.Items.Add(name);
                }
            }
        }

        private void InitialiseAdorner(UIElement listViewItem)
        {
            var brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner(listViewItem, listViewItem.RenderSize, brush) {Opacity = 0.5};
            _layer = AdornerLayer.GetAdornerLayer(lvSavedBuilds);
            _layer.Add(_adorner);
        }

        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (Equals(e.OriginalSource, lvSavedBuilds))
            {
                var p = e.GetPosition(lvSavedBuilds);
                var r = VisualTreeHelper.GetContentBounds(lvSavedBuilds);
                if (!r.Contains(p))
                {
                    e.Handled = true;
                }
            }
        }

        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(lvSavedBuilds).X - _dragAndDropStartPoint.X;
                _adorner.OffsetTop = args.GetPosition(lvSavedBuilds).Y - _dragAndDropStartPoint.Y;
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #endregion

        #region Theme

        private void mnuSetTheme_Click(object sender, RoutedEventArgs e)

        {
            if (sender.Equals(mnuViewThemeLight))
            {
                SetLightTheme();
                _persistentData.Options.OptionsTheme = OptionsTheme.Light;
            }
            else
            {
                SetDarkTheme();
                _persistentData.Options.OptionsTheme = OptionsTheme.Dark;
            }
        }

        private void SetTheme(OptionsTheme theme)
        {
            switch (theme)
            {
                case OptionsTheme.Light:
                    SetLightTheme();
                    break;
                case OptionsTheme.Dark:
                    SetDarkTheme();
                    break;
            }
        }

        private void SetLightTheme()
        {
            var accent = ThemeManager.Accents.First(x => x.Name == "Steel");
            var theme = ThemeManager.GetAppTheme("BaseLight");
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            mnuViewThemeLight.IsChecked = true;
        }

        private void SetDarkTheme()
        {
            var accent = ThemeManager.Accents.First(x => x.Name == "Steel");
            var theme = ThemeManager.GetAppTheme("BaseDark");
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            mnuViewThemeDark.IsChecked = true;
        }

        #endregion

        #region Legacy

        /// <summary>
        /// Import builds from legacy build save file "savedBuilds" to PersistentData.xml.
        /// Warning: This will remove the "savedBuilds"
        /// </summary>
        private void ImportLegacySavedBuilds()
        {
            try
            {
                if (File.Exists("savedBuilds"))
                {
                    var saved_builds = new List<PoEBuild>();
                    var builds = File.ReadAllText("savedBuilds").Split('\n');
                    foreach (var b in builds)
                    {
                        var description = b.Split(';')[0].Split('|')[1];
                        var poeClass = description.Split(',')[0].Trim();
                        var pointsUsed = description.Split(',')[1].Trim().Split(' ')[0].Trim();

                        if (HasBuildNote(b))
                        {

                            saved_builds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], poeClass, pointsUsed,
                                b.Split(';')[1].Split('|')[0], b.Split(';')[1].Split('|')[1]));
                        }
                        else
                        {
                            saved_builds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], poeClass, pointsUsed,
                                b.Split(';')[1], "Right Click to add build note"));
                        }
                    }
                    lvSavedBuilds.Items.Clear();
                    foreach (var lvi in saved_builds.Select(build => new ListViewItem {Content = build}))
                    {
                        AddItemToSavedBuilds(lvi);
                    }
                    File.Move("savedBuilds", "savedBuilds.old");
                    SaveBuildsToFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load the saved builds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool HasBuildNote(string b)
        {
            var buildNoteTest = b.Split(';')[1].Split('|');
            return buildNoteTest.Length > 1;
        }

        #endregion
    }
}