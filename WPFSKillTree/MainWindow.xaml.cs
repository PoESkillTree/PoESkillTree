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
        private static readonly Action emptyDelegate = delegate { };
        private readonly List<string> allAttributesList = new List<string>();
        private readonly List<string> attiblist = new List<string>();
        private readonly Regex backreplace = new Regex("#");
        private readonly ToolTip sToolTip = new ToolTip();
        private readonly List<PoEBuild> savedBuilds = new List<PoEBuild>();
        private ListCollectionView AllAttributeCollection;
        private ListCollectionView AttibuteCollection;
        private RenderTargetBitmap ClipboardBmp;

        private ItemAttributes ItemAttributes;
        private SkillTree Tree;
        private string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private Vector2D addtransform;
        private bool justLoaded;
        private string lasttooltip;

        private LoadingWindow loadingWindow;
        private Vector2D multransform;

        private List<ushort> prePath;
        private HashSet<ushort> toRemove;

        Stack<string> undoList = new Stack<string>();
        Stack<string> redoList = new Stack<string>();

        private Point _dragAndDropStartPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;

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
                s = backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        public static string MakePoeUrl(string Url)
        {
            try
            {
                if (Url.Length <= 12)
                {
                    return Url;
                }
                if (!Url.ToLower().StartsWith("http") && !Url.ToLower().StartsWith("ftp"))
                {
                    Url = "http://" + Url;
                }
                WebRequest request = WebRequest.Create("http://poeurl.com/shrink.php?url=" + Url);
                WebResponse res = request.GetResponse();
                string text;
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch (Exception)
            {
                return Url;
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
            if (ItemAttributes != null)
            {
                Dictionary<string, List<float>> attritemp = Tree.SelectedAttributesWithoutImplicit;
                foreach (ItemAttributes.Attribute mod in ItemAttributes.NonLocalMods)
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

                allAttributesList.Clear();
                foreach (string item in (attritemp.Select(InsertNumbersInAttributes)))
                {
                    allAttributesList.Add(item);
                }
                AllAttributeCollection.Refresh();
            }
            tbSkillURL.Text = Tree.SaveToURL();
            UpdateAttributeList();
        }

        public void UpdateAttributeList()
        {
            attiblist.Clear();
            foreach (string item in (Tree.SelectedAttributes.Select(InsertNumbersInAttributes)))
            {
                attiblist.Add(item);
            }
            AttibuteCollection.Refresh();
            tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            File.WriteAllText("skilltreeAddress.txt", tbSkillURL.Text + "\n" + tbLevel.Text);

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
            AttibuteCollection = new ListCollectionView(attiblist);
            listBox1.ItemsSource = AttibuteCollection;
            // AttibuteCollection.CustomSort = 
            var pgd = new PropertyGroupDescription("");
            pgd.Converter = new GroupStringConverter();
            AttibuteCollection.GroupDescriptions.Add(pgd);

            AllAttributeCollection = new ListCollectionView(allAttributesList);
            AllAttributeCollection.GroupDescriptions.Add(pgd);
            lbAllAttr.ItemsSource = AllAttributeCollection;

            Tree = SkillTree.CreateSkillTree(startLoadingWindow, updatetLoadingWindow, CloseLoadingWindow);
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);


            Tree.Chartype = Tree.CharName.IndexOf(((string) ((ComboBoxItem) cbCharType.SelectedItem).Content).ToUpper());
            Tree.UpdateAvailNodes();
            UpdateAllAttributeList();

            multransform = Tree.TRect.Size / recSkillTree.RenderSize.Height;
            addtransform = Tree.TRect.TopLeft;

            // loading last build
            if (File.Exists("skilltreeAddress.txt"))
            {
                string s = File.ReadAllText("skilltreeAddress.txt");
                tbSkillURL.Text = s.Split('\n')[0];
                tbLevel.Text = s.Split('\n')[1];
                button2_Click(this, new RoutedEventArgs());
                justLoaded = false;
            }

            // loading saved build
            try
            {
                if (File.Exists("savedBuilds"))
                {
                    string[] builds = File.ReadAllText("savedBuilds").Split('\n');
                    foreach (string b in builds)
                    {
                        savedBuilds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], b.Split(';')[0].Split('|')[1],
                            b.Split(';')[1]));
                    }
                    //flyout_builds.IsOpen = true; //For some reason no text will show if the flyout is hiden here
                    lvSavedBuilds.Items.Clear();
                    foreach (PoEBuild build in savedBuilds)
                    {
                        var lvi = new ListViewItem
                        {
                            Content = build
                        };
                        lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                        lvSavedBuilds.Items.Add(lvi);
                    }
                    //flyout_builds.IsOpen = false;
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

            v = v * multransform + addtransform;
            SkillTree.SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillTree.SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50));
            if (nodes != null && nodes.Count() != 0)
            {
                node = nodes.First().Value;

                if (node.spc == null)
                {
                    if (Tree.SkilledNodes.Contains(node.id))
                    {
                        Tree.ForceRefundNode(node.id);
                        UpdateAllAttributeList();

                        prePath = Tree.GetShortestPathTo(node.id);
                        Tree.DrawPath(prePath);
                    }
                    else if (prePath != null)
                    {
                        foreach (ushort i in prePath)
                        {
                            Tree.SkilledNodes.Add(i);
                        }
                        UpdateAllAttributeList();
                        Tree.UpdateAvailNodes();

                        toRemove = Tree.ForceRefundNodePreview(node.id);
                        if (toRemove != null)
                            Tree.DrawRefundPreview(toRemove);
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
            sToolTip.IsOpen = false;
        }

        private void border1_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(border1.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * multransform + addtransform;
            textBox1.Text = "" + v.X;
            textBox2.Text = "" + v.Y;
            SkillTree.SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillTree.SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50));
            if (nodes != null && nodes.Count() != 0)
                node = nodes.First().Value;

            if (node != null && node.Attributes.Count != 0)
            {
                string tooltip = node.name + "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(sToolTip.IsOpen && lasttooltip == tooltip))
                {
                    sToolTip.Content = tooltip;
                    sToolTip.IsOpen = true;
                    lasttooltip = tooltip;
                }
                if (Tree.SkilledNodes.Contains(node.id))
                {
                    toRemove = Tree.ForceRefundNodePreview(node.id);
                    if (toRemove != null)
                        Tree.DrawRefundPreview(toRemove);
                }
                else
                {
                    prePath = Tree.GetShortestPathTo(node.id);
                    Tree.DrawPath(prePath);
                }
            }
            else
            {
                sToolTip.Tag = false;
                sToolTip.IsOpen = false;
                prePath = null;
                toRemove = null;
                if (Tree != null)
                {
                    Tree.ClearPath();
                }
            }
        }

        private void btnCopyStats_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (string at in attiblist)
            {
                sb.AppendLine(at);
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
                ItemAttributes = new ItemAttributes(fileDialog.FileName);
                lbItemAttr.ItemsSource = ItemAttributes.Attributes;
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
            if ((bool) formBuildName.ShowDialog())
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
            ;

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
            int maxsize = 3000;
            Geometry geometry = Tree.picActiveLinks.Clip;
            Rect2D contentBounds = Tree.picActiveLinks.ContentBounds;
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

            ClipboardBmp = new RenderTargetBitmap((int) xmax, (int) ymax, 96, 96, PixelFormats.Pbgra32);
            var db = new VisualBrush(Tree.SkillTreeVisual);
            db.ViewboxUnits = BrushMappingMode.Absolute;
            db.Viewbox = contentBounds;
            var dw = new DrawingVisual();

            using (DrawingContext dc = dw.RenderOpen())
            {
                dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
            }
            ClipboardBmp.Render(dw);
            ClipboardBmp.Freeze();

            Clipboard.SetImage(ClipboardBmp);

            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
        }

        private void ImportItems_Click(object sender, RoutedEventArgs e)
        {
            string filetoload = "";
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


            ItemAttributes = new ItemAttributes(filetoload);
            lbItemAttr.ItemsSource = ItemAttributes.Attributes;
            UpdateAllAttributeList();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
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
                    urlString = urlString.Replace(poebuilderTree, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeO, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeWWW, TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeOWWW, TreeAddress);
                    tbSkillURL.Text = urlString;
                    Tree.LoadFromURL(urlString);
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
                    Tree.LoadFromURL(tbSkillURL.Text);

                justLoaded = true;
                //cleans the default tree on load if 2
                if (justLoaded)
                {
                    if (undoList.Count > 1)
                    {
                        string holder = undoList.Pop();
                        undoList.Pop();
                        undoList.Push(holder);
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

        private void CloseLoadingWindow()
        {
            loadingWindow.Close();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (justLoaded)
            {
                justLoaded = false;
                return;
            }

            if (Tree == null)
                return;
            SkillTree.SkillNode startnode =
                Tree.Skillnodes.First(
                    nd => nd.Value.name.ToUpper() == (Tree.CharName[cbCharType.SelectedIndex]).ToUpper()).Value;
            Tree.SkilledNodes.Clear();
            Tree.SkilledNodes.Add(startnode.id);
            Tree.Chartype = Tree.CharName.IndexOf((Tree.CharName[cbCharType.SelectedIndex]).ToUpper());
            Tree.UpdateAvailNodes();
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
                expanderMargin.Left = 210;
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
            sToolTip.IsOpen = false;
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
                rawBuilds.Append(build.name + '|' + build.description + ';' + build.url + '\n');
            }
            File.WriteAllText("savedBuilds", rawBuilds.ToString().Trim());
        }

        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lvi = (ListViewItem) sender;
            tbSkillURL.Text = ((PoEBuild) lvi.Content).url;
            button2_Click(this, null); // loading the build
        }

        private void startLoadingWindow()
        {
            loadingWindow = new LoadingWindow();
            loadingWindow.Show();
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbCharLink.Text = "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tree.HighlightNodes(tbSearch.Text, checkBox1.IsChecked.Value);
        }

        private void tbSkillURL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbSkillURL.SelectAll();
        }
    
        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            undoList.Push(tbSkillURL.Text);
        }

        private void tbSkillURL_Undo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Undo();
        }

        private void tbSkillURL_Undo()
        {
            if (undoList.Count > 0)
            {
                if (undoList.Peek() == tbSkillURL.Text && undoList.Count > 1)
                {
                    undoList.Pop();
                    tbSkillURL_Undo();
                }
                else if (undoList.Peek() != tbSkillURL.Text)
                {
                    redoList.Push(tbSkillURL.Text);
                    tbSkillURL.Text = undoList.Pop();
                    Tree.LoadFromURL(tbSkillURL.Text);
                    tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
                }
            }
        }

        private void tbSkillURL_Redo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Redo();
        }

        private void tbSkillURL_Redo()
        {
            if (redoList.Count > 0)
            {
                if (redoList.Peek() == tbSkillURL.Text && redoList.Count > 1)
                {
                    redoList.Pop();
                    tbSkillURL_Redo();
                }
                else if (redoList.Peek() != tbSkillURL.Text)
                {
                    tbSkillURL.Text = redoList.Pop();
                    Tree.LoadFromURL(tbSkillURL.Text);
                    tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
                }
            }
        }

        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lvl = 0;
            if (int.TryParse(tbLevel.Text, out lvl))
            {
                Tree.Level = lvl;
                UpdateAllAttributeList();
            }
        }

        private void updatetLoadingWindow(double c, double max)
        {
            loadingWindow.progressBar1.Maximum = max;
            loadingWindow.progressBar1.Value = c;
            loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate);
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

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(lvSavedBuilds as Visual);
            _layer.Add(_adorner);
        }


        void ListViewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this._dragIsOutOfScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }


        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == lvSavedBuilds)
            {
                Point p = e.GetPosition(lvSavedBuilds);
                Rect r = VisualTreeHelper.GetContentBounds(lvSavedBuilds);
                if (!r.Contains(p))
                {
                    this._dragIsOutOfScope = true;
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

        [ValueConversion(typeof (string), typeof (string))]
        public class GroupStringConverter : IValueConverter
        {
            public static List<string[]> Groups = new List<string[]>
            {
                new[] {"all attrib", "BaseStats"},
                new[] {"dex", "BaseStats"},
                new[] {"intel", "BaseStats"},
                new[] {"stre", "BaseStats"},
                new[] {"armour", "Defense"},
                new[] {"defence", "Defense"},
                new[] {"evasi", "Defense"},
                new[] {"life", "Defense"},
                new[] {"move", "Defense"},
                new[] {"resist", "Defense"},
                new[] {"shield", "Defense"},
                new[] {"charge", "Charge"},
                new[] {"area", "Spell"},
                new[] {"buff", "Spell"},
                new[] {"cast", "Spell"},
                new[] {"mana", "Spell"},
                new[] {"spell", "Spell"},
                new[] {"accur", "Weapon"},
                new[] {"attack", "Weapon"},
                new[] {"axe", "Weapon"},
                new[] {"bow", "Weapon"},
                new[] {"claw", "Weapon"},
                new[] {"dagg", "Weapon"},
                new[] {"dual wiel", "Weapon"},
                new[] {"mace", "Weapon"},
                new[] {"melee phys", "Weapon"},
                new[] {"physical dam", "Weapon"},
                new[] {"pierc", "Weapon"},
                new[] {"proj", "Weapon"},
                new[] {"staff", "Weapon"},
                new[] {"stav", "Weapon"},
                new[] {"wand", "Weapon"},
                new[] {"weapon", "Weapon"},
                new[] {"crit", "Crit"},
                new[] {"trap", "Traps"},
                new[] {"spectre", "Minion"},
                new[] {"skele", "Minion"},
                new[] {"zombie", "Minion"},
                new[] {"minio", "Minion"},
            };

            static GroupStringConverter()
            {
                if (!File.Exists("groups.txt"))
                    return;
                Groups.Clear();
                foreach (string s in File.ReadAllLines("groups.txt"))
                {
                    string[] sa = s.Split(',');
                    Groups.Add(sa);
                }
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var s = (string) value;
                foreach (var gp in Groups)
                {
                    if (s.ToLower().Contains(gp[0].ToLower()))
                    {
                        return gp[1];
                    }
                }
                return "Everything else";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class NumberLessStringComparer : IComparer<string>
        {
            private static readonly Regex numberfilter = new Regex(@"[0-9\\.]+");

            public int Compare(string x, string y)
            {
                return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
            }
        }
    }

    internal class PoEBuild
    {
        public string description;
        public string name;
        public string url;

        public PoEBuild(string n, string d, string u)
        {
            name = n;
            description = d;
            url = u;
        }

        public override string ToString()
        {
            return name + '\n' + description;
        }
    }
}