using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using POESKillTree.Model;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members
        private static readonly Color TreeComparisonColor = Colors.RoyalBlue;

        private readonly List<KeyValuePair<Rect, ImageBrush>> _faceBrushes = new List<KeyValuePair<Rect, ImageBrush>>();
        private readonly List<KeyValuePair<Size, ImageBrush>> _nodeSurroundBrush = new List<KeyValuePair<Size, ImageBrush>>();
        private readonly List<KeyValuePair<Size, ImageBrush>> _nodeSurroundHighlightBrush = new List<KeyValuePair<Size, ImageBrush>>();
        private readonly Dictionary<bool, KeyValuePair<Rect, ImageBrush>> _startBackgrounds = new Dictionary<bool, KeyValuePair<Rect, ImageBrush>>();

        private readonly NodeHighlighter _nodeHighlighter = new NodeHighlighter();
        private readonly IPersistentData _persistentData;
        private readonly List<Tuple<int, Vector2D>> _originalPositions = new List<Tuple<int, Vector2D>>();
        public bool drawAscendancy = false;

        public DrawingVisual SkillTreeVisual;
        private DrawingVisual Background;
        private DrawingVisual NodeComparisonHighlight;
        private DrawingVisual PathComparisonHighlight;
        private DrawingVisual Paths;
        public DrawingVisual ActivePaths;
        private DrawingVisual PathOverlay;
        private DrawingVisual SkillIcons;
        private DrawingVisual AsctiveSkillIcons;
        private DrawingVisual NodeSurround;
        private DrawingVisual ActiveNodeSurround;
        private DrawingVisual CharacterFaces;
        private DrawingVisual Highlights;
        private DrawingVisual JewelHighlight;

        private DrawingVisual AscSkillTreeVisual;
        private DrawingVisual AscClassFaces;
        private DrawingVisual AscButtons;
        private DrawingVisual AscNodeComparisonHighlight;
        private DrawingVisual AscPathComparisonHighlight;
        public DrawingVisual AscPaths;
        private DrawingVisual AscActivePaths;
        private DrawingVisual AscPathOverlay;
        private DrawingVisual AscSkillIcons;
        private DrawingVisual AscActiveSkillIcons;
        private DrawingVisual AscNodeSurround;
        private DrawingVisual AscActiveNodeSurround;
        #endregion
        public void InitialSkillTreeDrawing()
        {
            if (_initialized) return;
            InitializeDrawingVisuals();
            InitializeNodeSurroundBrushes();
            InitializeFaceBrushes();
            DrawInitialLayers();
            //drawing
            CreateCombineVisuals();
        }
        /// <summary>
        /// This will initialize all drawing visuals. If a new drawing visual is added then it should be initialized here as well.
        /// </summary>
        private void InitializeDrawingVisuals()
        {
            SkillTreeVisual = new DrawingVisual();
            Background = new DrawingVisual();
            NodeComparisonHighlight = new DrawingVisual();
            PathComparisonHighlight = new DrawingVisual();
            Paths = new DrawingVisual();
            ActivePaths = new DrawingVisual();
            PathOverlay = new DrawingVisual();
            SkillIcons = new DrawingVisual();
            AsctiveSkillIcons = new DrawingVisual();
            NodeSurround = new DrawingVisual();
            ActiveNodeSurround = new DrawingVisual();
            CharacterFaces = new DrawingVisual();
            Highlights = new DrawingVisual();
            JewelHighlight = new DrawingVisual();

            AscSkillTreeVisual = new DrawingVisual();
            AscClassFaces = new DrawingVisual();
            AscButtons = new DrawingVisual();
            AscNodeComparisonHighlight = new DrawingVisual();
            AscPathComparisonHighlight = new DrawingVisual();
            AscPaths = new DrawingVisual();
            AscActivePaths = new DrawingVisual();
            AscPathOverlay = new DrawingVisual();
            AscSkillIcons = new DrawingVisual();
            AscActiveSkillIcons = new DrawingVisual();
            AscNodeSurround = new DrawingVisual();
            AscActiveNodeSurround = new DrawingVisual();
        }

        private void DrawInitialLayers()
        {
            DrawBackgroundLayer();
            DrawNodeSurroundLayer();
            DrawNodeSkillIcons();
            //stopped here
            DrawLinkBackgroundLayer(Links);
            DrawAscendancyLinkBackgroundLayer(Links);
            DrawAscendancyClasses();
            DrawDynamicLayers();
        }
        private void DrawDynamicLayers()
        {
            DrawActiveNodeIcons();
            DrawNodeHighlightSurround();
            DrawFaces();

            DrawAscendancyActiveNodeIcons();
            DrawAscendancyNodeHighlightSurround();
        }

        private void CreateCombineVisuals()
        {
            //Top most add will be the bottom most element drawn
            SkillTreeVisual.Children.Add(Background);
            SkillTreeVisual.Children.Add(NodeComparisonHighlight);
            SkillTreeVisual.Children.Add(PathComparisonHighlight);
            SkillTreeVisual.Children.Add(Paths);
            SkillTreeVisual.Children.Add(ActivePaths);
            SkillTreeVisual.Children.Add(PathOverlay);
            SkillTreeVisual.Children.Add(SkillIcons);
            SkillTreeVisual.Children.Add(AsctiveSkillIcons);
            SkillTreeVisual.Children.Add(NodeSurround);
            SkillTreeVisual.Children.Add(ActiveNodeSurround);
            SkillTreeVisual.Children.Add(CharacterFaces);

            AscSkillTreeVisual.Children.Add(AscClassFaces);
            AscSkillTreeVisual.Children.Add(AscNodeComparisonHighlight);
            AscSkillTreeVisual.Children.Add(AscPathComparisonHighlight);
            AscSkillTreeVisual.Children.Add(AscPaths);
            AscSkillTreeVisual.Children.Add(AscActivePaths);
            AscSkillTreeVisual.Children.Add(AscPathOverlay);
            AscSkillTreeVisual.Children.Add(AscSkillIcons);
            AscSkillTreeVisual.Children.Add(AscActiveSkillIcons);
            AscSkillTreeVisual.Children.Add(AscNodeSurround);
            AscSkillTreeVisual.Children.Add(AscActiveNodeSurround);

            SkillTreeVisual.Children.Add(AscSkillTreeVisual);
            SkillTreeVisual.Children.Add(AscButtons);
            SkillTreeVisual.Children.Add(Highlights);
            SkillTreeVisual.Children.Add(JewelHighlight);
        }

        private void InitializeNodeSurroundBrushes()
        {
            if (_initialized) return;
            foreach (var background in NodeBackgrounds)
            {
                if (NodeBackgroundsActive.ContainsKey(background.Key))
                {
                    var normalBrush = new ImageBrush {Stretch = Stretch.Uniform};
                    BitmapImage normalBrushPImage = _assets[NodeBackgrounds[background.Key]];
                    normalBrush.ImageSource = normalBrushPImage;
                    var normalSize = new Size(normalBrushPImage.PixelWidth, normalBrushPImage.PixelHeight);

                    var activeBrush = new ImageBrush {Stretch = Stretch.Uniform};
                    BitmapImage activeBrushPImage = _assets[NodeBackgroundsActive[background.Key]];
                    activeBrush.ImageSource = activeBrushPImage;
                    var activeSize = new Size(activeBrushPImage.PixelWidth, activeBrushPImage.PixelHeight);

                    _nodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(normalSize, normalBrush));
                    _nodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(activeSize, activeBrush));

                    //tree comparison highlight generator
                    var outlinecolor = TreeComparisonColor;
                    var omask = outlinecolor.B | (uint)outlinecolor.G << 8 | (uint)outlinecolor.R << 16;

                    var bitmap = (BitmapImage)normalBrush.ImageSource;
                    var wb = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
                    if (wb.Format == PixelFormats.Bgra32)//BGRA is byte order .. little endian in uint reverse it
                    {
                        uint[] pixeldata = new uint[wb.PixelHeight * wb.PixelWidth];
                        bitmap.CopyPixels(pixeldata, wb.PixelWidth * 4, 0);
                        for (int i = 0; i < pixeldata.Length; i++)
                        {
                            pixeldata[i] = pixeldata[i] & 0xFF000000 | omask;
                        }
                        wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), pixeldata, wb.PixelWidth * 4, 0);

                        var ibr = new ImageBrush
                        {
                            Stretch = Stretch.Uniform,
                            ImageSource = wb
                        };

                        _nodeSurroundHighlightBrush.Add(new KeyValuePair<Size, ImageBrush>(normalSize, ibr));
                    }
                    else
                    {
                        throw new Exception("Highlight Generator did not generate with the correct byte order");
                    }
                }
            }
        }
        private void DrawSkillNodeIcon(DrawingContext dc, DrawingContext adc, SkillNode skillNode, bool onlyAscendancy = false, bool isActive = false)
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

            if (skillNode.ascendancyName != null)
            {
                var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);
                if (drawAscendancy && (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName))
                    adc.DrawEllipse(imageBrush, null, skillNode.Position, rect.Width, rect.Height);
            }
            else
                dc.DrawEllipse(imageBrush, null, skillNode.Position, rect.Width, rect.Height);
        }

        private void DrawSurround(DrawingContext dc, SkillNode node, bool onlyAscendancy = false, bool isActive = false, bool isHighlight = false)
        {
            if (onlyAscendancy && node.ascendancyName == null) return;
            var surroundBrush = _nodeSurroundBrush;
            var factor = 1f;
            var activeOffset = 0;
            if (isActive)
                activeOffset = 1;
            if (isHighlight)
            {
                surroundBrush = _nodeSurroundHighlightBrush;
                factor = 1.2f;
            }
            var ascendancyClassName = AscClasses.GetClassName(_chartype, AscType);

            if (node.IsAscendancyStart)
            {
                if (!drawAscendancy || isHighlight) return;

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
                if (!drawAscendancy) return;

                if (_persistentData.Options.ShowAllAscendancyClasses || node.ascendancyName == ascendancyClassName)
                    dc.DrawRectangle(surroundBrush[10 + activeOffset].Value, null,
                        new Rect((int)node.Position.X - surroundBrush[10 + activeOffset].Key.Width * .875 * factor,
                            (int)node.Position.Y - surroundBrush[10 + activeOffset].Key.Height * .875 * factor,
                            surroundBrush[10 + activeOffset].Key.Width * 1.75 * factor,
                            surroundBrush[10 + activeOffset].Key.Height * 1.75 * factor));
            }
            else if (node.ascendancyName != null && node.Type == NodeType.Normal)
            {
                if (!drawAscendancy) return;

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

        private void DrawNodeSkillIcons()
        {
            var pen = new Pen(Brushes.Black, 5);
            Geometry g = new RectangleGeometry(SkillTreeRect);
            using (var dc = SkillIcons.RenderOpen())
            {
                dc.DrawGeometry(null, pen, g);
                using (var adc = AscSkillIcons.RenderOpen())
                {
                    adc.DrawGeometry(null, pen, g);
                    foreach (var skillNode in Skillnodes)
                    {
                        DrawSkillNodeIcon(dc, adc, skillNode.Value);
                    }
                    adc.Close();
                }
                dc.Close();
            }
        }

        

        private void DrawNodeSurroundLayer()
        {
            using (DrawingContext dc = NodeSurround.RenderOpen())
            {
                using (DrawingContext adc = AscNodeSurround.RenderOpen())
                {
                    foreach (var n in Skillnodes)
                    {
                        DrawSurround(n.Value.ascendancyName != null ? adc : dc, n.Value);
                    }
                    adc.Close();
                }
                dc.Close();
            }
        }
        public void ClearPath()
        {
            PathOverlay.RenderOpen().Close();
            AscPathOverlay.RenderOpen().Close();
        }

        public void ClearJewelHighlight()
        {
            JewelHighlight.RenderOpen().Close();
        }

        public void ToggleAscendancyTree()
        {
            drawAscendancy = !drawAscendancy;
            DrawAscendancyLayers();
        }
        
        public void ToggleAscendancyTree(bool draw)
        {
            drawAscendancy = draw;
            DrawAscendancyLayers();
        }

        private void UpdateAscendancyClassPositions()
        {
            if (!_persistentData.Options.ShowAllAscendancyClasses)
            {
                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var nodeList = Skillnodes.Where(x => x.Value.ascendancyName == AscClasses.GetClassName(className, AscType) && !x.Value.IsAscendancyStart);
                var worldPos = Skillnodes.First(x => x.Value.Name.ToUpperInvariant() == CharName[_chartype]).Value.Position;
                var ascStartNode = Skillnodes.First(x => x.Value.ascendancyName == AscClasses.GetClassName(className, AscType) && x.Value.IsAscendancyStart).Value;
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
                foreach(var g in _originalPositions)
                {
                    foreach(var n in Skillnodes)
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
            using (var dc = Background.RenderOpen())
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
                var backgroundBrush = new ImageBrush(Assets["Background1"]) {TileMode = TileMode.Tile};
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
                dc.Close();
            }
        }

        private static void DrawConnection(DrawingContext dc, Pen pen2, SkillNode n1, SkillNode n2)
        {
            if (n1.VisibleNeighbors.Contains(n2) && n2.VisibleNeighbors.Contains(n1))
            {
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
                    foreach(var attibute in n1.attributes)
                    {
                        if(AscendantClassStartRegex.IsMatch(attibute))
                            draw = false;
                    }
                    if (n1.Type == NodeType.Mastery || n2.Type == NodeType.Mastery)
                        draw = false;
                    if (draw)
                        dc.DrawLine(pen2, n1.Position, n2.Position);
                }
            }
        }

        private void DrawFaces()
        {
            using (DrawingContext dc = CharacterFaces.RenderOpen())
            {
                for (int i = 0; i < CharName.Count; i++)
                {
                    string s = CharName[i];
                    Vector2D pos = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == s).Value.Position;
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
                dc.Close();
            }
        }

        private void DrawAscendancyClasses()
        {
            using (DrawingContext dc = AscClassFaces.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dc.Close();
                    return;
                }
                foreach (var node in Skillnodes)
                {
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    if (node.Value.IsAscendancyStart && (_persistentData.Options.ShowAllAscendancyClasses || node.Value.ascendancyName == AscClasses.GetClassName(className, AscType)))
                    {
                        var imageName = "Classes" + node.Value.ascendancyName;
                        var bitmap = Assets[imageName]; 
                        var brush = new ImageBrush(Assets[imageName]);
                        var pos = node.Value.Position;
                        dc.DrawRectangle(brush, null,
                            new Rect(
                                pos -
                                new Vector2D(bitmap.PixelWidth * 1.25, bitmap.PixelHeight * 1.25),
                                new Size(bitmap.PixelWidth * 2.5, bitmap.PixelHeight * 2.5)));
                        var currentClass = AscClasses.GetClass(node.Value.ascendancyName);
                        if(currentClass != null)
                        {
                            var textBrush = new SolidColorBrush(Color.FromRgb(
                                (byte) currentClass.FlavourTextColour[0],
                                (byte) currentClass.FlavourTextColour[1],
                                (byte) currentClass.FlavourTextColour[2]));
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
                dc.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">"Normal", "Highlight", and "Pressed"</param>
        public void DrawAscendancyButton(string type = "Normal")
        {
            using (DrawingContext dc = AscButtons.RenderOpen())
            {
                if (AscType != 0 && !_persistentData.Options.ShowAllAscendancyClasses)
                {
                    foreach (var i in RootNodeList)
                    {
                        if (!SkilledNodes.Contains(Skillnodes[(ushort) i]))
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
                dc.Close();
            }
        }

        private FormattedText CreateAttributeText(string text, SolidColorBrush colorBrush)
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
            using (DrawingContext dc = Highlights.RenderOpen())
            {
                foreach (var pair in nh.nodeHighlights)
                {
                    if (pair.Key.ascendancyName != null && !drawAscendancy || pair.Key.Spc != null)
                        continue;
                    // TODO: Make more elegant? Needs profiling.
                    HighlightState hs = pair.Value;

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
                            int red = 0;
                            int green = 0;
                            int blue = 0;
                            System.Drawing.Color attrHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeAttrHighlightColor);
                            System.Drawing.Color searchHighlight = System.Drawing.Color.FromName(_persistentData.Options.NodeSearchHighlightColor);

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
                        dc.DrawLine(checkPen, new Point(x - 10, y + 50), new Point(x - 50, y + 20));
                        dc.DrawLine(checkPen, new Point(x + 50, y - 50), new Point(x - 22, y + 52));
                    }

                    if (hs.HasFlag(HighlightState.Crossed))
                    {
                        // Crossed nodes get highlighted with two crossing red lines.
                        dc.DrawLine(crossPen, new Point(x + 50, y + 70), new Point(x - 50, y - 70));
                        dc.DrawLine(crossPen, new Point(x + 50, y - 70), new Point(x - 50, y + 70));
                    }
                }
                dc.Close();
            }
        }

        private void DrawLinkBackgroundLayer(List<ushort[]> links)
        {
            var pen2 = new Pen(Brushes.DarkSlateGray, 20f);
            using (DrawingContext dc = Paths.RenderOpen())
            {
                foreach (var nid in links)
                {
                    var n1 = Skillnodes[nid[0]];
                    var n2 = Skillnodes[nid[1]];
                    if (n1.ascendancyName != null && n2.ascendancyName != null) continue;

                    DrawConnection(dc, pen2, n1, n2);
                }
                dc.Close();
            }
        }

        private void DrawAscendancyLinkBackgroundLayer(List<ushort[]> links)
        {
            var pen2 = new Pen(Brushes.DarkSlateGray, 20f);
            using (DrawingContext dcAsc = AscPaths.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                foreach (var nid in links)
                {
                    var n1 = Skillnodes[nid[0]];
                    var n2 = Skillnodes[nid[1]];
                    if (n1.ascendancyName == null || n2.ascendancyName == null) continue;

                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == AscClasses.GetClassName(className, AscType) && n2.ascendancyName == AscClasses.GetClassName(className, AscType)))
                        DrawConnection(dcAsc, pen2, n1, n2);
                }
                dcAsc.Close();
            }
        }

        //TODO: SpaceOgre: Check what really needs to be drawn when this is called.
        private void UpdateAvailNodesDraw()
        {
            DrawActiveLinks();
            DrawAscendancyActiveLinks();
            DrawDynamicLayers();
        }

        private void DrawActiveLinks()
        {
            var pen2 = new Pen(Brushes.DarkKhaki, 15f);
            using (DrawingContext dc = ActivePaths.RenderOpen())
            {
                foreach (var n1 in SkilledNodes)
                {
                    foreach (SkillNode n2 in n1.VisibleNeighbors)
                    {
                        if (!SkilledNodes.Contains(n2) || n2.ascendancyName != null) continue;
                        DrawConnection(dc, pen2, n2, n1);
                    }
                }
                dc.Close();
            }
        }

        private void DrawAscendancyActiveLinks()
        {
            var pen2 = new Pen(Brushes.DarkKhaki, 15f);
            using (DrawingContext dcAsc = AscActivePaths.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscClasses.GetClassName(className, AscType);
                foreach (var n1 in SkilledNodes)
                {
                    foreach (var n2 in n1.VisibleNeighbors)
                    {
                        if (n2.ascendancyName == null || !SkilledNodes.Contains(n2)) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || n2.ascendancyName == ascendancyClassName)
                            DrawConnection(dcAsc, pen2, n2, n1);
                    }
                }
                dcAsc.Close();
            }
        }

        internal void DrawTreeComparisonHighlight()
        {
            const float factor = 1.2f;
            var className = CharacterNames.GetClassNameFromChartype(_chartype);
            var ascendancyClassName = AscClasses.GetClassName(className, AscType);
            using (DrawingContext dc = NodeComparisonHighlight.RenderOpen())
            {
                using (DrawingContext dcAsc = AscNodeComparisonHighlight.RenderOpen())
                {
                    if (HighlightedNodes != null)
                    {
                        foreach (var skillNode in HighlightedNodes)
                        {
                            var pos = skillNode.Position;
                            if (skillNode.IsAscendancyStart)
                            {
                                //already drawn, but needs to be here to prevent highlighting
                            }
                            else if (skillNode.ascendancyName != null && skillNode.Type == NodeType.Notable)
                            {
                                if (!drawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName)
                                    dcAsc.DrawRectangle(_nodeSurroundHighlightBrush[5].Value, null,
                                        new Rect((int)pos.X - _nodeSurroundHighlightBrush[5].Key.Width * .875 * factor,
                                            (int)pos.Y - _nodeSurroundHighlightBrush[5].Key.Height * .875 * factor,
                                            _nodeSurroundHighlightBrush[5].Key.Width * 1.75 * factor,
                                            _nodeSurroundHighlightBrush[5].Key.Height * 1.75 * factor));
                            }
                            else if (skillNode.ascendancyName != null)
                            {
                                if (!drawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName)
                                    dcAsc.DrawRectangle(_nodeSurroundHighlightBrush[4].Value, null,
                                        new Rect((int)pos.X - _nodeSurroundHighlightBrush[4].Key.Width * factor,
                                            (int)pos.Y - _nodeSurroundHighlightBrush[4].Key.Height * factor,
                                            _nodeSurroundHighlightBrush[4].Key.Width * 2 * factor,
                                            _nodeSurroundHighlightBrush[4].Key.Height * 2 * factor));
                            }
                            else if (skillNode.Type == NodeType.Notable)
                            {
                                dc.DrawRectangle(_nodeSurroundHighlightBrush[1].Value, null,
                                    new Rect((int)pos.X - _nodeSurroundHighlightBrush[1].Key.Width * factor,
                                        (int)pos.Y - _nodeSurroundHighlightBrush[1].Key.Height * factor,
                                        _nodeSurroundHighlightBrush[1].Key.Width * 2 * factor,
                                        _nodeSurroundHighlightBrush[1].Key.Height * 2 * factor));
                            }
                            else if (skillNode.Type == NodeType.Keystone)
                            {
                                dc.DrawRectangle(_nodeSurroundHighlightBrush[2].Value, null,
                                    new Rect((int)pos.X - _nodeSurroundHighlightBrush[2].Key.Width * factor,
                                        (int)pos.Y - _nodeSurroundHighlightBrush[2].Key.Height * factor,
                                        _nodeSurroundHighlightBrush[2].Key.Width * 2 * factor,
                                        _nodeSurroundHighlightBrush[2].Key.Height * 2 * factor));
                            }
                            else if (skillNode.Type == NodeType.Mastery)
                            {
                                //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                            }
                            else if (skillNode.Type == NodeType.JewelSocket)
                            {
                                dc.DrawRectangle(_nodeSurroundHighlightBrush[3].Value, null,
                                    new Rect((int)pos.X - _nodeSurroundHighlightBrush[3].Key.Width * factor,
                                        (int)pos.Y - _nodeSurroundHighlightBrush[3].Key.Height * factor,
                                        _nodeSurroundHighlightBrush[3].Key.Width * 2 * factor,
                                        _nodeSurroundHighlightBrush[3].Key.Height * 2 * factor));
                            }
                            else
                                dc.DrawRectangle(_nodeSurroundHighlightBrush[0].Value, null,
                                    new Rect((int)pos.X - _nodeSurroundHighlightBrush[0].Key.Width * factor,
                                        (int)pos.Y - _nodeSurroundHighlightBrush[0].Key.Height * factor,
                                        _nodeSurroundHighlightBrush[0].Key.Width * 2 * factor,
                                        _nodeSurroundHighlightBrush[0].Key.Height * 2 * factor));
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }

            var pen2 = new Pen(new SolidColorBrush(TreeComparisonColor), 25 * factor);
            using (DrawingContext dc = PathComparisonHighlight.RenderOpen())
            {
                using (DrawingContext dcAsc = AscPathComparisonHighlight.RenderOpen())
                {
                    if (HighlightedNodes != null)
                    {
                        foreach (var n1 in HighlightedNodes)
                        {
                            foreach (var n2 in n1.VisibleNeighbors)
                            {
                                if (!HighlightedNodes.Contains(n2)) continue;

                                if (n2.ascendancyName != null && n1.ascendancyName != null)
                                {
                                    if (!drawAscendancy) continue;

                                    if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
                                        DrawConnection(dcAsc, pen2, n2, n1);
                                }
                                else
                                    DrawConnection(dc, pen2, n2, n1);
                            }
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }
        }

        private void DrawNodeHighlightSurround()
        {
            using (DrawingContext dc = ActiveNodeSurround.RenderOpen())
            {
                foreach (var skillNode in SkilledNodes)
                {
                    var pos = (skillNode.Position);
                    if (skillNode.IsAscendancyStart || skillNode.ascendancyName != null)
                    {
                        //Needs to be here so that Ascendancy nodes don't get anything drawn around them.
                    }
                    else if (skillNode.Type == NodeType.Notable)
                    {
                        dc.DrawRectangle(_nodeSurroundBrush[3].Value, null,
                            new Rect((int)pos.X - _nodeSurroundBrush[3].Key.Width,
                                (int)pos.Y - _nodeSurroundBrush[3].Key.Height,
                                _nodeSurroundBrush[3].Key.Width * 2,
                                _nodeSurroundBrush[3].Key.Height * 2));
                    }
                    else if (skillNode.Type == NodeType.Keystone)
                    {
                        dc.DrawRectangle(_nodeSurroundBrush[5].Value, null,
                            new Rect((int)pos.X - _nodeSurroundBrush[5].Key.Width,
                                (int)pos.Y - _nodeSurroundBrush[5].Key.Height,
                                _nodeSurroundBrush[5].Key.Width * 2,
                                _nodeSurroundBrush[5].Key.Height * 2));
                    }
                    else if (skillNode.Type == NodeType.Mastery)
                    {
                        //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                    }
                    else if (skillNode.Type == NodeType.JewelSocket)
                    {
                        dc.DrawRectangle(_nodeSurroundBrush[7].Value, null,
                            new Rect((int)pos.X - _nodeSurroundBrush[7].Key.Width,
                                (int)pos.Y - _nodeSurroundBrush[7].Key.Height,
                                _nodeSurroundBrush[7].Key.Width * 2,
                                _nodeSurroundBrush[7].Key.Height * 2));
                    }
                    else
                        dc.DrawRectangle(_nodeSurroundBrush[1].Value, null,
                            new Rect((int)pos.X - _nodeSurroundBrush[1].Key.Width,
                                (int)pos.Y - _nodeSurroundBrush[1].Key.Height,
                                _nodeSurroundBrush[1].Key.Width * 2,
                                _nodeSurroundBrush[1].Key.Height * 2));
                }
                dc.Close();
            }
        }

        private void DrawAscendancyNodeHighlightSurround()
        {
            using (DrawingContext dcAsc = AscActiveNodeSurround.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscClasses.GetClassName(className, AscType);
                foreach (var skillNode in SkilledNodes)
                {
                    var pos = (skillNode.Position);
                    if (skillNode.IsAscendancyStart)
                    {
                        //already drawn, but needs to be here to prevent highlighting
                    }
                    else if (skillNode.ascendancyName != null && skillNode.Type == NodeType.Notable)
                    {
                        if (!drawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName)
                            dcAsc.DrawRectangle(_nodeSurroundBrush[11].Value, null,
                                new Rect((int)pos.X - _nodeSurroundBrush[11].Key.Width * .875,
                                    (int)pos.Y - _nodeSurroundBrush[11].Key.Height * .875,
                                    _nodeSurroundBrush[11].Key.Width * 1.75,
                                    _nodeSurroundBrush[11].Key.Height * 1.75));
                    }
                    else if (skillNode.ascendancyName != null)
                    {
                        if (!drawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName)
                            dcAsc.DrawRectangle(_nodeSurroundBrush[9].Value, null,
                                new Rect((int)pos.X - _nodeSurroundBrush[9].Key.Width,
                                    (int)pos.Y - _nodeSurroundBrush[9].Key.Height,
                                    _nodeSurroundBrush[9].Key.Width * 2,
                                    _nodeSurroundBrush[9].Key.Height * 2));
                    }
                }
                dcAsc.Close();
            }
        }

        public void DrawPath(IEnumerable<SkillNode> path)
        {
            var pen2 = new Pen(Brushes.LawnGreen, 15f) {DashStyle = new DashStyle(new DoubleCollection {2}, 2)};

            using (DrawingContext dc = PathOverlay.RenderOpen())
            {
                using (DrawingContext dcAsc = AscPathOverlay.RenderOpen())
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
                            if (drawAscendancy)
                            {
                                if (_persistentData.Options.ShowAllAscendancyClasses || (pathNodes.First().ascendancyName == ascendancyClassName && skilledNeighbors.First().ascendancyName == ascendancyClassName))
                                    DrawConnection(dcAsc, pen2, skilledNeighbors.First(), pathNodes.First());
                            }
                        }
                        else
                            DrawConnection(dc, pen2, skilledNeighbors.First(), pathNodes.First());
                    }

                    // Draw connections for the path itself (only those that should be visible).
                    for (var i = 0; i < pathNodes.Count - 1; i++)
                    {
                        var n1 = pathNodes.ElementAt(i);
                        var n2 = pathNodes.ElementAt(i + 1);
                        if (!n1.VisibleNeighbors.Contains(n2)) continue;

                        if (n1.ascendancyName != null && n2.ascendancyName != null)
                        {
                            if (!drawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
                                DrawConnection(dcAsc, pen2, n1, n2);
                        }
                        else
                            DrawConnection(dc, pen2, n1, n2);
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }
        }

        public void DrawRefundPreview(IEnumerable<SkillNode> nodes)
        {
            var pen2 = new Pen(Brushes.Red, 15f) {DashStyle = new DashStyle(new DoubleCollection {2}, 2)};

            using (var dc = PathOverlay.RenderOpen())
            {
                using (var dcAsc = AscPathOverlay.RenderOpen())
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
                                if (!drawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses ||
                                    (node.ascendancyName == ascendancyClassName &&
                                     n2.ascendancyName == ascendancyClassName))
                                    DrawConnection(dcAsc, pen2, node, n2);
                            }
                            else
                                DrawConnection(dc, pen2, node, n2);
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }
        }

        private void DrawActiveNodeIcons()
        {
            var pen = new Pen(Brushes.Black, 5);
            Geometry g = new RectangleGeometry(SkillTreeRect);
            using (DrawingContext dc = AsctiveSkillIcons.RenderOpen())
            {
                dc.DrawGeometry(null, pen, g);

                foreach (var skillNode in SkilledNodes)
                {
                    if (skillNode.ascendancyName != null) continue;

                    var imageBrush = new ImageBrush();
                    var rect = IconActiveSkills.SkillPositions[skillNode.IconKey];
                    var bitmapImage = IconActiveSkills.GetSkillImage(skillNode.IconKey);
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;

                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    var pos = (skillNode.Position);
                    dc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }

                dc.Close();
            }
        }

        private void DrawAscendancyActiveNodeIcons()
        {
            var pen = new Pen(Brushes.Black, 5);
            Geometry g = new RectangleGeometry(SkillTreeRect);
            using (DrawingContext dcAsc = AscActiveSkillIcons.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                dcAsc.DrawGeometry(null, pen, g);
                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscClasses.GetClassName(className, AscType);
                foreach (var skillNode in SkilledNodes)
                {
                    if (skillNode.ascendancyName == null) continue;
                    if (!_persistentData.Options.ShowAllAscendancyClasses && skillNode.ascendancyName != ascendancyClassName)  continue;

                    var imageBrush = new ImageBrush();
                    var rect = IconActiveSkills.SkillPositions[skillNode.IconKey];
                    var bitmapImage = IconActiveSkills.GetSkillImage(skillNode.IconKey);
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;

                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    var pos = (skillNode.Position);

                    if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.ascendancyName == ascendancyClassName)
                        dcAsc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }
                dcAsc.Close();
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
            if (drawAscendancy)
            {
                UpdateAscendancyClassPositions();
                DrawAscendancyLinkBackgroundLayer(Links);
                DrawAscendancyClasses();
                DrawAscendancyActiveLinks();
                DrawAscendancyActiveNodeIcons();
                DrawAscendancyNodeHighlightSurround();
            }
            else
            {
                AscClassFaces.RenderOpen().Close();
                AscPathComparisonHighlight.RenderOpen().Close();
                AscNodeComparisonHighlight.RenderOpen().Close();
                AscPaths.RenderOpen().Close();
                AscActivePaths.RenderOpen().Close();
                AscPathOverlay.RenderOpen().Close();
                AscSkillIcons.RenderOpen().Close();
                AscActiveSkillIcons.RenderOpen().Close();
                AscNodeSurround.RenderOpen().Close();
                AscActiveNodeSurround.RenderOpen().Close();   
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

            using (DrawingContext dc = JewelHighlight.RenderOpen())
            {
                dc.DrawEllipse(null, radiusPen, node.Position, smallRadius, smallRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, mediumRadius, mediumRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, largeRadius, largeRadius);
                dc.Close();
            }
        }
    }
}