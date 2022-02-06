using EnumsNET;
using MoreLinq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Model;
using PoESkillTree.Model.Items;
using PoESkillTree.TreeDrawing;
using PoESkillTree.Utils.Wpf;
using PoESkillTree.ViewModels.Equipment;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HighlightState = PoESkillTree.SkillTreeFiles.NodeHighlighter.HighlightState;

namespace PoESkillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members
        private static readonly Color TreeComparisonColor = Colors.RoyalBlue;
        private readonly Pen _basePathPen = new Pen(Brushes.DarkSlateGray, 10f);
        private readonly Pen _activePathPen = new Pen(Brushes.DarkKhaki, 7.5f);
        private readonly Pen _skillOverlayPen = new Pen(Brushes.LawnGreen, 7.5f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private readonly Pen _refundOverlayPen = new Pen(Brushes.Red, 7.5f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private readonly Pen _textShadowPen = new Pen(Brushes.Black, 2) { LineJoin = PenLineJoin.Round, MiterLimit = 10 };
        private const float HighlightFactor = 1.75f;
        private const float DistanceFromStartNodeCenter = 270f;

        private readonly List<(Rect rect, ImageBrush brush)> _faceBrushes = new List<(Rect, ImageBrush)>();
        private readonly List<(Size size, ImageBrush brush)> _nodeSurroundBrushes = new List<(Size, ImageBrush)>();
        private readonly List<(Size size, ImageBrush brush)> _nodeSurroundComparisonBrushes = new List<(Size, ImageBrush)>();
        private readonly Dictionary<bool, (Rect rect, ImageBrush brush)> _startBackgrounds = new Dictionary<bool, (Rect, ImageBrush)>();

        private readonly NodeHighlighter _nodeHighlighter = new NodeHighlighter();
        private readonly IPersistentData _persistentData;
        public bool DrawAscendancy;

        public DrawingVisual SkillTreeVisual { get; private set; }
        private DrawingVisual _background;
        private AllNodeEffectDrawer _nodeEffectDrawer;
        private NonAscendancyNodeSurroundDrawer _nodeComparisonSurroundDrawer;
        private DrawingVisual _pathComparisonHighlight;
        private DrawingVisual _paths;
        public DrawingVisual ActivePaths { get; private set; }
        private DrawingVisual _pathOverlay;
        private NonAscendancyNodeIconDrawer _nodeIconDrawer;
        private MasteryNodeConnectedDrawer _masteryNodeConnectedDrawer;
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

            SkilledNodes.CollectionChanged += SkilledNodes_CollectionChanged_Draw;
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

        private void SkilledNodes_CollectionChanged_Draw(object sender, EventArgs e)
        {
            DrawActiveNodeEffects();
            DrawActiveSkillIconsAndSurrounds();
            DrawActivePaths();
            DrawCharacterFaces();
            _jewelRadiusDrawer.DrawSkilledNodes();
            _jewelDrawer.Draw();
        }

        private void ItemAllocatedNodesOnCollectionChanged(object sender, CollectionChangedEventArgs<PassiveNodeViewModel> args)
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
            _masteryNodeConnectedDrawer = new MasteryNodeConnectedDrawer(IconActiveSkills, SkilledNodes, Assets.Where(x => x.Key.Contains("Connected")).ToDictionary());
            _activeNodeIconDrawer = new NonAscendancyNodeIconDrawer(IconActiveSkills, SkilledNodes);
            _itemAllocatedNodeIconDrawer = new NonAscendancyNodeIconDrawer(IconActiveSkills, _itemAllocatedNodes);
            _ascendancyNodeIconDrawer = new AscendancyNodeIconDrawer(IconInActiveSkills, Skillnodes.Values);
            _activeAscendancyNodeIconDrawer = new AscendancyNodeIconDrawer(IconActiveSkills, SkilledNodes);

            _nodeEffectDrawer = new AllNodeEffectDrawer(IconActiveSkills, SkilledNodes);

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
            _jewelRadiusDrawer = new JewelRadiusDrawer(Skillnodes, SkilledNodes, n => GetNodeSurroundBrushSize(n, 0));
        }

        private void CreateCombineVisuals()
        {
            // Top most add will be the bottom most element drawn
            SkillTreeVisual.Children.Add(_background);
            SkillTreeVisual.Children.Add(_nodeEffectDrawer.Visual);
            SkillTreeVisual.Children.Add(_nodeComparisonSurroundDrawer.Visual);
            SkillTreeVisual.Children.Add(_pathComparisonHighlight);
            SkillTreeVisual.Children.Add(_paths);
            SkillTreeVisual.Children.Add(ActivePaths);
            SkillTreeVisual.Children.Add(_pathOverlay);
            SkillTreeVisual.Children.Add(_nodeIconDrawer.Visual);
            SkillTreeVisual.Children.Add(_masteryNodeConnectedDrawer.Visual);
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
                if (!Assets.ContainsKey(NodeBackgroundsActive[background.Key])) continue;

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

        private Size GetNodeSurroundBrushSize(PassiveNodeViewModel node, int offset) =>
            _nodeSurroundBrushes[GetNodeSurroundBrushOffset(node) + offset].size;

        private Brush GetNodeSurroundBrush(PassiveNodeViewModel node, int offset) =>
            _nodeSurroundBrushes[GetNodeSurroundBrushOffset(node) + offset].brush;

        private Size GetComparisonNodeSurroundBrushSize(PassiveNodeViewModel node) =>
            _nodeSurroundComparisonBrushes[GetNodeSurroundBrushOffset(node)].size;

        private Brush GetComparisonNodeSurroundBrush(PassiveNodeViewModel node) =>
            _nodeSurroundComparisonBrushes[GetNodeSurroundBrushOffset(node)].brush;

        private static int GetNodeSurroundBrushOffset(PassiveNodeViewModel node) =>
            node.PassiveNodeType switch
            {
                _ when node.IsAscendancyStart => 12,
                PassiveNodeType.Small when node.IsAscendancyNode => 8,
                PassiveNodeType.Small => 0,
                PassiveNodeType.Notable when node.IsAscendancyNode => 10,
                PassiveNodeType.Notable when node.IsBlighted => 14,
                PassiveNodeType.Notable => 2,
                PassiveNodeType.Keystone => 4,
                PassiveNodeType.ExpansionJewelSocket => 16,
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
            _masteryNodeConnectedDrawer.Draw();

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
                var ascStartNode = AscRootNodeList.First(x => x.AscendancyName == AscendancyClassName);
                PoESkillTree.RepositionAscendancyAt(ascStartNode, GetPositionForSingleAscendancy(ascStartNode));
            }
            else
            {
                PoESkillTree.FixAscendancyPassiveNodeGroups();
            }
        }

        private Vector2D GetPositionForSingleAscendancy(PassiveNodeViewModel ascStartNode)
        {

            var bitmap = Assets[$"Classes{ascStartNode.AscendancyName}"];
            var node = Skillnodes[RootNodeClassDictionary[CharClass]];
            var (position, _) = GetAscendancyButtonPosition(node, new Vector2D(DistanceFromStartNodeCenter + bitmap.Width * 1.25, DistanceFromStartNodeCenter + bitmap.Height * 1.25));
            return new Vector2D(position.X, position.Y);
        }

        private void DrawBackgroundLayer()
        {
            if (_initialized) return;
            using (var dc = _background.RenderOpen())
            {
                //These are the images around the groups of nodes 
                var backgrounds = new List<string>
                {
                    "PSGroupBackground1",
                    "PSGroupBackground2",
                    "PSGroupBackground3",
                    "GroupBackgroundSmallAlt",
                    "GroupBackgroundMediumAlt",
                    "GroupBackgroundLargeHalfAlt",
                };
                var groupBackgrounds = new List<BitmapImage>();
                var groupOrbitBrush = new List<ImageBrush>();

                foreach (var background in backgrounds)
                {
                    if (Assets.ContainsKey(background))
                    {
                        groupBackgrounds.Add(Assets[background]);
                        groupOrbitBrush.Add(new ImageBrush(Assets[background]));
                    }
                }

                if (PoESkillTree.LargeGroupUsesHalfImage)
                {
                    groupOrbitBrush[2].TileMode = TileMode.FlipXY;
                    groupOrbitBrush[2].Viewport = new Rect(0, 0, 1, .5f);
                    if (groupOrbitBrush.Count > 3)
                    {
                        groupOrbitBrush[5].TileMode = TileMode.FlipXY;
                        groupOrbitBrush[5].Viewport = new Rect(0, 0, 1, .5f);
                    }
                }
                #region Background Drawing
                if (Assets.ContainsKey("AtlasPassiveBackground"))
                {
                    var backgroundAsset = Assets["AtlasPassiveBackground"];
                    var backgroundBrush = new ImageBrush(backgroundAsset);
                    dc.DrawRectangle(backgroundBrush, null, SkillTreeRect);
                }
                else
                {
                    var backgroundAsset = Assets.ContainsKey("Background1") ? Assets["Background1"] : Assets["Background2"];
                    var backgroundBrush = new ImageBrush(backgroundAsset) { TileMode = TileMode.Tile };
                    backgroundBrush.Viewport = new Rect(0, 0,
                        3 * backgroundBrush.ImageSource.Width / SkillTreeRect.Width,
                        3 * backgroundBrush.ImageSource.Height / SkillTreeRect.Height);
                    dc.DrawRectangle(backgroundBrush, null, SkillTreeRect);
                }
                #endregion

                #region SkillNodeGroup Background Drawing
                foreach (var skillNodeGroup in PoESkillTree.PassiveNodeGroups.Values)
                {
                    if (skillNodeGroup.PassiveNodes.Values.Where(n => n.IsAscendancyNode).ToArray().Length > 0)
                        continue;
                    if (skillNodeGroup.BackgroundOverride == 4)
                        continue;
                    
                    var cgrp = skillNodeGroup.OccupiedOrbits.Where(ng => ng <= 3) ?? Enumerable.Empty<ushort>();
                    var enumerable = cgrp as IList<ushort> ?? cgrp.ToList();
                    if (!enumerable.Any()) continue;
                    var maxr = enumerable.Max(ng => ng);
                    if (maxr == 0) continue;
                    var groupType = maxr > 3 ? 2 : maxr - 1;
                    var heightFactor = groupType == 2 && PoESkillTree.LargeGroupUsesHalfImage ? 2 : 1;
                    if (skillNodeGroup.IsProxy && groupOrbitBrush.Count > 3)
                    {
                        groupType += 3;
                    }
                    var size = new Size(groupBackgrounds[groupType].PixelWidth, groupBackgrounds[groupType].PixelHeight * heightFactor);
                    var offset = new Vector2D(size.Width, size.Height) / 2;
                    dc.DrawRectangle(groupOrbitBrush[groupType], null, new Rect(skillNodeGroup.Position - offset, size));
                }
                #endregion
            }
        }

        private static void DrawConnection(DrawingContext dc, Pen pen2, PassiveNodeViewModel n1, PassiveNodeViewModel n2)
        {
            if (!n1.VisibleNeighborPassiveNodes.ContainsKey(n2.Id) || !n2.VisibleNeighborPassiveNodes.ContainsKey(n1.Id)) return;
            if (n1.PassiveNodeGroup == n2.PassiveNodeGroup && n1.Orbit == n2.Orbit)
            {
                if (n1.Arc - n2.Arc > 0 && n1.Arc - n2.Arc <= Math.PI ||
                    n1.Arc - n2.Arc < -Math.PI)
                {
                    dc.DrawArc(null, pen2, n1.Position, n2.Position, new Size(n1.OrbitRadii[n1.Orbit] * n1.ZoomLevel, n1.OrbitRadii[n1.Orbit] * n1.ZoomLevel));
                }
                else
                {
                    dc.DrawArc(null, pen2, n2.Position, n1.Position, new Size(n1.OrbitRadii[n1.Orbit] * n1.ZoomLevel, n1.OrbitRadii[n1.Orbit] * n1.ZoomLevel));
                }
            }
            else
            {
                var draw = !n1.IsAscendantClassStartNode;
                if (n1.PassiveNodeType == PassiveNodeType.Mastery || n2.PassiveNodeType == PassiveNodeType.Mastery)
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
            var seen = new HashSet<ushort>();
            foreach (var (id1, n1) in Skillnodes)
            {
                seen.Add(id1);
                foreach (var (id2, n2) in n1.VisibleNeighborPassiveNodes)
                {
                    if (seen.Contains(id2)) continue;
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
                    if (!RootNodeClassDictionary.TryGetValue(charClass, out var id))
                    {
                        continue;
                    }

                    var pos = Skillnodes[id].Position;
                    if (_startBackgrounds.ContainsKey(false))
                    {
                        dc.DrawRectangle(_startBackgrounds[false].brush, null,
                            new Rect(
                                pos - new Vector2D(_startBackgrounds[false].rect.Width, _startBackgrounds[false].rect.Height),
                                pos + new Vector2D(_startBackgrounds[false].rect.Width, _startBackgrounds[false].rect.Height)));
                    }
                    if (CharClass == charClass)
                    {
                        var i = (int)CharClass;
                        var (rect, brush) = _faceBrushes[i];
                        dc.DrawRectangle(brush, null, new Rect(pos - new Vector2D(rect.Width, rect.Height), pos + new Vector2D(rect.Width, rect.Height)));

                        if (!CharBaseAttributes.TryGetValue(charClass, out var charBase)) 
                        {
                            continue;
                        }

                        var charBaseAttr = charBase.ToDictionary();
                        var text = CreateAttributeText(charBaseAttr["+# to Intelligence"].ToString(CultureInfo.InvariantCulture), Brushes.DodgerBlue);
                        var textPos = pos - new Vector2D(10, 47);
                        dc.DrawGeometry(null, _textShadowPen, text.BuildGeometry(textPos));
                        dc.DrawText(text, textPos);

                        text = CreateAttributeText(charBaseAttr["+# to Strength"].ToString(CultureInfo.InvariantCulture), Brushes.IndianRed);
                        textPos = pos - new Vector2D(42, -10);
                        dc.DrawGeometry(null, _textShadowPen, text.BuildGeometry(textPos));
                        dc.DrawText(text, textPos);

                        text = CreateAttributeText(charBaseAttr["+# to Dexterity"].ToString(CultureInfo.InvariantCulture), Brushes.MediumSeaGreen);
                        textPos = pos - new Vector2D(-24, -10);
                        dc.DrawGeometry(null, _textShadowPen, text.BuildGeometry(textPos));
                        dc.DrawText(text, textPos);

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
                    dc.DrawRectangle(brush, null, new Rect(pos - new Vector2D(bitmap.PixelWidth / 2, bitmap.PixelHeight / 2), new Size(bitmap.PixelWidth, bitmap.PixelHeight)));
                    var currentClass = AscendancyClasses.GetClass(node.AscendancyName!);
                    if (currentClass == null) continue;
                    var textBrush = new SolidColorBrush(Color.FromRgb(currentClass.FlavourTextColour.R, currentClass.FlavourTextColour.G, currentClass.FlavourTextColour.B));
                    var text =
                        new FormattedText(
                            currentClass.FlavourText,
                            new CultureInfo("en-us"), FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("serif"), FontStyles.Italic, FontWeights.Regular,
                            new FontStretch()),
                            16, textBrush, VisualTreeHelper.GetDpi(SkillTreeVisual).PixelsPerDip);
                    var textPos =
                        new Point(
                            pos.X - (bitmap.PixelWidth / 2) + currentClass.FlavourTextRect.X * node.ZoomLevel,
                            pos.Y - (bitmap.PixelHeight / 2) + currentClass.FlavourTextRect.Y * node.ZoomLevel);
                    text.TextAlignment = TextAlignment.Left;
                    dc.DrawGeometry(null, _textShadowPen, text.BuildGeometry(textPos));
                    dc.DrawText(text, textPos);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">"Normal", "Highlight", and "Pressed"</param>
        public void DrawAscendancyButton(string type = "")
        {
            using (var dc = _ascButtons.RenderOpen())
            {
                if (AscType == 0 || _persistentData.Options.ShowAllAscendancyClasses) return;
                foreach (var i in RootNodeList)
                {
                    if (!SkilledNodes.Contains(Skillnodes[i]))
                        continue;
                    var node = Skillnodes[i];
                    var imageName = $"AscendancyButton{type}";
                    var b = Assets[imageName];
                    var brush = new ImageBrush(Assets[imageName]);

                    var (position, rotation) = GetAscendancyButtonPosition(node, new Vector2D(DistanceFromStartNodeCenter, DistanceFromStartNodeCenter), node.ZoomLevel);
                    AscendancyButtonRect = new Rect(position.X - (b.PixelWidth / 2), position.Y - (DistanceFromStartNodeCenter - b.PixelHeight) * node.ZoomLevel / 2, b.PixelWidth, b.PixelHeight);
                    dc.PushTransform(new RotateTransform(rotation * (180 / Math.PI), position.X, position.Y));
                    dc.DrawRectangle(brush, null, AscendancyButtonRect);
                }
            }
        }

        private (Vector2D position, double rotation) GetAscendancyButtonPosition(PassiveNodeViewModel node, Vector2D offset, double scale = 1f)
        {
            var position = node.Position / node.ZoomLevel;
            var dirX = 0.0;
            var dirY = 1.0;
            var isCentered = Math.Abs(position.X) < 10.0 && Math.Abs(position.Y) < 10.0;
            if (!isCentered)
            {
                var distToCentre = Math.Sqrt(position.X * position.X + position.Y * position.Y);
                dirX = position.X / distToCentre;
                dirY = -position.Y / distToCentre;
            }

            var rotation = Math.Atan2(dirX, dirY);
            var x = (position.X + offset.X * Math.Cos(rotation + Math.PI / 2)) * scale;
            var y = (position.Y + offset.Y * Math.Sin(rotation + Math.PI / 2)) * scale;

            return (new Vector2D(x, y), rotation);
        }

        private FormattedText CreateAttributeText(string text, SolidColorBrush colorBrush)
        {
            return new FormattedText(text,
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal,
                new FontStretch()),
                16, colorBrush, VisualTreeHelper.GetDpi(SkillTreeVisual).PixelsPerDip);
        }

        public void DrawHighlights()
        {
            var nh = _nodeHighlighter;
            var crossPen = new Pen(Brushes.Red, 5);
            var checkPen = new Pen(Brushes.Lime, 5);
            using (var dc = _highlights.RenderOpen())
            {
                foreach (var (node, state) in nh.NodeHighlights)
                {
                    if (node.IsAscendancyNode && !DrawAscendancy || node.StartingCharacterClass != null)
                        continue;
                    // TODO: Make more elegant? Needs profiling.

                    // These should not appear together, so not checking for their conjunction.
                    if (state != HighlightState.Crossed && state != HighlightState.Checked)
                    {
                        Pen hpen;

                        // If it has FromHover, don't mix it with the other highlights.
                        if (state.HasFlag(HighlightState.FromHover))
                        {
                            var brushColor = (Brush)new BrushConverter().ConvertFromString(_persistentData.Options.NodeHoverHighlightColor);
                            hpen = new Pen(brushColor, 5);
                        }
                        else
                        {
                            var red = 0;
                            var green = 0;
                            var blue = 0;
                            var attrHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeAttrHighlightColor);
                            var searchHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeSearchHighlightColor);

                            if (state.HasFlag(HighlightState.FromAttrib))
                            {
                                red = (red | attrHighlight.R);
                                green = (green | attrHighlight.G);
                                blue = (blue | attrHighlight.B);
                            }
                            if (state.HasFlag(HighlightState.FromSearch))
                            {
                                red = (red | searchHighlight.R);
                                green = (green | searchHighlight.G);
                                blue = (blue | searchHighlight.B);
                            }
                            hpen = new Pen(new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue)), 5);
                        }

                        var size = GetNodeSurroundBrushSize(node, 0);
                        var radius = Math.Sqrt((size.Width * size.Height) / Math.PI) + 5;
                        dc.DrawEllipse(null, hpen, node.Position, radius, radius);
                    }

                    var x = node.Position.X;
                    var y = node.Position.Y;

                    if (state.HasFlag(HighlightState.Checked))
                    {
                        // Checked nodes get highlighted with two green lines resembling a check mark.
                        // TODO a better looking check mark
                        dc.DrawLine(checkPen, new Point(x - 4, y + 21), new Point(x - 21.5, y + 8));
                        dc.DrawLine(checkPen, new Point(x + 21.5, y - 18), new Point(x - 7, y + 21.5));
                    }

                    if (state.HasFlag(HighlightState.Crossed))
                    {
                        // Crossed nodes get highlighted with two crossing red lines.
                        dc.DrawLine(crossPen, new Point(x + 18, y + 24), new Point(x - 18, y - 24));
                        dc.DrawLine(crossPen, new Point(x + 18, y - 24), new Point(x - 18, y + 24));
                    }
                }
            }
        }

        private void DrawActiveNodeEffects()
        {
            _nodeEffectDrawer.Draw();
        }

        private void DrawActivePaths(bool onlyAscendancy = false)
        {
            DrawingContext? dc = null;
            var adc = _ascActivePaths.RenderOpen();
            if (!onlyAscendancy)
                dc = ActivePaths.RenderOpen();
            var seen = new HashSet<PassiveNodeViewModel>();
            var ascendancyClassName = AscendancyClassName;
            foreach (var n1 in SkilledNodes)
            {
                seen.Add(n1);
                foreach (var n2 in n1.VisibleNeighborPassiveNodes.Values)
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
            var pen2 = new Pen(new SolidColorBrush(TreeComparisonColor), _basePathPen.Thickness * HighlightFactor);
            _nodeComparisonSurroundDrawer.Draw();
            if (DrawAscendancy)
            {
                _ascendancyNodeComparisonSurroundDrawer.Draw(_persistentData.Options.ShowAllAscendancyClasses, AscendancyClassName);
            }
            using (DrawingContext
                    dcPath = _pathComparisonHighlight.RenderOpen(),
                    adcPath = _ascPathComparisonHighlight.RenderOpen())
            {
                var seen = new HashSet<PassiveNodeViewModel>();
                foreach (var n1 in HighlightedNodes)
                {
                    seen.Add(n1);
                    foreach (var n2 in n1.VisibleNeighborPassiveNodes.Values)
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

        public void DrawPath(IEnumerable<PassiveNodeViewModel> path)
        {
            using (DrawingContext
                    dc = _pathOverlay.RenderOpen(),
                    dcAsc = _ascPathOverlay.RenderOpen())
            {
                // Draw a connection from a skilled node to the first path node.
                var skilledNeighbors = new List<PassiveNodeViewModel>();
                var ascendancyClassName = AscendancyClassName;

                var pathNodes = path as IList<PassiveNodeViewModel> ?? path.ToList();
                if (pathNodes.Any())
                    skilledNeighbors = pathNodes.First().VisibleNeighborPassiveNodes.Values.Where(sn => SkilledNodes.Contains(sn)).ToList();
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
                    if (!n1.VisibleNeighborPassiveNodes.Values.Contains(n2)) continue;

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

        public void DrawRefundPreview(IEnumerable<PassiveNodeViewModel> nodes)
        {
            using (DrawingContext
                    dc = _pathOverlay.RenderOpen(),
                    dcAsc = _ascPathOverlay.RenderOpen())
            {
                var ascendancyClassName = AscendancyClassName;
                var skillNodes = nodes as IList<PassiveNodeViewModel> ?? nodes.ToList();
                foreach (var node in skillNodes)
                {
                    foreach (var n2 in node.VisibleNeighborPassiveNodes.Values)
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
                var facePath = Path.Combine(_assetsFolderPath, $"{faceName}.png");
                if (File.Exists(facePath))
                {
                    var bi = BitmapImageFactory.Create(facePath);
                    _faceBrushes.Add((new Rect(0, 0, bi.PixelWidth * PoESkillTree.MaxImageZoomLevel, bi.PixelHeight * PoESkillTree.MaxImageZoomLevel),
                        new ImageBrush(bi)));
                }
            }

            var path = Path.Combine(_assetsFolderPath, "PSStartNodeBackgroundInactive.png");
            if (File.Exists(path))
            {
                var bi2 = BitmapImageFactory.Create(path);
                if (_startBackgrounds.ContainsKey(false))
                {
                    if (!_startBackgrounds[false].rect.Equals(new Rect(0, 0, bi2.PixelWidth * PoESkillTree.MaxImageZoomLevel, bi2.PixelHeight * PoESkillTree.MaxImageZoomLevel)))
                    {
                        _startBackgrounds.Add(false,
                            ((new Rect(0, 0, bi2.PixelWidth * PoESkillTree.MaxImageZoomLevel, bi2.PixelHeight * PoESkillTree.MaxImageZoomLevel),
                                new ImageBrush(bi2))));
                    }
                }
                else
                {
                    _startBackgrounds.Add(false,
                        ((new Rect(0, 0, bi2.PixelWidth * PoESkillTree.MaxImageZoomLevel, bi2.PixelHeight * PoESkillTree.MaxImageZoomLevel),
                            new ImageBrush(bi2))));
                }
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

        public void DrawJewelHighlight(PassiveNodeViewModel node, Item? socketedJewel)
        {
            _jewelRadiusDrawer.DrawHighlight(node, socketedJewel);
        }
    }
}