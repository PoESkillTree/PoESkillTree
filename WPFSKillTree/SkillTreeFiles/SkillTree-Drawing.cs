using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnumsNET;
using HighlightState = PoESkillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using PoESkillTree.Model;
using MoreLinq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Model.Items;
using PoESkillTree.TreeDrawing;
using PoESkillTree.Utils.Wpf;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members
        private static readonly Color TreeComparisonColor = Colors.RoyalBlue;
        private readonly Pen _basePathPen = new Pen(Brushes.DarkSlateGray, 20f);
        private readonly Pen _activePathPen = new Pen(Brushes.DarkKhaki, 15f);
        private readonly Pen _skillOverlayPen = new Pen(Brushes.LawnGreen, 15f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private readonly Pen _refundOverlayPen = new Pen(Brushes.Red, 15f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private const float HighlightFactor = 1.2f;

        private readonly List<(Rect rect, ImageBrush brush)> _faceBrushes = new List<(Rect, ImageBrush)>();
        private readonly List<(Size size, ImageBrush brush)> _nodeSurroundBrushes = new List<(Size, ImageBrush)>();
        private readonly List<(Size size, ImageBrush brush)> _nodeSurroundComparisonBrushes = new List<(Size, ImageBrush)>();
        private readonly Dictionary<bool, (Rect rect, ImageBrush brush)> _startBackgrounds = new Dictionary<bool, (Rect, ImageBrush)>();

        private readonly NodeHighlighter _nodeHighlighter = new NodeHighlighter();
        private readonly IPersistentData _persistentData;
        private readonly List<Tuple<int, Vector2D>> _originalPositions = new List<Tuple<int, Vector2D>>();
        public bool DrawAscendancy;

        public DrawingVisual SkillTreeVisual { get; private set; }
        private DrawingVisual _background;
        private NonAscendancyNodeSurroundDrawer _nodeComparisonSurroundDrawer;
        private DrawingVisual _pathComparisonHighlight;
        private DrawingVisual _paths;
        public DrawingVisual ActivePaths { get; private set; }
        private DrawingVisual _pathOverlay;
        private NonAscendancyNodeIconDrawer _nodeIconDrawer;
        private NonAscendancyNodeIconDrawer _activeNodeIconDrawer;
        private NonAscendancyNodeIconDrawer _itemAllocatedNodeIconDrawer;
        private NonAscendancyNodeSurroundDrawer _nodeSurroundDrawer;
        private NonAscendancyNodeSurroundDrawer _activeNodeSurroundDrawer;
        private NonAscendancyNodeSurroundDrawer _itemAllocatedNodeSurroundDrawer;
        private DrawingVisual _characterFaces;
        private DrawingVisual _highlights;

        private DrawingVisual _ascSkillTreeVisual;
        private DrawingVisual _ascClassFaces;
        private DrawingVisual _ascButtons;
        private AscendancyNodeSurroundDrawer _ascendancyNodeComparisonSurroundDrawer;
        private DrawingVisual _ascPathComparisonHighlight;
        private DrawingVisual _ascPaths;
        private DrawingVisual _ascActivePaths;
        private DrawingVisual _ascPathOverlay;
        private AscendancyNodeIconDrawer _ascendancyNodeIconDrawer;
        private AscendancyNodeIconDrawer _activeAscendancyNodeIconDrawer;
        private AscendancyNodeSurroundDrawer _ascendancyNodeSurroundDrawer;
        private AscendancyNodeSurroundDrawer _activeAscendancyNodeSurroundDrawer;

        private JewelDrawer _jewelDrawer;
        private JewelRadiusDrawer _jewelRadiusDrawer;

        public IReadOnlyList<InventoryItemViewModel> JewelViewModels
        {
            set
            {
                _jewelDrawer.JewelViewModels = value;
                _jewelRadiusDrawer.JewelViewModels = value;
            }
        }
        #endregion

        private void InitialSkillTreeDrawing()
        {

            SkilledNodes.CollectionChanged += SkilledNodes_CollectionChanged;
            HighlightedNodes.CollectionChanged += HighlightedNodes_CollectionChanged;
            _itemAllocatedNodes.CollectionChanged += ItemAllocatedNodesOnCollectionChanged;

            if (_initialized) return;

            InitializeDrawingVisuals();
            InitializeNodeSurroundBrushes();
            InitializeFaceBrushes();
            InitializeDrawers();

            //Drawing
            DrawBackgroundLayer();
            DrawInitialPaths();
            DrawSkillIconsAndSurrounds();
            DrawCharacterFaces();
            DrawAscendancyClasses();
            DrawAscendancyButton();

            //Add all the drawings on one layer
            CreateCombineVisuals();
        }

        private void HighlightedNodes_CollectionChanged(object sender, EventArgs e)
        {
            DrawTreeComparisonHighlight();
        }

        private void SkilledNodes_CollectionChanged(object sender, EventArgs e)
        {
            DrawActiveSkillIconsAndSurrounds();
            DrawActivePaths();
            DrawCharacterFaces();
            _jewelRadiusDrawer.DrawSkilledNodes();
            _jewelDrawer.Draw();
        }

        private void ItemAllocatedNodesOnCollectionChanged(object sender, CollectionChangedEventArgs<SkillNode> args)
        {
            _itemAllocatedNodeIconDrawer.Draw();
            _itemAllocatedNodeSurroundDrawer.Draw();
        }

        /// <summary>
        /// This will initialize all drawing visuals. If a new drawing visual is added then it should be initialized here as well.
        /// </summary>
        private void InitializeDrawingVisuals()
        {
            SkillTreeVisual = new DrawingVisual();
            _background = new DrawingVisual();
            _pathComparisonHighlight = new DrawingVisual();
            _paths = new DrawingVisual();
            ActivePaths = new DrawingVisual();
            _pathOverlay = new DrawingVisual();
            _characterFaces = new DrawingVisual();
            _highlights = new DrawingVisual();

            _ascSkillTreeVisual = new DrawingVisual();
            _ascClassFaces = new DrawingVisual();
            _ascButtons = new DrawingVisual();
            _ascPathComparisonHighlight = new DrawingVisual();
            _ascPaths = new DrawingVisual();
            _ascActivePaths = new DrawingVisual();
            _ascPathOverlay = new DrawingVisual();
        }

        private void InitializeDrawers()
        {
            _nodeIconDrawer = new NonAscendancyNodeIconDrawer(IconInActiveSkills, Skillnodes.Values);
            _activeNodeIconDrawer = new NonAscendancyNodeIconDrawer(IconActiveSkills, SkilledNodes);
            _itemAllocatedNodeIconDrawer = new NonAscendancyNodeIconDrawer(IconActiveSkills, _itemAllocatedNodes);
            _ascendancyNodeIconDrawer = new AscendancyNodeIconDrawer(IconInActiveSkills, Skillnodes.Values);
            _activeAscendancyNodeIconDrawer = new AscendancyNodeIconDrawer(IconActiveSkills, SkilledNodes);

            _nodeSurroundDrawer = new NonAscendancyNodeSurroundDrawer(
                Skillnodes.Values, 1, n => GetNodeSurroundBrushSize(n, 0), n => GetNodeSurroundBrush(n, 0));
            _activeNodeSurroundDrawer = new NonAscendancyNodeSurroundDrawer(
                SkilledNodes, 1, n => GetNodeSurroundBrushSize(n, 1), n => GetNodeSurroundBrush(n, 1));
            _itemAllocatedNodeSurroundDrawer = new NonAscendancyNodeSurroundDrawer(
                _itemAllocatedNodes, 1, n => GetNodeSurroundBrushSize(n, 1), n => GetNodeSurroundBrush(n, 1));
            _nodeComparisonSurroundDrawer = new NonAscendancyNodeSurroundDrawer(
                HighlightedNodes, 1.2, GetComparisonNodeSurroundBrushSize, GetComparisonNodeSurroundBrush);
            _ascendancyNodeSurroundDrawer = new AscendancyNodeSurroundDrawer(
                Skillnodes.Values, 1, n => GetNodeSurroundBrushSize(n, 0), n => GetNodeSurroundBrush(n, 0));
            _activeAscendancyNodeSurroundDrawer = new AscendancyNodeSurroundDrawer(
                SkilledNodes, 1, n => GetNodeSurroundBrushSize(n, 1), n => GetNodeSurroundBrush(n, 1));
            _ascendancyNodeComparisonSurroundDrawer = new AscendancyNodeSurroundDrawer(
                HighlightedNodes, 1.2, GetComparisonNodeSurroundBrushSize, GetComparisonNodeSurroundBrush);

            _jewelDrawer = new JewelDrawer(Assets, Skillnodes);
            _jewelRadiusDrawer = new JewelRadiusDrawer(PoESkillTreeOptions, Skillnodes, SkilledNodes);
        }

        private void CreateCombineVisuals()
        {
            // Top most add will be the bottom most element drawn
            SkillTreeVisual.Children.Add(_background);
            SkillTreeVisual.Children.Add(_nodeComparisonSurroundDrawer.Visual);
            SkillTreeVisual.Children.Add(_pathComparisonHighlight);
            SkillTreeVisual.Children.Add(_paths);
            SkillTreeVisual.Children.Add(ActivePaths);
            SkillTreeVisual.Children.Add(_pathOverlay);
            SkillTreeVisual.Children.Add(_nodeIconDrawer.Visual);
            SkillTreeVisual.Children.Add(_itemAllocatedNodeIconDrawer.Visual);
            SkillTreeVisual.Children.Add(_activeNodeIconDrawer.Visual);
            SkillTreeVisual.Children.Add(_nodeSurroundDrawer.Visual);
            SkillTreeVisual.Children.Add(_itemAllocatedNodeSurroundDrawer.Visual);
            SkillTreeVisual.Children.Add(_activeNodeSurroundDrawer.Visual);
            SkillTreeVisual.Children.Add(_characterFaces);
            SkillTreeVisual.Children.Add(_jewelDrawer.Visual);
            SkillTreeVisual.Children.Add(_jewelRadiusDrawer.Visual);

            _ascSkillTreeVisual.Children.Add(_ascClassFaces);
            _ascSkillTreeVisual.Children.Add(_ascendancyNodeComparisonSurroundDrawer.Visual);
            _ascSkillTreeVisual.Children.Add(_ascPathComparisonHighlight);
            _ascSkillTreeVisual.Children.Add(_ascPaths);
            _ascSkillTreeVisual.Children.Add(_ascActivePaths);
            _ascSkillTreeVisual.Children.Add(_ascPathOverlay);
            _ascSkillTreeVisual.Children.Add(_ascendancyNodeIconDrawer.Visual);
            _ascSkillTreeVisual.Children.Add(_activeAscendancyNodeIconDrawer.Visual);
            _ascSkillTreeVisual.Children.Add(_ascendancyNodeSurroundDrawer.Visual);
            _ascSkillTreeVisual.Children.Add(_activeAscendancyNodeSurroundDrawer.Visual);

            SkillTreeVisual.Children.Add(_ascSkillTreeVisual);
            SkillTreeVisual.Children.Add(_ascButtons);
            SkillTreeVisual.Children.Add(_highlights);
        }

        private void InitializeNodeSurroundBrushes()
        {
            foreach (var background in NodeBackgrounds)
            {
                if (!NodeBackgroundsActive.ContainsKey(background.Key)) continue;
                var normalBrushPImage = Assets[NodeBackgrounds[background.Key]];
                var normalBrush = new ImageBrush
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = normalBrushPImage
                };
                var normalSize = new Size(normalBrushPImage.PixelWidth, normalBrushPImage.PixelHeight);

                var activeBrushPImage = Assets[NodeBackgroundsActive[background.Key]];
                var activeBrush = new ImageBrush
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = activeBrushPImage
                };
                var activeSize = new Size(activeBrushPImage.PixelWidth, activeBrushPImage.PixelHeight);

                _nodeSurroundBrushes.Add((normalSize, normalBrush));
                _nodeSurroundBrushes.Add((activeSize, activeBrush));

                //tree comparison highlight generator
                var outlinecolor = TreeComparisonColor;
                var omask = outlinecolor.B | (uint)outlinecolor.G << 8 | (uint)outlinecolor.R << 16;

                var bitmap = normalBrushPImage;
                var wb = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
                if (wb.Format == PixelFormats.Bgra32)//BGRA is byte order .. little endian in uint reverse it
                {
                    var pixeldata = new uint[wb.PixelHeight * wb.PixelWidth];
                    bitmap.CopyPixels(pixeldata, wb.PixelWidth * 4, 0);
                    for (var i = 0; i < pixeldata.Length; i++)
                    {
                        pixeldata[i] = pixeldata[i] & 0xFF000000 | omask;
                    }
                    wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), pixeldata, wb.PixelWidth * 4, 0);

                    var ibr = new ImageBrush
                    {
                        Stretch = Stretch.Uniform,
                        ImageSource = wb
                    };
                    //doubled so that it matches what is in the other node comparison brush
                    _nodeSurroundComparisonBrushes.Add((normalSize, ibr));
                    _nodeSurroundComparisonBrushes.Add((normalSize, ibr));
                }
                else
                {
                    throw new Exception("Highlight Generator did not generate with the correct byte order");
                }
            }
        }

        private Size GetNodeSurroundBrushSize(SkillNode node, int offset) =>
            _nodeSurroundBrushes[GetNodeSurroundBrushOffset(node) + offset].size;

        private Brush GetNodeSurroundBrush(SkillNode node, int offset) =>
            _nodeSurroundBrushes[GetNodeSurroundBrushOffset(node) + offset].brush;

        private Size GetComparisonNodeSurroundBrushSize(SkillNode node) =>
            _nodeSurroundComparisonBrushes[GetNodeSurroundBrushOffset(node)].size;

        private Brush GetComparisonNodeSurroundBrush(SkillNode node) =>
            _nodeSurroundComparisonBrushes[GetNodeSurroundBrushOffset(node)].brush;

        private static int GetNodeSurroundBrushOffset(SkillNode node) =>
            node.Type switch
            {
                _ when node.IsAscendancyStart => 12,
                PassiveNodeType.Small when node.IsAscendancyNode => 8,
                PassiveNodeType.Small => 0,
                PassiveNodeType.Notable when node.IsAscendancyNode => 10,
                PassiveNodeType.Notable when node.IsBlighted => 14,
                PassiveNodeType.Notable => 2,
                PassiveNodeType.Keystone => 4,
                PassiveNodeType.JewelSocket => 6,
                _ => 0
            };

        private void DrawSkillIconsAndSurrounds(bool onlyAscendancy = false)
        {
            if (DrawAscendancy)
            {
                _ascendancyNodeIconDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
                _ascendancyNodeSurroundDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
            }
            if (!onlyAscendancy)
            {
                _nodeIconDrawer.Draw();
                _nodeSurroundDrawer.Draw();
            }
        }

        private void DrawActiveSkillIconsAndSurrounds(bool onlyAscendancy = false)
        {
            if (DrawAscendancy)
            {
                _activeAscendancyNodeIconDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
                _activeAscendancyNodeSurroundDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
            }
            if (!onlyAscendancy)
            {
                _activeNodeIconDrawer.Draw();
                _activeNodeSurroundDrawer.Draw();
            }
        }

        public void ClearPath()
        {
            _pathOverlay.RenderOpen().Close();
            _ascPathOverlay.RenderOpen().Close();
        }

        public void ClearJewelHighlight()
        {
            _jewelRadiusDrawer.ClearHighlight();
        }

        public void ToggleAscendancyTree()
        {
            DrawAscendancy = !DrawAscendancy;
            DrawAscendancyLayers();
        }

        public void ToggleAscendancyTree(bool draw)
        {
            DrawAscendancy = draw;
            DrawAscendancyLayers();
        }

        private void UpdateAscendancyClassPositions()
        {
            if (!_persistentData.Options.ShowAllAscendancyClasses)
            {
                var ascName = AscendancyClassName;
                var nodeList = Skillnodes.Where(x => x.Value.AscendancyName == ascName && !x.Value.IsAscendancyStart);
                var worldPos = Skillnodes[RootNodeClassDictionary[CharClass]].Position;
                var ascStartNode = AscRootNodeList.First(x => x.AscendancyName == ascName);
                var ascNodeOriginalPos = ascStartNode.Group.Position;
                if (_originalPositions.All(x => x.Item1 != ascStartNode.GroupId))
                    _originalPositions.Add(new Tuple<int, Vector2D>(ascStartNode.GroupId, ascNodeOriginalPos));

                var imageName = "Classes" + ascStartNode.AscendancyName;
                var bitmap = Assets[imageName];

                const int distanceFromStartNodeCenter = 270;
                var dirX = 0.0;
                var dirY = 1.0;
                var distToCentre = Math.Sqrt(worldPos.X * worldPos.X + worldPos.Y * worldPos.Y);
                var isCentered = Math.Abs(worldPos.X) < 10.0 && Math.Abs(worldPos.Y) < 10.0;
                if (!isCentered)
                {
                    dirX = worldPos.X / distToCentre;
                    dirY = -worldPos.Y / distToCentre;
                }
                var ascButtonRot = Math.Atan2(dirX, dirY);
                var imageCx = worldPos.X + (distanceFromStartNodeCenter + bitmap.Height * 1.25) * Math.Cos(ascButtonRot + Math.PI / 2);
                var imageCy = worldPos.Y + (distanceFromStartNodeCenter + bitmap.Width * 1.25) * Math.Sin(ascButtonRot + Math.PI / 2);

                ascStartNode.Group.Position = new Vector2D(imageCx, imageCy);
                var done = new List<SkillNodeGroup> { ascStartNode.Group };

                foreach (var n in nodeList)
                {
                    if (done.Contains(n.Value.Group))
                        continue;
                    if (_originalPositions.All(x => x.Item1 != n.Value.GroupId))
                        _originalPositions.Add(new Tuple<int, Vector2D>(n.Value.GroupId, n.Value.Group.Position));
                    var diffDist = ascNodeOriginalPos - n.Value.Group.Position;

                    n.Value.Group.Position = ascStartNode.Group.Position - diffDist;
                    done.Add(n.Value.Group);
                }
            }
            else
            {
                foreach (var g in _originalPositions)
                {
                    foreach (var n in Skillnodes)
                    {
                        if (g.Item1 != n.Value.GroupId) continue;
                        n.Value.Group.Position = g.Item2;
                    }
                }
                _originalPositions.Clear();
            }
        }

        private void DrawBackgroundLayer()
        {
            if (_initialized) return;
            using (var dc = _background.RenderOpen())
            {
                //These are the images around the groups of nodes 
                BitmapImage[] groupBackgrounds =
                {
                    Assets["PSGroupBackground1"],
                    Assets["PSGroupBackground2"],
                    Assets["PSGroupBackground3"]
                };
                Brush[] groupOrbitBrush =
                {
                    new ImageBrush(Assets["PSGroupBackground1"]),
                    new ImageBrush(Assets["PSGroupBackground2"]),
                    new ImageBrush(Assets["PSGroupBackground3"])
                };
                var imageBrush = groupOrbitBrush[2] as ImageBrush;
                if (imageBrush != null)
                {
                    imageBrush.TileMode = TileMode.FlipXY;
                    imageBrush.Viewport = new Rect(0, 0, 1, .5f);
                }

                #region Background Drawing
                var backgroundBrush = new ImageBrush(Assets["Background1"]) { TileMode = TileMode.Tile };
                backgroundBrush.Viewport = new Rect(0, 0,
                    6 * backgroundBrush.ImageSource.Width / SkillTreeRect.Width,
                    6 * backgroundBrush.ImageSource.Height / SkillTreeRect.Height);
                dc.DrawRectangle(backgroundBrush, null, SkillTreeRect);

                LinearGradientBrush[] linearGradientBrushes =
                {
                    new LinearGradientBrush(new Color(), Colors.Black, new Point(0, 1), new Point(0, 0)), //top
                    new LinearGradientBrush(new Color(), Colors.Black, new Point(0, 0), new Point(1, 0)), //right
                    new LinearGradientBrush(new Color(), Colors.Black, new Point(0, 0), new Point(0, 1)), //bottom
                    new LinearGradientBrush(new Color(), Colors.Black, new Point(1, 0), new Point(0, 0))  //left
                };
                var GradientSize = 250;
                Rect2D[] gradientRect =
                {
                    new Rect2D(SkillTreeRect.Left, SkillTreeRect.Top, SkillTreeRect.Width, GradientSize), //top
                    new Rect2D(SkillTreeRect.Right - GradientSize, SkillTreeRect.Top, GradientSize, SkillTreeRect.Height), //right
                    new Rect2D(SkillTreeRect.Left, SkillTreeRect.Bottom - GradientSize, SkillTreeRect.Width, GradientSize), //bottom
                    new Rect2D(SkillTreeRect.Left, SkillTreeRect.Top, GradientSize, SkillTreeRect.Height) //left
                };

                if (linearGradientBrushes.Length != gradientRect.Length)
                    throw new Exception("Gradient must have a Rectangle for each Brush");
                for (var i = 0; i < linearGradientBrushes.Length; i++)
                    dc.DrawRectangle(linearGradientBrushes[i], null, gradientRect[i]);
                #endregion
                #region SkillNodeGroup Background Drawing

                foreach (var i in PoESkillTree.Groups)
                {
                    var skillNodeGroup = i.Value;
                    if (skillNodeGroup.Nodes.Where(n => n.IsAscendancyNode).ToArray().Length > 0)
                        continue;
                    var cgrp = skillNodeGroup.OccupiedOrbits?.Keys.Where(ng => ng <= 3) ?? Enumerable.Empty<int>();
                    var enumerable = cgrp as IList<int> ?? cgrp.ToList();
                    if (!enumerable.Any()) continue;
                    var maxr = enumerable.Max(ng => ng);
                    if (maxr == 0) continue;
                    maxr = maxr > 3 ? 2 : maxr - 1;
                    var maxfac = maxr == 2 ? 2 : 1;
                    dc.DrawRectangle(groupOrbitBrush[maxr], null,
                        new Rect(
                            skillNodeGroup.Position -
                            new Vector2D(groupBackgrounds[maxr].PixelWidth * 1.25, groupBackgrounds[maxr].PixelHeight * 1.25 * maxfac),
                            new Size(groupBackgrounds[maxr].PixelWidth * 2.5, groupBackgrounds[maxr].PixelHeight * 2.5 * maxfac)));
                }
                #endregion
            }
        }

        private static void DrawConnection(DrawingContext dc, Pen pen2, SkillNode n1, SkillNode n2)
        {
            if (!n1.VisibleNeighbors.Contains(n2) || !n2.VisibleNeighbors.Contains(n1)) return;
            if (n1.Group == n2.Group && n1.OrbitRadiiIndex == n2.OrbitRadiiIndex)
            {
                if (n1.Arc - n2.Arc > 0 && n1.Arc - n2.Arc <= Math.PI ||
                    n1.Arc - n2.Arc < -Math.PI)
                {
                    dc.DrawArc(null, pen2, n1.Position, n2.Position,
                        new Size(SkillNode.OrbitRadii[n1.OrbitRadiiIndex],
                            SkillNode.OrbitRadii[n1.OrbitRadiiIndex]));
                }
                else
                {
                    dc.DrawArc(null, pen2, n2.Position, n1.Position,
                        new Size(SkillNode.OrbitRadii[n1.OrbitRadiiIndex],
                            SkillNode.OrbitRadii[n1.OrbitRadiiIndex]));
                }
            }
            else
            {
                var draw = true;
                foreach (var attibute in n1.StatDefinitions)
                {
                    if (AscendantClassStartRegex.IsMatch(attibute))
                        draw = false;
                }
                if (n1.Type == PassiveNodeType.Mastery || n2.Type == PassiveNodeType.Mastery)
                    draw = false;
                if (draw)
                    dc.DrawLine(pen2, n1.Position, n2.Position);
            }
        }

        private void DrawInitialPaths(bool onlyAscendancy = false)
        {
            DrawingContext? dc = null;
            var adc = _ascPaths.RenderOpen();

            if (!onlyAscendancy)
                dc = _paths.RenderOpen();
            var seen = new HashSet<SkillNode>();
            foreach (var i in Skillnodes)
            {
                var n1 = i.Value;
                seen.Add(n1);
                foreach (var n2 in n1.VisibleNeighbors)
                {
                    if (seen.Contains(n2)) continue;
                    if (n2.IsAscendancyNode)
                    {
                        if (!DrawAscendancy) continue;
                        if (_persistentData.Options.ShowAllAscendancyClasses || n2.AscendancyName == AscendancyClassName)
                            DrawConnection(adc, _basePathPen, n2, n1);
                    }
                    else
                    {
                        if (onlyAscendancy) continue;
                        DrawConnection(dc!, _basePathPen, n2, n1);
                    }
                }
            }
            adc.Close();
            dc?.Close();
        }

        private void DrawCharacterFaces()
        {
            using (var dc = _characterFaces.RenderOpen())
            {
                foreach (var charClass in Enums.GetValues<CharacterClass>())
                {
                    var pos = Skillnodes[RootNodeClassDictionary[charClass]].Position;
                    dc.DrawRectangle(_startBackgrounds[false].brush, null,
                        new Rect(
                            pos - new Vector2D(_startBackgrounds[false].rect.Width, _startBackgrounds[false].rect.Height),
                            pos + new Vector2D(_startBackgrounds[false].rect.Width, _startBackgrounds[false].rect.Height)));
                    if (CharClass == charClass)
                    {
                        var i = (int)CharClass;
                        var (rect, brush) = _faceBrushes[i];
                        dc.DrawRectangle(brush, null,
                            new Rect(pos - new Vector2D(rect.Width, rect.Height),
                                pos + new Vector2D(rect.Width, rect.Height)));

                        var charBaseAttr = CharBaseAttributes[CharClass].ToDictionary();

                        var text = CreateAttributeText(charBaseAttr["+# to Intelligence"].ToString(CultureInfo.InvariantCulture), Brushes.DodgerBlue);
                        dc.DrawText(text, pos - new Vector2D(19, 117));

                        text = CreateAttributeText(charBaseAttr["+# to Strength"].ToString(CultureInfo.InvariantCulture), Brushes.IndianRed);
                        dc.DrawText(text, pos - new Vector2D(102, -32));

                        text = CreateAttributeText(charBaseAttr["+# to Dexterity"].ToString(CultureInfo.InvariantCulture), Brushes.MediumSeaGreen);
                        dc.DrawText(text, pos - new Vector2D(-69, -32));

                    }
                }
            }
        }

        private void DrawAscendancyClasses()
        {
            using (var dc = _ascClassFaces.RenderOpen())
            {
                if (!DrawAscendancy) return;
                var ascName = AscendancyClassName;
                foreach (var node in AscRootNodeList)
                {
                    if (!_persistentData.Options.ShowAllAscendancyClasses && node.AscendancyName != ascName) continue;
                    var imageName = "Classes" + node.AscendancyName;
                    var bitmap = Assets[imageName];
                    var brush = new ImageBrush(Assets[imageName]);
                    var pos = node.Position;
                    dc.DrawRectangle(brush, null,
                        new Rect(
                            pos -
                            new Vector2D(bitmap.PixelWidth * 1.25, bitmap.PixelHeight * 1.25),
                            new Size(bitmap.PixelWidth * 2.5, bitmap.PixelHeight * 2.5)));
                    var currentClass = AscendancyClasses.GetClass(node.AscendancyName!);
                    if (currentClass == null) continue;
                    var textBrush = new SolidColorBrush(Color.FromRgb(
                        (byte)currentClass.FlavourTextColour[0],
                        (byte)currentClass.FlavourTextColour[1],
                        (byte)currentClass.FlavourTextColour[2]));
                    var text =
                        new FormattedText(
                            currentClass.FlavourText,
                            new CultureInfo("en-us"), FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Regular,
                                new FontStretch()),
                            42, textBrush, VisualTreeHelper.GetDpi(SkillTreeVisual).PixelsPerDip);
                    var textPos =
                        new Point(
                            (pos.X - (bitmap.PixelWidth * 1.25)) + currentClass.FlavourTextRect.X,
                            (pos.Y - (bitmap.PixelHeight * 1.25)) + currentClass.FlavourTextRect.Y);
                    text.TextAlignment = TextAlignment.Left;
                    dc.DrawText(text, textPos);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">"Normal", "Highlight", and "Pressed"</param>
        public void DrawAscendancyButton(string type = "Normal")
        {
            using (var dc = _ascButtons.RenderOpen())
            {
                if (AscType == 0 || _persistentData.Options.ShowAllAscendancyClasses) return;
                foreach (var i in RootNodeList)
                {
                    if (!SkilledNodes.Contains(Skillnodes[i]))
                        continue;
                    var node = Skillnodes[i];
                    string imageName;
                    switch (type)
                    {
                        case "Highlight":
                            imageName = "PassiveSkillScreenAscendancyButtonHighlight";
                            break;
                        case "Pressed":
                            imageName = "PassiveSkillScreenAscendancyButtonPressed";
                            break;
                        default:
                            imageName = "PassiveSkillScreenAscendancyButton";
                            break;
                    }

                    var b = Assets[imageName];
                    var brush = new ImageBrush(Assets[imageName]);

                    var worldPos = node.Position;
                    const int distanceFromStartNodeCenter = 325;
                    var dirX = 0.0;
                    var dirY = 1.0;
                    var distToCentre = Math.Sqrt(worldPos.X * worldPos.X + worldPos.Y * worldPos.Y);
                    var isCentered = Math.Abs(worldPos.X) < 10.0 && Math.Abs(worldPos.Y) < 10.0;
                    if (!isCentered)
                    {
                        dirX = worldPos.X / distToCentre;
                        dirY = -worldPos.Y / distToCentre;
                    }

                    var ascButtonRot = Math.Atan2(dirX, dirY);
                    var buttonCx = worldPos.X + distanceFromStartNodeCenter * Math.Cos(ascButtonRot + Math.PI / 2);
                    var buttonCy = worldPos.Y + distanceFromStartNodeCenter * Math.Sin(ascButtonRot + Math.PI / 2);
                    var buttonPoint = new Vector2D(buttonCx, buttonCy);

                    var rect = new Rect(buttonCx - b.Height * 1.75, buttonCy - b.Width * 1.75, b.Width * 2.5, b.Height * 2.5);
                    dc.PushTransform(new RotateTransform(ascButtonRot * (180 / Math.PI), buttonCx, buttonCy));
                    dc.DrawRectangle(brush, null, rect);

                    AscButtonPosition = buttonPoint;
                }
            }
        }

        private FormattedText CreateAttributeText(string text, SolidColorBrush colorBrush)
        {
            return new FormattedText(text,
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal,
                    new FontStretch()),
                30, colorBrush, VisualTreeHelper.GetDpi(SkillTreeVisual).PixelsPerDip);
        }

        public void DrawHighlights()
        {
            var nh = _nodeHighlighter;
            var crossPen = new Pen(Brushes.Red, 20);
            var checkPen = new Pen(Brushes.Lime, 20);
            using (var dc = _highlights.RenderOpen())
            {
                foreach (var pair in nh.NodeHighlights)
                {
                    if (pair.Key.IsAscendancyNode && !DrawAscendancy || pair.Key.Character != null)
                        continue;
                    // TODO: Make more elegant? Needs profiling.
                    var hs = pair.Value;

                    // These should not appear together, so not checking for their conjunction.
                    if (hs != HighlightState.Crossed && hs != HighlightState.Checked)
                    {
                        Pen hpen;

                        // If it has FromHover, don't mix it with the other highlights.
                        if (hs.HasFlag(HighlightState.FromHover))
                        {
                            var brushColor = (Brush)new BrushConverter().ConvertFromString(_persistentData.Options.NodeHoverHighlightColor);
                            hpen = new Pen(brushColor, 20);
                        }
                        else
                        {
                            var red = 0;
                            var green = 0;
                            var blue = 0;
                            var attrHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeAttrHighlightColor);
                            var searchHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeSearchHighlightColor);

                            if (hs.HasFlag(HighlightState.FromAttrib))
                            {
                                red = (red | attrHighlight.R);
                                green = (green | attrHighlight.G);
                                blue = (blue | attrHighlight.B);
                            }
                            if (hs.HasFlag(HighlightState.FromSearch))
                            {
                                red = (red | searchHighlight.R);
                                green = (green | searchHighlight.G);
                                blue = (blue | searchHighlight.B);
                            }
                            hpen = new Pen(new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue)), 20);
                        }

                        dc.DrawEllipse(null, hpen, pair.Key.Position, 80, 80);
                    }

                    var x = pair.Key.Position.X;
                    var y = pair.Key.Position.Y;

                    if (hs.HasFlag(HighlightState.Checked))
                    {
                        // Checked nodes get highlighted with two green lines resembling a check mark.
                        // TODO a better looking check mark
                        dc.DrawLine(checkPen, new Point(x - 8, y + 49), new Point(x - 50, y + 20));
                        dc.DrawLine(checkPen, new Point(x + 50, y - 50), new Point(x - 22, y + 52));
                    }

                    if (hs.HasFlag(HighlightState.Crossed))
                    {
                        // Crossed nodes get highlighted with two crossing red lines.
                        dc.DrawLine(crossPen, new Point(x + 50, y + 70), new Point(x - 50, y - 70));
                        dc.DrawLine(crossPen, new Point(x + 50, y - 70), new Point(x - 50, y + 70));
                    }
                }
            }
        }

        private void DrawActivePaths(bool onlyAscendancy = false)
        {
            DrawingContext? dc = null;
            var adc = _ascActivePaths.RenderOpen();
            if (!onlyAscendancy)
                dc = ActivePaths.RenderOpen();
            var seen = new HashSet<SkillNode>();
            var ascendancyClassName = AscendancyClassName;
            foreach (var n1 in SkilledNodes)
            {
                seen.Add(n1);
                foreach (var n2 in n1.VisibleNeighbors)
                {
                    if (!SkilledNodes.Contains(n2) || seen.Contains(n2)) continue;
                    if (n2.IsAscendancyNode)
                    {
                        if (!DrawAscendancy) continue;
                        if (_persistentData.Options.ShowAllAscendancyClasses ||
                            n2.AscendancyName == ascendancyClassName)
                            DrawConnection(adc, _activePathPen, n2, n1);
                    }
                    else
                    {
                        if (onlyAscendancy) continue;
                        DrawConnection(dc!, _activePathPen, n2, n1);
                    }
                }
            }
            adc.Close();
            dc?.Close();
        }

        private void DrawTreeComparisonHighlight()
        {
            var ascendancyClassName = AscendancyClassName;
            var pen2 = new Pen(new SolidColorBrush(TreeComparisonColor), 25 * HighlightFactor);
            _nodeComparisonSurroundDrawer.Draw();
            if (DrawAscendancy)
            {
                _ascendancyNodeComparisonSurroundDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
            }
            using (DrawingContext
                    dcPath = _pathComparisonHighlight.RenderOpen(),
                    adcPath = _ascPathComparisonHighlight.RenderOpen())
            {
                var seen = new HashSet<SkillNode>();
                foreach (var n1 in HighlightedNodes)
                {
                    seen.Add(n1);
                    foreach (var n2 in n1.VisibleNeighbors)
                    {
                        if (!HighlightedNodes.Contains(n2) || seen.Contains(n2)) continue;

                        if (n2.IsAscendancyNode && n1.IsAscendancyNode)
                        {
                            if (!DrawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses || (n1.AscendancyName == ascendancyClassName && n2.AscendancyName == ascendancyClassName))
                                DrawConnection(adcPath, pen2, n2, n1);
                        }
                        else
                            DrawConnection(dcPath, pen2, n2, n1);
                    }
                }
            }
        }

        public void DrawPath(IEnumerable<SkillNode> path)
        {
            using (DrawingContext
                    dc = _pathOverlay.RenderOpen(),
                    dcAsc = _ascPathOverlay.RenderOpen())
            {
                // Draw a connection from a skilled node to the first path node.
                var skilledNeighbors = new List<SkillNode>();
                var ascendancyClassName = AscendancyClassName;

                var pathNodes = path as IList<SkillNode> ?? path.ToList();
                if (pathNodes.Any())
                    skilledNeighbors = pathNodes.First().VisibleNeighbors.Where(sn => SkilledNodes.Contains(sn)).ToList();
                // The node might not be connected to a skilled node through visible neighbors
                // in which case we don't want to draw a connection anyway.
                if (skilledNeighbors.Any())
                {
                    if (pathNodes.First() != null && skilledNeighbors.First().IsAscendancyNode)
                    {
                        if (DrawAscendancy)
                        {
                            if (_persistentData.Options.ShowAllAscendancyClasses || (pathNodes.First().AscendancyName == ascendancyClassName && skilledNeighbors.First().AscendancyName == ascendancyClassName))
                                DrawConnection(dcAsc, _skillOverlayPen, skilledNeighbors.First(), pathNodes.First());
                        }
                    }
                    else
                        DrawConnection(dc, _skillOverlayPen, skilledNeighbors.First(), pathNodes.First());
                }

                // Draw connections for the path itself (only those that should be visible).
                for (var i = 0; i < pathNodes.Count - 1; i++)
                {
                    var n1 = pathNodes.ElementAt(i);
                    var n2 = pathNodes.ElementAt(i + 1);
                    if (!n1.VisibleNeighbors.Contains(n2)) continue;

                    if (n1.IsAscendancyNode && n2.IsAscendancyNode)
                    {
                        if (!DrawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || (n1.AscendancyName == ascendancyClassName && n2.AscendancyName == ascendancyClassName))
                            DrawConnection(dcAsc, _skillOverlayPen, n1, n2);
                    }
                    else
                        DrawConnection(dc, _skillOverlayPen, n1, n2);
                }
            }
        }

        public void DrawRefundPreview(IEnumerable<SkillNode> nodes)
        {
            using (DrawingContext
                    dc = _pathOverlay.RenderOpen(),
                    dcAsc = _ascPathOverlay.RenderOpen())
            {
                var ascendancyClassName = AscendancyClassName;
                var skillNodes = nodes as IList<SkillNode> ?? nodes.ToList();
                foreach (var node in skillNodes)
                {
                    foreach (var n2 in node.VisibleNeighbors)
                    {
                        if (!SkilledNodes.Contains(n2) || (node.Id >= n2.Id && skillNodes.Contains(n2))) continue;
                        if (node.IsAscendancyNode && n2.IsAscendancyNode)
                        {
                            if (!DrawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses ||
                                (node.AscendancyName == ascendancyClassName &&
                                    n2.AscendancyName == ascendancyClassName))
                                DrawConnection(dcAsc, _refundOverlayPen, node, n2);
                        }
                        else
                            DrawConnection(dc, _refundOverlayPen, node, n2);
                    }
                }
            }
        }

        private void InitializeFaceBrushes()
        {
            if (_initialized) return;
            foreach (var faceName in CharacterFaceNames)
            {
                var bi = BitmapImageFactory.Create(_assetsFolderPath + faceName + ".png");
                _faceBrushes.Add((new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                    new ImageBrush(bi)));
            }

            var bi2 = BitmapImageFactory.Create(_assetsFolderPath + "PSStartNodeBackgroundInactive.png");
            if (_startBackgrounds.ContainsKey(false))
            {
                if (!_startBackgrounds[false].rect.Equals(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight)))
                {
                    _startBackgrounds.Add(false,
                        ((new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                            new ImageBrush(bi2))));
                }
            }
            else
            {
                _startBackgrounds.Add(false,
                    ((new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                        new ImageBrush(bi2))));
            }
        }

        /// <summary>
        /// Only draws what is needed for Ascendancy show/hide
        /// </summary>
        private void DrawAscendancyLayers()
        {
            DrawAscendancyButton();
            if (DrawAscendancy)
            {
                UpdateAscendancyClassPositions();
                DrawAscendancyClasses();
                DrawActiveSkillIconsAndSurrounds(true);
                DrawActivePaths(true);
                DrawSkillIconsAndSurrounds(true);
                DrawInitialPaths(true);
            }
            else
            {
                _ascClassFaces.RenderOpen().Close();
                _ascPathComparisonHighlight.RenderOpen().Close();
                _ascendancyNodeComparisonSurroundDrawer.Clear();
                _ascPaths.RenderOpen().Close();
                _ascActivePaths.RenderOpen().Close();
                _ascPathOverlay.RenderOpen().Close();
                _ascendancyNodeIconDrawer.Clear();
                _activeAscendancyNodeIconDrawer.Clear();
                _ascendancyNodeSurroundDrawer.Clear();
                _activeAscendancyNodeSurroundDrawer.Clear();
            }
        }

        public static void ClearAssets()
        {
            _initialized = false;
        }

        public void DrawJewelHighlight(SkillNode node, Item? socketedJewel)
        {
            _jewelRadiusDrawer.DrawHighlight(node, socketedJewel);
        }
    }
}