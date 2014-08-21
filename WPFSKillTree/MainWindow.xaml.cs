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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows.Documents;

namespace POESKillTree
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly Action EmptyDelegate = delegate { };
        private readonly List<string> _allAttributesList = new List<string>();
        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
        private readonly List<PoEBuild> _savedBuilds = new List<PoEBuild>();
        private ListCollectionView _allAttributeCollection;
        private ListCollectionView _attibuteCollection;
        private RenderTargetBitmap _clipboardBmp;

        private ItemAttributes _itemAttributes;
        private SkillTree _tree;
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
                Dictionary<string, List<float>> attritemp = _tree.SelectedAttributesWithoutImplicit;
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

                foreach (var a in _tree.ImplicitAttributes(attritemp))
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
                    _allAttributesList.Add(item);
                }
                _allAttributeCollection.Refresh();
            }
            tbSkillURL.Text = _tree.SaveToURL();
            UpdateAttributeList();
        }

        public void UpdateAttributeList()
        {
            _attiblist.Clear();
            foreach (var item in (_tree.SelectedAttributes.Select(InsertNumbersInAttributes)))
            {
                _attiblist.Add(new Attribute(item));
            }
            _attibuteCollection.Refresh();
            tbUsedPoints.Text = "" + (_tree.SkilledNodes.Count - 1);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            File.WriteAllText("skilltreeAddress.txt", tbSkillURL.Text + Environment.NewLine + tbLevel.Text);

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
            else
            {
                if (File.Exists("savedBuilds"))
                {
                    File.Delete("savedBuilds");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _attibuteCollection = new ListCollectionView(_attiblist);
            listBox1.ItemsSource = _attibuteCollection;
            // AttibuteCollection.CustomSort = 
            var pgd = new PropertyGroupDescription("");
            pgd.Converter = new GroupStringConverter();
            _attibuteCollection.GroupDescriptions.Add(pgd);

            _allAttributeCollection = new ListCollectionView(_allAttributesList);
            _allAttributeCollection.GroupDescriptions.Add(pgd);
            lbAllAttr.ItemsSource = _allAttributeCollection;

            _tree = SkillTree.CreateSkillTree(startLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
            recSkillTree.Fill = new VisualBrush(_tree.SkillTreeVisual);


            _tree.Chartype = _tree.CharName.IndexOf(((string) ((ComboBoxItem) cbCharType.SelectedItem).Content).ToUpper());
            _tree.UpdateAvailNodes();
            UpdateAllAttributeList();

            _multransform = _tree.TRect.Size / recSkillTree.RenderSize.Height;
            _addtransform = _tree.TRect.TopLeft;

            // loading last build
            if (File.Exists("skilltreeAddress.txt"))
            {
                string s = File.ReadAllText("skilltreeAddress.txt");
                tbSkillURL.Text = s.Split('\n')[0];
                tbLevel.Text = s.Split('\n')[1];
                button2_Click(this, new RoutedEventArgs());
                _justLoaded = false;
            }

            // loading saved build
            try
            {
                if (File.Exists("savedBuilds"))
                {
                    var builds = File.ReadAllText("savedBuilds").Split('\n');
                    foreach (var b in builds)
                    {
                        _savedBuilds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], b.Split(';')[0].Split('|')[1],
                            b.Split(';')[1]));
                    }
                    lvSavedBuilds.Items.Clear();
                    foreach (var lvi in _savedBuilds.Select(build => new ListViewItem {Content = build}))
                    {
                        lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                        lvSavedBuilds.Items.Add(lvi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load the saved builds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Q:
                        HideAttributes();
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

            IEnumerable<KeyValuePair<ushort, SkillTree.SkillNode>> nodes =
                _tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
            {
                var node = nodes.First().Value;

                if (node.spc == null)
                {
                    if (_tree.SkilledNodes.Contains(node.id))
                    {
                        _tree.ForceRefundNode(node.id);
                        UpdateAllAttributeList();

                        _prePath = _tree.GetShortestPathTo(node.id);
                        _tree.DrawPath(_prePath);
                    }
                    else if (_prePath != null)
                    {
                        foreach (ushort i in _prePath)
                        {
                            _tree.SkilledNodes.Add(i);
                        }
                        UpdateAllAttributeList();
                        _tree.UpdateAvailNodes();

                        _toRemove = _tree.ForceRefundNodePreview(node.id);
                        if (_toRemove != null)
                            _tree.DrawRefundPreview(_toRemove);
                    }
                }
            }
            tbSkillURL.Text = _tree.SaveToURL();
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
            SkillTree.SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillTree.SkillNode>> nodes =
                _tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
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
                if (_tree.SkilledNodes.Contains(node.id))
                {
                    _toRemove = _tree.ForceRefundNodePreview(node.id);
                    if (_toRemove != null)
                        _tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = _tree.GetShortestPathTo(node.id);
                    _tree.DrawPath(_prePath);
                }
            }
            else
            {
                _sToolTip.Tag = false;
                _sToolTip.IsOpen = false;
                _prePath = null;
                _toRemove = null;
                if (_tree != null)
                {
                    _tree.ClearPath();
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
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            bool? ftoload = fileDialog.ShowDialog(this);
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
                ((ListViewItem) lvSavedBuilds.SelectedItem).Content =
                    new PoEBuild(((ListViewItem) lvSavedBuilds.SelectedItem).Content.ToString().Split('\n')[0],
                        cbCharType.Text + ", " + tbUsedPoints.Text + " points used", tbSkillURL.Text);
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
                        new PoEBuild(formBuildName.getBuildName(),
                            cbCharType.Text + ", " + tbUsedPoints.Text + " points used", tbSkillURL.Text)
                };
                lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                lvSavedBuilds.Items.Add(lvi);
            }

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
            else
            {
                if (File.Exists("savedBuilds"))
                {
                    File.Delete("savedBuilds");
                }
            }
        }

        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            const int maxsize = 3000;
            Rect2D contentBounds = _tree.picActiveLinks.ContentBounds;
            contentBounds *= 1.2;


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

            _clipboardBmp = new RenderTargetBitmap((int) xmax, (int) ymax, 96, 96, PixelFormats.Pbgra32);
            var db = new VisualBrush(_tree.SkillTreeVisual);
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

            recSkillTree.Fill = new VisualBrush(_tree.SkillTreeVisual);
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

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(_tree, tbSkillURL.Text);
                    tbSkillURL.Text = _tree.SaveToURL();
                }
                else if (tbSkillURL.Text.Contains("poebuilder.com/"))
                {
                    const string poebuilderTree = "https://poebuilder.com/character/";
                    const string poebuilderTreeWWW = "https://www.poebuilder.com/character/";
                    const string poebuilderTreeOWWW = "http://www.poebuilder.com/character/";
                    const string poebuilderTreeO = "http://poebuilder.com/character/";
                    var urlString = tbSkillURL.Text;
                    urlString = urlString.Replace(poebuilderTree, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeO, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeWWW, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeOWWW, TreeAddress);
                    tbSkillURL.Text = urlString;
                    _tree.LoadFromURL(urlString);
                }
                else if (tbSkillURL.Text.Contains("tinyurl.com/"))
                {
                    var request = (HttpWebRequest) WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse) request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    button2_Click(sender, e);
                }
                else if (tbSkillURL.Text.Contains("poeurl.com/"))
                {
                    tbSkillURL.Text = tbSkillURL.Text.Replace("http://poeurl.com/",
                        "http://poeurl.com/redirect.php?url=");
                    var request = (HttpWebRequest) WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse) request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    button2_Click(sender, e);
                }
                else
                    _tree.LoadFromURL(tbSkillURL.Text);

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
                cbCharType.SelectedIndex = _tree.Chartype;
                UpdateAllAttributeList();
            }
            catch (Exception)
            {
                MessageBox.Show("The Build you tried to load, is invalid");
            }
        }

        private void SkillHighlightedNodes_Click(object sender, RoutedEventArgs e)
        {
            _tree.SkillAllHighligtedNodes();
            UpdateAllAttributeList();
        }

        private void RecSkillTree_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_tree == null)
                return;
            _tree.Reset();
            UpdateAllAttributeList();
        }

        private void CloseLoadingWindow()
        {
            _loadingWindow.Close();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_justLoaded)
            {
                _justLoaded = false;
                return;
            }

            if (_tree == null)
                return;
            SkillTree.SkillNode startnode =
                _tree.Skillnodes.First(
                    nd => nd.Value.name.ToUpper() == (_tree.CharName[cbCharType.SelectedIndex]).ToUpper()).Value;
            _tree.SkilledNodes.Clear();
            _tree.SkilledNodes.Add(startnode.id);
            _tree.Chartype = _tree.CharName.IndexOf((_tree.CharName[cbCharType.SelectedIndex]).ToUpper());
            _tree.UpdateAvailNodes();
            UpdateAllAttributeList();
        }

        private void HideAttributes_Click(object sender, RoutedEventArgs e)
        {
            HideAttributes();
        }

        private void HideAttributes()
        {
            if (tabControl1.Visibility == Visibility.Hidden)
            {
                Thickness margin = Margin;
                margin.Left = 415;
                margin.Right = 4;
                margin.Top = 0;
                margin.Bottom = 29;
                OuterBorder1.Margin = margin;

                hideShit.Content = "-";
                tabControl1.Visibility = Visibility.Visible;
                leftBackground.Visibility = Visibility.Visible;

                var expanderMargin = flyout_builds.Margin;
                expanderMargin.Top = expanderMargin.Top - 17;
                expanderMargin.Left = 208;
                flyout_builds.Margin = expanderMargin;
            }
            else
            {
                Thickness margin = Margin;
                margin.Left = 4;
                margin.Right = 4;
                margin.Top = 34;
                margin.Bottom = 29;
                OuterBorder1.Margin = margin;

                hideShit.Content = "+";
                textBox1.Visibility = Visibility.Hidden;
                textBox2.Visibility = Visibility.Hidden;
                tabControl1.Visibility = Visibility.Hidden;
                leftBackground.Visibility = Visibility.Hidden;

                var expanderMargin = flyout_builds.Margin;
                expanderMargin.Top = expanderMargin.Top + 17;
                expanderMargin.Left = 0;
                flyout_builds.Margin = expanderMargin;
            }
        }

        private void tabControl1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mnuViewAttributes.IsChecked = tabControl1.Visibility == Visibility.Visible;
        }

        private void ToggleBuilds_Click(object sender, RoutedEventArgs e)
        {
            ToggleBuilds();
        }

        private void ToggleBuilds()
        {
            flyout_builds.IsOpen = !flyout_builds.IsOpen;
        }

        private void flyout_builds_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mnuViewBuilds.IsChecked = flyout_builds.Visibility == Visibility.Visible;
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
                object obj = lvSavedBuilds.Items[lvSavedBuilds.SelectedIndex];
                int selectedIndex = lvSavedBuilds.SelectedIndex;
                lvSavedBuilds.Items.RemoveAt(selectedIndex);
                lvSavedBuilds.Items.Insert(selectedIndex - 1, obj);
                lvSavedBuilds.SelectedItem = lvSavedBuilds.Items[selectedIndex - 1];
                lvSavedBuilds.SelectedIndex = selectedIndex - 1;
                lvSavedBuilds.Items.Refresh();

                SaveBuildsToFile();
            }

            else if (e.Key == Key.Down && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                     lvSavedBuilds.SelectedIndex < lvSavedBuilds.Items.Count - 1)
            {
                object obj = lvSavedBuilds.Items[lvSavedBuilds.SelectedIndex];
                int selectedIndex = lvSavedBuilds.SelectedIndex;
                lvSavedBuilds.Items.RemoveAt(selectedIndex);
                lvSavedBuilds.Items.Insert(selectedIndex + 1, obj);
                lvSavedBuilds.SelectedItem = lvSavedBuilds.Items[selectedIndex + 1];
                lvSavedBuilds.SelectedIndex = selectedIndex + 1;
                lvSavedBuilds.Items.Refresh();

                SaveBuildsToFile();
            }
        }

        private void SaveBuildsToFile()
        {
            var rawBuilds = new StringBuilder();
            foreach (ListViewItem lvi in lvSavedBuilds.Items)
            {
                var build = (PoEBuild)lvi.Content;
                rawBuilds.Append(build.Name + '|' + build.Description + ';' + build.Url + '\n');
            }
            File.WriteAllText("savedBuilds", rawBuilds.ToString().Trim());
        }

        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lvi = (ListViewItem) sender;
            tbSkillURL.Text = ((PoEBuild) lvi.Content).Url;
            button2_Click(this, null); // loading the build
        }

        private void startLoadingWindow()
        {
            _loadingWindow = new LoadingWindow();
            _loadingWindow.Show();
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbCharLink.Text = "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tree.HighlightNodes(tbSearch.Text, checkBox1.IsChecked != null && checkBox1.IsChecked.Value);
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
            if (_undoList.Count > 0)
            {
                if (_undoList.Peek() == tbSkillURL.Text && _undoList.Count > 1)
                {
                    _undoList.Pop();
                    tbSkillURL_Undo();
                }
                else if (_undoList.Peek() != tbSkillURL.Text)
                {
                    _redoList.Push(tbSkillURL.Text);
                    tbSkillURL.Text = _undoList.Pop();
                    _tree.LoadFromURL(tbSkillURL.Text);
                    tbUsedPoints.Text = "" + (_tree.SkilledNodes.Count - 1);
                }
            }
        }

        private void tbSkillURL_Redo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Redo();
        }

        private void tbSkillURL_Redo()
        {
            if (_redoList.Count > 0)
            {
                if (_redoList.Peek() == tbSkillURL.Text && _redoList.Count > 1)
                {
                    _redoList.Pop();
                    tbSkillURL_Redo();
                }
                else if (_redoList.Peek() != tbSkillURL.Text)
                {
                    tbSkillURL.Text = _redoList.Pop();
                    _tree.LoadFromURL(tbSkillURL.Text);
                    tbUsedPoints.Text = "" + (_tree.SkilledNodes.Count - 1);
                }
            }
        }

        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lvl;
            if (!int.TryParse(tbLevel.Text, out lvl)) return;
            _tree.Level = lvl;
            UpdateAllAttributeList();
        }

        private void UpdateLoadingWindow(double c, double max)
        {
            _loadingWindow.progressBar1.Maximum = max;
            _loadingWindow.progressBar1.Value = c;
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
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
            ListView listView = lvSavedBuilds;
            ListViewItem listViewItem =
                FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

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

            DataObject data = new DataObject("myFormat", name);
            DragDropEffects de = DragDrop.DoDragDrop(lvSavedBuilds, data, DragDropEffects.Move);

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
    }
}