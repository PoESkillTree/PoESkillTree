using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using POESKillTree.Model;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members
        private static readonly Color TreeComparisonColor = Colors.RoyalBlue;
        private readonly Pen _basePathPen = new Pen(Brushes.DarkSlateGray, 20f);
        private readonly Pen _activePathPen = new Pen(Brushes.DarkKhaki, 15f);
        private readonly Pen _skillOverlayPen = new Pen(Brushes.LawnGreen, 15f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private readonly Pen _refundOverlayPen = new Pen(Brushes.Red, 15f) { DashStyle = new DashStyle(new DoubleCollection { 2 }, 2) };
        private readonly Pen _skillIconPen = new Pen(Brushes.Black, 5);
        private readonly Geometry _skillTreeRectGeometry = new RectangleGeometry(SkillTreeRect);
        private const float HighlightFactor = 1.2f;

        private readonly List<KeyValuePair<Rect, ImageBrush>> _faceBrushes = new List<KeyValuePair<Rect, ImageBrush>>();
        private readonly List<KeyValuePair<Size, ImageBrush>> _nodeSurroundBrushes = new List<KeyValuePair<Size, ImageBrush>>();
        private readonly List<KeyValuePair<Size, ImageBrush>> _nodeSurroundComparisonBrushes = new List<KeyValuePair<Size, ImageBrush>>();
        private readonly Dictionary<bool, KeyValuePair<Rect, ImageBrush>> _startBackgrounds = new Dictionary<bool, KeyValuePair<Rect, ImageBrush>>();

        private readonly NodeHighlighter _nodeHighlighter = new NodeHighlighter();
        private readonly IPersistentData _persistentData;
        private readonly List<Tuple<int, Vector2D>> _originalPositions = new List<Tuple<int, Vector2D>>();
        public bool DrawAscendancy;

        public DrawingVisual SkillTreeVisual;
        private DrawingVisual _background;
        private DrawingVisual _nodeComparisonHighlight;
        private DrawingVisual _pathComparisonHighlight;
        private DrawingVisual _paths;
        public DrawingVisual ActivePaths;
        private DrawingVisual _pathOverlay;
        private DrawingVisual _skillIcons;
        private DrawingVisual _activeSkillIcons;
        private DrawingVisual _nodeSurround;
        private DrawingVisual _activeNodeSurround;
        private DrawingVisual _characterFaces;
        private DrawingVisual _highlights;
        private DrawingVisual _jewelHighlight;

        private DrawingVisual _ascSkillTreeVisual;
        private DrawingVisual _ascClassFaces;
        private DrawingVisual _ascButtons;
        private DrawingVisual _ascNodeComparisonHighlight;
        private DrawingVisual _ascPathComparisonHighlight;
        private DrawingVisual _ascPaths;
        private DrawingVisual _ascActivePaths;
        private DrawingVisual _ascPathOverlay;
        private DrawingVisual _ascSkillIcons;
        private DrawingVisual _ascActiveSkillIcons;
        private DrawingVisual _ascNodeSurround;
        private DrawingVisual _ascActiveNodeSurround;
        #endregion
        public void InitialSkillTreeDrawing()
        {

            SkilledNodes.CollectionChanged += SkilledNodes_CollectionChanged;
            HighlightedNodes.CollectionChanged += HighlightedNodes_CollectionChanged;
            if (_initialized) return;
            InitializeDrawingVisuals();
            InitializeNodeSurroundBrushes();
            InitializeFaceBrushes();
            //Drawing
            DrawBackgroundLayer();
            DrawInitialPaths(Links);
            DrawSkillIconsAndSurrounds();
            DrawCharacterFaces();
            DrawAscendancyClasses();
            DrawAscendancyButton();

            //Add all the drawings on one layer
            CreateCombineVisuals();
        }

        private void HighlightedNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DrawTreeComparisonHighlight();
        }

        private void SkilledNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null && e.OldItems == null) return;
            DrawActiveSkillIconsAndSurrounds();
            DrawActivePaths();
            DrawCharacterFaces();
        }
        /// <summary>
        /// This will initialize all drawing visuals. If a new drawing visual is added then it should be initialized here as well.
        /// </summary>
        private void InitializeDrawingVisuals()
        {
            SkillTreeVisual = new DrawingVisual();
            _background = new DrawingVisual();
            _nodeComparisonHighlight = new DrawingVisual();
            _pathComparisonHighlight = new DrawingVisual();
            _paths = new DrawingVisual();
            ActivePaths = new DrawingVisual();
            _pathOverlay = new DrawingVisual();
            _skillIcons = new DrawingVisual();
            _activeSkillIcons = new DrawingVisual();
            _nodeSurround = new DrawingVisual();
            _activeNodeSurround = new DrawingVisual();
            _characterFaces = new DrawingVisual();
            _highlights = new DrawingVisual();
            _jewelHighlight = new DrawingVisual();

            _ascSkillTreeVisual = new DrawingVisual();
            _ascClassFaces = new DrawingVisual();
            _ascButtons = new DrawingVisual();
            _ascNodeComparisonHighlight = new DrawingVisual();
            _ascPathComparisonHighlight = new DrawingVisual();
            _ascPaths = new DrawingVisual();
            _ascActivePaths = new DrawingVisual();
            _ascPathOverlay = new DrawingVisual();
            _ascSkillIcons = new DrawingVisual();
            _ascActiveSkillIcons = new DrawingVisual();
            _ascNodeSurround = new DrawingVisual();
            _ascActiveNodeSurround = new DrawingVisual();
        }

        private void CreateCombineVisuals()
        {
            //Top most add will be the bottom most element drawn
            SkillTreeVisual.Children.Add(_background);
            SkillTreeVisual.Children.Add(_nodeComparisonHighlight);
            SkillTreeVisual.Children.Add(_pathComparisonHighlight);
            SkillTreeVisual.Children.Add(_paths);
            SkillTreeVisual.Children.Add(ActivePaths);
            SkillTreeVisual.Children.Add(_pathOverlay);
            SkillTreeVisual.Children.Add(_skillIcons);
            SkillTreeVisual.Children.Add(_activeSkillIcons);
            SkillTreeVisual.Children.Add(_nodeSurround);
            SkillTreeVisual.Children.Add(_activeNodeSurround);
            SkillTreeVisual.Children.Add(_characterFaces);

            _ascSkillTreeVisual.Children.Add(_ascClassFaces);
            _ascSkillTreeVisual.Children.Add(_ascNodeComparisonHighlight);
            _ascSkillTreeVisual.Children.Add(_ascPathComparisonHighlight);
            _ascSkillTreeVisual.Children.Add(_ascPaths);
            _ascSkillTreeVisual.Children.Add(_ascActivePaths);
            _ascSkillTreeVisual.Children.Add(_ascPathOverlay);
            _ascSkillTreeVisual.Children.Add(_ascSkillIcons);
            _ascSkillTreeVisual.Children.Add(_ascActiveSkillIcons);
            _ascSkillTreeVisual.Children.Add(_ascNodeSurround);
            _ascSkillTreeVisual.Children.Add(_ascActiveNodeSurround);

            SkillTreeVisual.Children.Add(_ascSkillTreeVisual);
            SkillTreeVisual.Children.Add(_ascButtons);
            SkillTreeVisual.Children.Add(_highlights);
            SkillTreeVisual.Children.Add(_jewelHighlight);
        }

        private void InitializeNodeSurroundBrushes()
        {
            if (_initialized) return;
            foreach (var background in NodeBackgrounds)
            {
                if (!NodeBackgroundsActive.ContainsKey(background.Key)) continue;
                var normalBrushPImage = _assets[NodeBackgrounds[background.Key]];
                var normalBrush = new ImageBrush
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = normalBrushPImage
                };
                var normalSize = new Size(normalBrushPImage.PixelWidth, normalBrushPImage.PixelHeight);

                var activeBrushPImage = _assets[NodeBackgroundsActive[background.Key]];
                var activeBrush = new ImageBrush
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = activeBrushPImage
                };
                var activeSize = new Size(activeBrushPImage.PixelWidth, activeBrushPImage.PixelHeight);

                _nodeSurroundBrushes.Add(new KeyValuePair<Size, ImageBrush>(normalSize, normalBrush));
                _nodeSurroundBrushes.Add(new KeyValuePair<Size, ImageBrush>(activeSize, activeBrush));

                //tree comparison highlight generator
                var outlinecolor = TreeComparisonColor;
                var omask = outlinecolor.B | (uint)outlinecolor.G << 8 | (uint)outlinecolor.R << 16;

                var bitmap = (BitmapImage)normalBrush.ImageSource;
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
                    _nodeSurroundComparisonBrushes.Add(new KeyValuePair<Size, ImageBrush>(normalSize, ibr));
                    _nodeSurroundComparisonBrushes.Add(new KeyValuePair<Size, ImageBrush>(normalSize, ibr));
                }
                else
                {
                    throw new Exception("Highlight Generator did not generate with the correct byte order");
                }
            }
        }
        private void DrawSkillNodeIcon(DrawingContext dc, SkillNode skillNode, bool onlyAscendancy = false, bool isActive = false)
        {
            if (onlyAscendancy && skillNode.ascendancyName == null) return;

            Rect rect;
            BitmapImage bitmapImage;

            if (isActive)
            {
                rect = IconActiveSkills.SkillPositions[skillNode.IconKey];
                bitmapImage = IconActiveSkills.GetSkillImage(skillNode.IconKey);
            }
            else
            {
                rect = IconInActiveSkills.SkillPositions[skillNode.IconKey];
                bitmapImage = IconInActiveSkills.GetSkillImage(skillNode.IconKey);
            }

            var imageBrush = new ImageBrush()
            {
                Stretch = Stretch.Uniform,
                ImageSource = bitmapImage,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight,
                       rect.Width / bitmapImage.PixelWidth, rect.Height / bitmapImage.PixelHeight)
            };

            dc.DrawEllipse(imageBrush, null, skillNode.Position, rect.Width, rect.Height);
        }

        private void DrawSurround(DrawingContext dc, SkillNode node, bool onlyAscendancy = false, bool isActive = false, bool isHighlight = false)
        {
            if (onlyAscendancy && node.ascendancyName == null) return;
            var surroundBrush = _nodeSurroundBrushes;
            var factor = 1f;
            var activeOffset = 0;
            if (isActive)
                activeOffset = 1;
            if (isHighlight)
            {
                surroundBrush = _nodeSurroundComparisonBrushes;
                factor = HighlightFactor;
            }
            var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);

            if (node.IsAscendancyStart)
            {
                if (!DrawAscendancy || isHighlight || isActive) return;

                const string ascStartName = "PassiveSkillScreenAscendancyMiddle";
                var bitmap = Assets[ascStartName];
                var brush = new ImageBrush(Assets[ascStartName]);
                if (_persistentData.Options.ShowAllAscendancyClasses || node.ascendancyName == ascendancyClassName)
                    dc.DrawRectangle(brush, null,
                        new Rect(node.Position - new Vector2D(bitmap.PixelWidth, bitmap.PixelHeight),
                                new Size(bitmap.PixelWidth * 2, bitmap.PixelHeight * 2)));
            }
            else if (node.ascendancyName != null && node.Type == NodeType.Notable)
            {
                if (!DrawAscendancy) return;

                if (_persistentData.Options.ShowAllAscendancyClasses || node.ascendancyName == ascendancyClassName)
                    dc.DrawRectangle(surroundBrush[10 + activeOffset].Value, null,
                        new Rect((int)node.Position.X - surroundBrush[10 + activeOffset].Key.Width * .875 * factor,
                            (int)node.Position.Y - surroundBrush[10 + activeOffset].Key.Height * .875 * factor,
                            surroundBrush[10 + activeOffset].Key.Width * 1.75 * factor,
                            surroundBrush[10 + activeOffset].Key.Height * 1.75 * factor));
            }
            else if (node.ascendancyName != null && node.Type == NodeType.Normal)
            {
                if (!DrawAscendancy) return;

                if (_persistentData.Options.ShowAllAscendancyClasses || node.ascendancyName == ascendancyClassName)
                    dc.DrawRectangle(surroundBrush[8 + activeOffset].Value, null,
                        new Rect((int)node.Position.X - surroundBrush[8 + activeOffset].Key.Width * factor,
                            (int)node.Position.Y - surroundBrush[8 + activeOffset].Key.Height * factor,
                            surroundBrush[8 + activeOffset].Key.Width * 2 * factor,
                            surroundBrush[8 + activeOffset].Key.Height * 2 * factor));
            }
            else if (node.Type == NodeType.Notable)
            {
                dc.DrawRectangle(surroundBrush[2 + activeOffset].Value, null,
                    new Rect((int)node.Position.X - surroundBrush[2 + activeOffset].Key.Width * factor,
                        (int)node.Position.Y - surroundBrush[2 + activeOffset].Key.Height * factor,
                        surroundBrush[2 + activeOffset].Key.Width * 2 * factor,
                        surroundBrush[2 + activeOffset].Key.Height * 2 * factor));
            }
            else if (node.Type == NodeType.Keystone)
            {
                dc.DrawRectangle(surroundBrush[4 + activeOffset].Value, null,
                    new Rect((int)node.Position.X - surroundBrush[4 + activeOffset].Key.Width * factor,
                        (int)node.Position.Y - surroundBrush[4 + activeOffset].Key.Height * factor,
                        surroundBrush[4 + activeOffset].Key.Width * 2 * factor,
                        surroundBrush[4 + activeOffset].Key.Height * 2 * factor));
            }
            else if (node.Type == NodeType.Mastery)
            {
                //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
            }
            else if (node.Type == NodeType.JewelSocket)
            {
                dc.DrawRectangle(surroundBrush[6 + activeOffset].Value, null,
                    new Rect((int)node.Position.X - surroundBrush[6 + activeOffset].Key.Width * factor,
                        (int)node.Position.Y - surroundBrush[6 + activeOffset].Key.Height * factor,
                        surroundBrush[6 + activeOffset].Key.Width * 2 * factor,
                        surroundBrush[6 + activeOffset].Key.Height * 2 * factor));
            }
            else
            {
                dc.DrawRectangle(surroundBrush[0 + activeOffset].Value, null,
                    new Rect((int)node.Position.X - surroundBrush[0 + activeOffset].Key.Width * factor,
                        (int)node.Position.Y - surroundBrush[0 + activeOffset].Key.Height * factor,
                        surroundBrush[0 + activeOffset].Key.Width * 2 * factor,
                        surroundBrush[0 + activeOffset].Key.Height * 2 * factor));
            }
        }

        private void DrawSkillIconsAndSurrounds(bool onlyAscendancy = false)
        {
            DrawingContext dcSkillIcons = null;
            DrawingContext dcSkillSurround = null;
            var ascSkillIcons = _ascSkillIcons.RenderOpen();
            var adcAsctiveSkillSurround = _ascNodeSurround.RenderOpen();

            if (!onlyAscendancy)
            {
                dcSkillIcons = _skillIcons.RenderOpen();
                dcSkillSurround = _nodeSurround.RenderOpen();
                dcSkillIcons.DrawGeometry(null, _skillIconPen, _skillTreeRectGeometry);
            }
            
            ascSkillIcons.DrawGeometry(null, _skillIconPen, _skillTreeRectGeometry);

            foreach (var n in Skillnodes)
            {
                if (n.Value.ascendancyName != null)
                {
                    if (!DrawAscendancy) continue;
                    var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
                    if ((!_persistentData.Options.ShowAllAscendancyClasses &&
                            n.Value.ascendancyName != ascendancyClassName)) continue;
                    DrawSkillNodeIcon(ascSkillIcons, n.Value, onlyAscendancy);
                    DrawSurround(adcAsctiveSkillSurround, n.Value, onlyAscendancy);
                }
                else
                {
                    if (onlyAscendancy) continue;
                    DrawSkillNodeIcon(dcSkillIcons, n.Value);
                    DrawSurround(dcSkillSurround, n.Value);
                }
            }
            ascSkillIcons.Close();
            adcAsctiveSkillSurround.Close();
            dcSkillIcons?.Close();
            dcSkillSurround?.Close();
        }

        private void DrawActiveSkillIconsAndSurrounds(bool onlyAscendancy = false)
        {
            DrawingContext dcActiveSkillIcons = null;
            DrawingContext dcActiveSkillSurround = null;
            if (!onlyAscendancy)
            {
                dcActiveSkillIcons = _activeSkillIcons.RenderOpen();
                dcActiveSkillSurround = _activeNodeSurround.RenderOpen();
                dcActiveSkillIcons.DrawGeometry(null, _skillIconPen, _skillTreeRectGeometry);
            }
            var ascActiveSkillIcons = _ascActiveSkillIcons.RenderOpen();
            var ascActiveSkillSurround = _ascActiveNodeSurround.RenderOpen();
            ascActiveSkillIcons.DrawGeometry(null, _skillIconPen, _skillTreeRectGeometry);

            foreach (var skillNode in SkilledNodes)
            {
                if (skillNode.ascendancyName != null)
                {
                    if (!DrawAscendancy) continue;
                    var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
                    if ((!_persistentData.Options.ShowAllAscendancyClasses &&
                            skillNode.ascendancyName != ascendancyClassName)) continue;
                    DrawSkillNodeIcon(ascActiveSkillIcons, skillNode, onlyAscendancy, true);
                    DrawSurround(ascActiveSkillSurround, skillNode, onlyAscendancy, true);
                }
                else
                {
                    if(onlyAscendancy) continue;
                    DrawSkillNodeIcon(dcActiveSkillIcons, skillNode, false, true);
                    DrawSurround(dcActiveSkillSurround, skillNode, false, true);
                }
            }
            ascActiveSkillIcons.Close();
            ascActiveSkillSurround.Close();
            dcActiveSkillIcons?.Close();
            dcActiveSkillSurround?.Close();
        }

        public void ClearPath()
        {
            _pathOverlay.RenderOpen().Close();
            _ascPathOverlay.RenderOpen().Close();
        }

        public void ClearJewelHighlight()
        {
            _jewelHighlight.RenderOpen().Close();
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
                var ascName = AscClasses.GetClassName(_chartype, AscType);
                var nodeList = Skillnodes.Where(x => x.Value.ascendancyName == ascName && !x.Value.IsAscendancyStart);
                var worldPos = Skillnodes.First(x => x.Value.Name.ToUpperInvariant() == CharName[_chartype]).Value.Position;
                var ascStartNode = AscRootNodeList.First(x => x.ascendancyName == ascName);
                var ascNodeOriginalPos = ascStartNode.SkillNodeGroup.Position;
                if (_originalPositions.All(x => x.Item1 != ascStartNode.G))
                    _originalPositions.Add(new Tuple<int, Vector2D>(ascStartNode.G, new Vector2D(ascNodeOriginalPos.ToContainingPoint())));

                var imageName = "Classes" + ascStartNode.ascendancyName;
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

                ascStartNode.SkillNodeGroup.Position = new Vector2D(imageCx, imageCy);
                var done = new List<SkillNodeGroup> { ascStartNode.SkillNodeGroup };

                foreach (var n in nodeList)
                {
                    if (done.Contains(n.Value.SkillNodeGroup))
                        continue;
                    if (_originalPositions.All(x => x.Item1 != n.Value.G))
                        _originalPositions.Add(new Tuple<int, Vector2D>(n.Value.G, new Vector2D(n.Value.SkillNodeGroup.Position.ToContainingPoint())));
                    var diffDist = ascNodeOriginalPos - n.Value.SkillNodeGroup.Position;
                    imageCx = ascStartNode.SkillNodeGroup.Position.X - diffDist.X;
                    imageCy = ascStartNode.SkillNodeGroup.Position.Y - diffDist.Y;

                    n.Value.SkillNodeGroup.Position = new Vector2D(imageCx, imageCy);
                    done.Add(n.Value.SkillNodeGroup);
                }
            }
            else
            {
                foreach (var g in _originalPositions)
                {
                    foreach (var n in Skillnodes)
                    {
                        if (g.Item1 != n.Value.G) continue;
                        n.Value.SkillNodeGroup.Position = g.Item2;
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

                foreach (var skillNodeGroup in NodeGroups)
                {
                    if (skillNodeGroup.Nodes.Where(n => n.ascendancyName != null).ToArray().Length > 0)
                        continue;
                    if (skillNodeGroup.OcpOrb == null)
                        skillNodeGroup.OcpOrb = new Dictionary<int, bool>();
                    var cgrp = skillNodeGroup.OcpOrb.Keys.Where(ng => ng <= 3);
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
            if (n1.SkillNodeGroup == n2.SkillNodeGroup && n1.Orbit == n2.Orbit)
            {
                if (n1.Arc - n2.Arc > 0 && n1.Arc - n2.Arc <= Math.PI ||
                    n1.Arc - n2.Arc < -Math.PI)
                {
                    dc.DrawArc(null, pen2, n1.Position, n2.Position,
                        new Size(SkillNode.OrbitRadii[n1.Orbit],
                            SkillNode.OrbitRadii[n1.Orbit]));
                }
                else
                {
                    dc.DrawArc(null, pen2, n2.Position, n1.Position,
                        new Size(SkillNode.OrbitRadii[n1.Orbit],
                            SkillNode.OrbitRadii[n1.Orbit]));
                }
            }
            else
            {
                var draw = true;
                foreach (var attibute in n1.attributes)
                {
                    if (AscendantClassStartRegex.IsMatch(attibute))
                        draw = false;
                }
                if (n1.Type == NodeType.Mastery || n2.Type == NodeType.Mastery)
                    draw = false;
                if (draw)
                    dc.DrawLine(pen2, n1.Position, n2.Position);
            }
        }

        private void DrawInitialPaths(IEnumerable<ushort[]> links, bool onlyAscendancy = false)
        {
            DrawingContext dc = null;
            var adc = _ascPaths.RenderOpen();

            if (!onlyAscendancy)
                dc = _paths.RenderOpen();
            foreach (var nodeid in links)
            {
                var n1 = Skillnodes[nodeid[0]];
                var n2 = Skillnodes[nodeid[1]];

                if (n2.ascendancyName != null)
                {
                    if (!DrawAscendancy) continue;
                    if (_persistentData.Options.ShowAllAscendancyClasses ||
                        n2.ascendancyName == AscClasses.GetClassName(_chartype, AscType))
                        DrawConnection(adc, _basePathPen, n2, n1);
                }
                else
                {
                    if (onlyAscendancy) continue;
                    DrawConnection(dc, _basePathPen, n2, n1);
                }
            }
            adc.Close();
            dc?.Close();
        }

        private void DrawCharacterFaces()
        {
            using (var dc = _characterFaces.RenderOpen())
            {
                for (var i = 0; i < CharName.Count; i++)
                {
                    var s = CharName[i];
                    var pos = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == s).Value.Position;
                    dc.DrawRectangle(_startBackgrounds[false].Value, null,
                        new Rect(
                            pos - new Vector2D(_startBackgrounds[false].Key.Width, _startBackgrounds[false].Key.Height),
                            pos + new Vector2D(_startBackgrounds[false].Key.Width, _startBackgrounds[false].Key.Height)));
                    if (_chartype == i)
                    {
                        dc.DrawRectangle(_faceBrushes[i].Value, null,
                            new Rect(pos - new Vector2D(_faceBrushes[i].Key.Width, _faceBrushes[i].Key.Height),
                                pos + new Vector2D(_faceBrushes[i].Key.Width, _faceBrushes[i].Key.Height)));

                        var charBaseAttr = CharBaseAttributes[Chartype];

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
                var ascName = AscClasses.GetClassName(_chartype, AscType);
                foreach (var node in AscRootNodeList)
                {
                    if (!_persistentData.Options.ShowAllAscendancyClasses && node.ascendancyName != ascName) continue;
                    var imageName = "Classes" + node.ascendancyName;
                    var bitmap = Assets[imageName];
                    var brush = new ImageBrush(Assets[imageName]);
                    var pos = node.Position;
                    dc.DrawRectangle(brush, null,
                        new Rect(
                            pos -
                            new Vector2D(bitmap.PixelWidth * 1.25, bitmap.PixelHeight * 1.25),
                            new Size(bitmap.PixelWidth * 2.5, bitmap.PixelHeight * 2.5)));
                    var currentClass = AscClasses.GetClass(node.ascendancyName);
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
                            42, textBrush);
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
                    if (!SkilledNodes.Contains(Skillnodes[(ushort)i]))
                        continue;
                    var node = Skillnodes[(ushort)i];
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

        private static FormattedText CreateAttributeText(string text, SolidColorBrush colorBrush)
        {
            return new FormattedText(text,
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal,
                    new FontStretch()),
                30, colorBrush);
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
                    if (pair.Key.ascendancyName != null && !DrawAscendancy || pair.Key.Spc != null)
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
            DrawingContext dc = null;
            var adc = _ascActivePaths.RenderOpen();
            if (!onlyAscendancy)
                dc = ActivePaths.RenderOpen();
            var seen = new HashSet<SkillNode>();
            var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
            foreach (var n1 in SkilledNodes)
            {
                seen.Add(n1);
                foreach (var n2 in n1.VisibleNeighbors)
                {
                    if (!SkilledNodes.Contains(n2) || seen.Contains(n2)) continue;
                    if (n2.ascendancyName != null)
                    {
                        if (!DrawAscendancy) continue;
                        if (_persistentData.Options.ShowAllAscendancyClasses ||
                            n2.ascendancyName == ascendancyClassName)
                            DrawConnection(adc, _activePathPen, n2, n1);
                    }
                    else
                    {
                        if (onlyAscendancy) continue;
                        DrawConnection(dc, _activePathPen, n2, n1);
                    }
                }
            }
            adc.Close();
            dc?.Close();
        }

        private void DrawTreeComparisonHighlight()
        {
            var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
            var pen2 = new Pen(new SolidColorBrush(TreeComparisonColor), 25 * HighlightFactor);
            using (DrawingContext
                    dc = _nodeComparisonHighlight.RenderOpen(),
                    dcAsc = _ascNodeComparisonHighlight.RenderOpen(),
                    dcPath = _pathComparisonHighlight.RenderOpen(),
                    adcPath = _ascPathComparisonHighlight.RenderOpen())
            {
                if (HighlightedNodes != null)
                {
                    var seen = new HashSet<SkillNode>();
                    foreach (var n1 in HighlightedNodes)
                    {
                        seen.Add(n1);
                        DrawSurround(n1.ascendancyName != null ? dcAsc : dc, n1, false, false, true);
                        foreach (var n2 in n1.VisibleNeighbors)
                        {
                            if (!HighlightedNodes.Contains(n2) || seen.Contains(n2)) continue;

                            if (n2.ascendancyName != null && n1.ascendancyName != null)
                            {
                                if (!DrawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
                                    DrawConnection(adcPath, pen2, n2, n1);
                            }
                            else
                                DrawConnection(dcPath, pen2, n2, n1);
                        }   
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
                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscClasses.GetClassName(className, AscType);

                var pathNodes = path as IList<SkillNode> ?? path.ToList();
                if (pathNodes.Any())
                    skilledNeighbors = pathNodes.First().VisibleNeighbors.Where(sn => SkilledNodes.Contains(sn)).ToList();
                // The node might not be connected to a skilled node through visible neighbors
                // in which case we don't want to draw a connection anyway.
                if (skilledNeighbors.Any())
                {
                    if (pathNodes.First() != null && skilledNeighbors.First().ascendancyName != null)
                    {
                        if (DrawAscendancy)
                        {
                            if (_persistentData.Options.ShowAllAscendancyClasses || (pathNodes.First().ascendancyName == ascendancyClassName && skilledNeighbors.First().ascendancyName == ascendancyClassName))
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

                    if (n1.ascendancyName != null && n2.ascendancyName != null)
                    {
                        if (!DrawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
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
                var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
                var skillNodes = nodes as IList<SkillNode> ?? nodes.ToList();
                foreach (var node in skillNodes)
                {
                    foreach (var n2 in node.VisibleNeighbors)
                    {
                        if (!SkilledNodes.Contains(n2) || (node.Id >= n2.Id && skillNodes.Contains(n2))) continue;
                        if (node.ascendancyName != null && n2.ascendancyName != null)
                        {
                            if (!DrawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses ||
                                (node.ascendancyName == ascendancyClassName &&
                                    n2.ascendancyName == ascendancyClassName))
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
                var bi = ImageHelper.OnLoadBitmapImage(new Uri(_assetsFolderPath + faceName + ".png", UriKind.Absolute));
                _faceBrushes.Add(new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                    new ImageBrush(bi)));
            }

            var bi2 = ImageHelper.OnLoadBitmapImage(new Uri(_assetsFolderPath + "PSStartNodeBackgroundInactive.png", UriKind.Absolute));
            if (_startBackgrounds.ContainsKey(false))
            {
                if (!_startBackgrounds[false].Key.Equals(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight)))
                {
                    _startBackgrounds.Add(false,
                        (new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                            new ImageBrush(bi2))));
                }
            }
            else
            {
                _startBackgrounds.Add(false,
                    (new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
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
                DrawInitialPaths(Links, true);
            }
            else
            {
                _ascClassFaces.RenderOpen().Close();
                _ascPathComparisonHighlight.RenderOpen().Close();
                _ascNodeComparisonHighlight.RenderOpen().Close();
                _ascPaths.RenderOpen().Close();
                _ascActivePaths.RenderOpen().Close();
                _ascPathOverlay.RenderOpen().Close();
                _ascSkillIcons.RenderOpen().Close();
                _ascActiveSkillIcons.RenderOpen().Close();
                _ascNodeSurround.RenderOpen().Close();
                _ascActiveNodeSurround.RenderOpen().Close();
            }
        }

        public static void ClearAssets()
        {
            _initialized = false;
        }

        public void DrawJewelHighlight(SkillNode node)
        {
            const int thickness = 10;
            var radiusPen = new Pen(Brushes.Cyan, thickness);

            const int smallRadius = 800 - thickness / 2;
            const int mediumRadius = 1200 - thickness / 2;
            const int largeRadius = 1500 - thickness / 2;

            using (var dc = _jewelHighlight.RenderOpen())
            {
                dc.DrawEllipse(null, radiusPen, node.Position, smallRadius, smallRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, mediumRadius, mediumRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, largeRadius, largeRadius);
            }
        }
    }
}