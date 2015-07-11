using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using POESKillTree.Views;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members

        private static readonly Color _treeComparisonColor = Colors.RoyalBlue;

        private static List<KeyValuePair<Rect, ImageBrush>> _FacesBrush = new List<KeyValuePair<Rect, ImageBrush>>();

        public static List<KeyValuePair<Rect, ImageBrush>> FacesBrush
        {
            get { return _FacesBrush; }
        }

        private static List<KeyValuePair<Size, ImageBrush>> _NodeSurroundBrush = new List<KeyValuePair<Size, ImageBrush>>();

        public static List<KeyValuePair<Size, ImageBrush>> NodeSurroundBrush
        {
            get { return SkillTree._NodeSurroundBrush; }
        }

        private List<KeyValuePair<Size, ImageBrush>> _NodeSurroundHighlightBrush = new List<KeyValuePair<Size, ImageBrush>>();

        public List<KeyValuePair<Size, ImageBrush>> NodeSurroundHighlightBrush
        {
            get { return _NodeSurroundHighlightBrush; }
        }

        private static Dictionary<bool, KeyValuePair<Rect, ImageBrush>> _StartBackgrounds = new Dictionary<bool, KeyValuePair<Rect, ImageBrush>>();

        public static Dictionary<bool, KeyValuePair<Rect, ImageBrush>> StartBackgrounds
        {
            get { return SkillTree._StartBackgrounds; }
        }
        // FIXME: private
        public NodeHighlighter _nodeHighlighter = new NodeHighlighter();

        public DrawingVisual SkillTreeVisual;
        public DrawingVisual picActiveLinks;
        public DrawingVisual picBackground;
        public DrawingVisual picFaces;
        public DrawingVisual picHighlights;
        public DrawingVisual picLinks;
        public DrawingVisual picPathOverlay;
        public DrawingVisual picSkillBaseSurround;
        public DrawingVisual picSkillIconLayer;
        public DrawingVisual picActiveSkillIconLayer;
        public DrawingVisual picSkillSurround;
        public DrawingVisual picJewelHighlight;


        public DrawingVisual picSkillSurroundHighlight;
        public DrawingVisual picPathHighlight;

        public void CreateCombineVisual()
        {
            SkillTreeVisual = new DrawingVisual();
            SkillTreeVisual.Children.Add(picBackground);
            SkillTreeVisual.Children.Add(picPathHighlight);
            SkillTreeVisual.Children.Add(picLinks);
            SkillTreeVisual.Children.Add(picActiveLinks);
            SkillTreeVisual.Children.Add(picPathOverlay);
            SkillTreeVisual.Children.Add(picSkillIconLayer);
            SkillTreeVisual.Children.Add(picActiveSkillIconLayer);
            SkillTreeVisual.Children.Add(picSkillSurroundHighlight);
            SkillTreeVisual.Children.Add(picSkillBaseSurround);
            SkillTreeVisual.Children.Add(picSkillSurround);
            SkillTreeVisual.Children.Add(picFaces);
            SkillTreeVisual.Children.Add(picHighlights);
            SkillTreeVisual.Children.Add(picJewelHighlight);
        }

        #endregion

        public void ClearPath()
        {
            picPathOverlay.RenderOpen().Close();
        }
        public void ClearJewelHighlight()
        {
            picJewelHighlight.RenderOpen().Close();
        }

        private void DrawBackgroundLayer()
        {
            picBackground = new DrawingVisual();
            using (var drawingContext = picBackground.RenderOpen())
            {
                BitmapImage[] iscr =
                {
                    _assets["PSGroupBackground1"].PImage, 
                    _assets["PSGroupBackground2"].PImage,
                    _assets["PSGroupBackground3"].PImage
                };
                var orbitBrush = new Brush[3];
                orbitBrush[0] = new ImageBrush(_assets["PSGroupBackground1"].PImage);
                orbitBrush[1] = new ImageBrush(_assets["PSGroupBackground2"].PImage);
                orbitBrush[2] = new ImageBrush(_assets["PSGroupBackground3"].PImage);
                (orbitBrush[2] as ImageBrush).TileMode = TileMode.FlipXY;
                (orbitBrush[2] as ImageBrush).Viewport = new Rect(0, 0, 1, .5f);

                var backgroundBrush = new ImageBrush(_assets["Background1"].PImage);
                backgroundBrush.TileMode = TileMode.Tile;
                backgroundBrush.Viewport = new Rect(0, 0,
                    6 * backgroundBrush.ImageSource.Width / TRect.Width,
                    6 * backgroundBrush.ImageSource.Height / TRect.Width);
                drawingContext.DrawRectangle(backgroundBrush, null, TRect);

                var topGradient = new LinearGradientBrush();
                topGradient.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
                topGradient.GradientStops.Add(new GradientStop(new Color(), 0.0));
                topGradient.StartPoint = new Point(0, 1);
                topGradient.EndPoint = new Point(0, 0);

                var leftGradient = new LinearGradientBrush();
                leftGradient.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
                leftGradient.GradientStops.Add(new GradientStop(new Color(), 0.0));
                leftGradient.StartPoint = new Point(1, 0);
                leftGradient.EndPoint = new Point(0, 0);

                var bottomGradient = new LinearGradientBrush();
                bottomGradient.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
                bottomGradient.GradientStops.Add(new GradientStop(new Color(), 0.0));
                bottomGradient.StartPoint = new Point(0, 0);
                bottomGradient.EndPoint = new Point(0, 1);

                var rightGradient = new LinearGradientBrush();
                rightGradient.GradientStops.Add(new GradientStop(Colors.Black, 1.0));
                rightGradient.GradientStops.Add(new GradientStop(new Color(), 0.0));
                rightGradient.StartPoint = new Point(0, 0);
                rightGradient.EndPoint = new Point(1, 0);

                const int gradientWidth = 200;
                var topRect = new Rect2D(TRect.Left, TRect.Top, TRect.Width, gradientWidth);
                var leftRect = new Rect2D(TRect.Left, TRect.Top, gradientWidth, TRect.Height);
                var bottomRect = new Rect2D(TRect.Left, TRect.Bottom - gradientWidth, TRect.Width, gradientWidth);
                var rightRect = new Rect2D(TRect.Right - gradientWidth, TRect.Top, gradientWidth, TRect.Height);

                drawingContext.DrawRectangle(topGradient, null, topRect);
                drawingContext.DrawRectangle(leftGradient, null, leftRect);
                drawingContext.DrawRectangle(bottomGradient, null, bottomRect);
                drawingContext.DrawRectangle(rightGradient, null, rightRect);
                foreach (var skillNodeGroup in NodeGroups)
                {
                    if (skillNodeGroup.OcpOrb == null)
                        skillNodeGroup.OcpOrb = new Dictionary<int, bool>();
                    var cgrp = skillNodeGroup.OcpOrb.Keys.Where(ng => ng <= 3);
                    if (!cgrp.Any()) continue;
                    var maxr = cgrp.Max(ng => ng);
                    if (maxr == 0) continue;
                    maxr = maxr > 3 ? 2 : maxr - 1;
                    var maxfac = maxr == 2 ? 2 : 1;
                    drawingContext.DrawRectangle(orbitBrush[maxr], null,
                        new Rect(
                            skillNodeGroup.Position -
                            new Vector2D(iscr[maxr].PixelWidth * 1.25, iscr[maxr].PixelHeight * 1.25 * maxfac),
                            new Size(iscr[maxr].PixelWidth * 2.5, iscr[maxr].PixelHeight * 2.5 * maxfac)));
                }
            }
        }

        private void DrawConnection(DrawingContext dc, Pen pen2, SkillNode n1, SkillNode n2)
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
                dc.DrawLine(pen2, n1.Position, n2.Position);
            }
        }

        public void DrawFaces()
        {
            using (DrawingContext dc = picFaces.RenderOpen())
            {
                for (int i = 0; i < CharName.Count; i++)
                {
                    string s = CharName[i];
                    Vector2D pos = Skillnodes.First(nd => nd.Value.Name.ToUpper() == s.ToUpper()).Value.Position;
                    dc.DrawRectangle(StartBackgrounds[false].Value, null,
                        new Rect(
                            pos - new Vector2D(StartBackgrounds[false].Key.Width, StartBackgrounds[false].Key.Height),
                            pos + new Vector2D(StartBackgrounds[false].Key.Width, StartBackgrounds[false].Key.Height)));
                    if (_chartype == i)
                    {
                        dc.DrawRectangle(FacesBrush[i].Value, null,
                            new Rect(pos - new Vector2D(FacesBrush[i].Key.Width, FacesBrush[i].Key.Height),
                                pos + new Vector2D(FacesBrush[i].Key.Width, FacesBrush[i].Key.Height)));

                        var charBaseAttr = CharBaseAttributes[Chartype];

                        var text = CreateAttributeText(charBaseAttr["+# to Intelligence"].ToString(), Brushes.DodgerBlue);
                        dc.DrawText(text, pos - new Vector2D(19, 117));

                        text = CreateAttributeText(charBaseAttr["+# to Strength"].ToString(), Brushes.IndianRed);
                        dc.DrawText(text, pos - new Vector2D(102, -32));

                        text = CreateAttributeText(charBaseAttr["+# to Dexterity"].ToString(), Brushes.MediumSeaGreen);
                        dc.DrawText(text, pos - new Vector2D(-69, -32));

                    }
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
                30, colorBrush);
        }

        public void DrawHighlights(NodeHighlighter nh)
        {
            var hpen = new Pen(Brushes.White, 20);
            var crossPen = new Pen(Brushes.Red, 20);
            using (DrawingContext dc = picHighlights.RenderOpen())
            {
                foreach (var pair in nh.nodeHighlights)
                {
                    // TODO: Make more elegant? Needs profiling.
                    HighlightState hs = pair.Value;

                    if (hs != HighlightState.Crossed)
                    {
                        byte red = (byte)(hs.HasFlag(HighlightState.FromSearch) ? 255 : 0);
                        byte green = (byte)(hs.HasFlag(HighlightState.FromAttrib) ? 255 : 0);
                        byte blue = (byte)(hs.HasFlag(HighlightState.FromNode) ? 255 : 0);
                        hpen = new Pen(new SolidColorBrush(Color.FromRgb(red, green, blue)), 20);

                        dc.DrawEllipse(null, hpen, pair.Key.Position, 80, 80);
                    }

                    if (hs.HasFlag(HighlightState.Crossed))
                    {
                        // Crossed nodes get highlighted with two crossing lines.
                        var x = pair.Key.Position.X;
                        var y = pair.Key.Position.Y;
                        dc.DrawLine(crossPen, new Point(x + 50, y + 70), new Point(x - 50, y - 70));
                        dc.DrawLine(crossPen, new Point(x + 50, y - 70), new Point(x - 50, y + 70));
                    }
                }
            }
        }

        private void DrawLinkBackgroundLayer(List<ushort[]> links)
        {
            picLinks = new DrawingVisual();
            var pen2 = new Pen(Brushes.DarkSlateGray, 20f);
            using (DrawingContext dc = picLinks.RenderOpen())
            {
                foreach (var nid in links)
                {
                    SkillNode n1 = Skillnodes[nid[0]];
                    SkillNode n2 = Skillnodes[nid[1]];
                    DrawConnection(dc, pen2, n1, n2);
                    //if (n1.SkillNodeGroup == n2.SkillNodeGroup && n1.orbit == n2.orbit)
                    //{
                    //    if (n1.Arc - n2.Arc > 0 && n1.Arc - n2.Arc < Math.PI || n1.Arc - n2.Arc < -Math.PI)
                    //    {
                    //        dc.DrawArc(null, pen2, n1.Position, n2.Position,
                    //                   new Size(SkillTree.SkillNode.orbitRadii[n1.orbit],
                    //                            SkillTree.SkillNode.orbitRadii[n1.orbit]));
                    //    }
                    //    else
                    //    {
                    //        dc.DrawArc(null, pen2, n2.Position, n1.Position,
                    //                   new Size(SkillTree.SkillNode.orbitRadii[n1.orbit],
                    //                            SkillTree.SkillNode.orbitRadii[n1.orbit]));
                    //    }
                    //}
                    //else
                    //{
                    //    dc.DrawLine(pen2, n1.Position, n2.Position);
                    //}
                }
            }
        }


        internal void DrawNodeBaseSurroundHighlight()
        {
            float factor = 1.2f;
            using (DrawingContext dc = picSkillSurroundHighlight.RenderOpen())
            {
                if (HighlightedNodes != null)
                {
                    foreach (ushort skillNode in HighlightedNodes)
                    {
                        Vector2D pos = (Skillnodes[skillNode].Position);

                        if (Skillnodes[skillNode].IsNotable)
                        {
                            dc.DrawRectangle(NodeSurroundHighlightBrush[3].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[3].Key.Width * factor,
                                    (int)pos.Y - NodeSurroundBrush[3].Key.Height * factor,
                                    NodeSurroundBrush[3].Key.Width * 2 * factor,
                                    NodeSurroundBrush[3].Key.Height * 2 * factor));
                        }
                        else if (Skillnodes[skillNode].IsKeyStone)
                        {
                            dc.DrawRectangle(NodeSurroundHighlightBrush[2].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[2].Key.Width * factor,
                                    (int)pos.Y - NodeSurroundBrush[2].Key.Height * factor,
                                    NodeSurroundBrush[2].Key.Width * 2 * factor,
                                    NodeSurroundBrush[2].Key.Height * 2 * factor));
                        }
                        else if (Skillnodes[skillNode].IsMastery)
                        {
                            //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                        }
                        else if (Skillnodes[skillNode].IsJewelSocket)
                        {
                            dc.DrawRectangle(NodeSurroundHighlightBrush[6].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[6].Key.Width * factor,
                                    (int)pos.Y - NodeSurroundBrush[6].Key.Height * factor,
                                    NodeSurroundBrush[6].Key.Width * 2 * factor,
                                    NodeSurroundBrush[6].Key.Height * 2 * factor));
                        }
                        else
                            dc.DrawRectangle(NodeSurroundHighlightBrush[0].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[0].Key.Width * factor,
                                    (int)pos.Y - NodeSurroundBrush[0].Key.Height * factor,
                                    NodeSurroundBrush[0].Key.Width * 2 * factor,
                                    NodeSurroundBrush[0].Key.Height * 2 * factor));
                    }
                }
            }

            var pen2 = new Pen(new SolidColorBrush(_treeComparisonColor), 25 * factor);
            using (DrawingContext dc = picPathHighlight.RenderOpen())
            {
                if (HighlightedNodes != null)
                {
                    foreach (ushort n1 in HighlightedNodes)
                    {

                        foreach (SkillNode n2 in Skillnodes[n1].Neighbor)
                        {
                            if (HighlightedNodes.Contains(n2.Id))
                            {
                                DrawConnection(dc, pen2, n2, Skillnodes[n1]);
                            }
                        }
                    }
                }
            }
        }

        private void DrawNodeBaseSurround()
        {
            using (DrawingContext dc = picSkillBaseSurround.RenderOpen())
            {
                foreach (ushort skillNode in Skillnodes.Keys)
                {
                    Vector2D pos = (Skillnodes[skillNode].Position);

                    if (Skillnodes[skillNode].IsNotable)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[3].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[3].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[3].Key.Height,
                                NodeSurroundBrush[3].Key.Width * 2,
                                NodeSurroundBrush[3].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].IsKeyStone)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[2].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[2].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[2].Key.Height,
                                NodeSurroundBrush[2].Key.Width * 2,
                                NodeSurroundBrush[2].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].IsMastery)
                    {
                        //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                    }
                    else if (Skillnodes[skillNode].IsJewelSocket)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[6].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[6].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[6].Key.Height,
                                NodeSurroundBrush[6].Key.Width * 2,
                                NodeSurroundBrush[6].Key.Height * 2));
                    }
                    else
                        dc.DrawRectangle(NodeSurroundBrush[0].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[0].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[0].Key.Height,
                                NodeSurroundBrush[0].Key.Width * 2,
                                NodeSurroundBrush[0].Key.Height * 2));
                }
            }
        }

        private void DrawNodeSurround()
        {
            using (DrawingContext dc = picSkillSurround.RenderOpen())
            {
                foreach (ushort skillNode in SkilledNodes)
                {
                    Vector2D pos = (Skillnodes[skillNode].Position);

                    if (Skillnodes[skillNode].IsNotable)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[5].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[5].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[5].Key.Height,
                                NodeSurroundBrush[5].Key.Width * 2,
                                NodeSurroundBrush[5].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].IsKeyStone)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[4].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[4].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[4].Key.Height,
                                NodeSurroundBrush[4].Key.Width * 2,
                                NodeSurroundBrush[4].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].IsMastery)
                    {
                        //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                    }
                    else if (Skillnodes[skillNode].IsJewelSocket)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[7].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[7].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[7].Key.Height,
                                NodeSurroundBrush[7].Key.Width * 2,
                                NodeSurroundBrush[7].Key.Height * 2));
                    }
                    else
                        dc.DrawRectangle(NodeSurroundBrush[1].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[1].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[1].Key.Height,
                                NodeSurroundBrush[1].Key.Width * 2,
                                NodeSurroundBrush[1].Key.Height * 2));
                }
            }
        }

        public void DrawPath(List<ushort> path)
        {
            var pen2 = new Pen(Brushes.LawnGreen, 15f);
            pen2.DashStyle = new DashStyle(new DoubleCollection { 2 }, 2);

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                for (int i = -1; i < path.Count - 1; i++)
                {
                    SkillNode n1 = i == -1
                        ? Skillnodes[path[i + 1]].Neighbor.First(sn => SkilledNodes.Contains(sn.Id))
                        : Skillnodes[path[i]];
                    SkillNode n2 = Skillnodes[path[i + 1]];

                    DrawConnection(dc, pen2, n1, n2);
                }
            }
        }

        public void DrawRefundPreview(HashSet<ushort> nodes)
        {
            var pen2 = new Pen(Brushes.Red, 15f);
            pen2.DashStyle = new DashStyle(new DoubleCollection { 2 }, 2);

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                foreach (ushort node in nodes)
                {
                    foreach (SkillNode n2 in Skillnodes[node].Neighbor)
                    {
                        if (SkilledNodes.Contains(n2.Id) && (node < n2.Id || !(nodes.Contains(n2.Id))))
                            DrawConnection(dc, pen2, Skillnodes[node], n2);
                    }
                }
            }
        }

        private void InitSkillIconLayers()
        {
            picActiveSkillIconLayer = new DrawingVisual();
            picSkillIconLayer = new DrawingVisual();
        }

        private void DrawSkillIconLayer()
        {
            var pen = new Pen(Brushes.Black, 5);

            Geometry g = new RectangleGeometry(TRect);
            using (DrawingContext dc = picSkillIconLayer.RenderOpen())
            {
                dc.DrawGeometry(null, pen, g);
                foreach (var skillNode in Skillnodes)
                {
                    Size isize;
                    var imageBrush = new ImageBrush();
                    var rect = IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Key;
                    var bitmapImage = IconInActiveSkills.Images[IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Value];
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;

                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    Vector2D pos = (skillNode.Value.Position);
                    dc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }
            }
        }

        private void DrawActiveNodeIcons()
        {
            var pen = new Pen(Brushes.Black, 5);

            Geometry g = new RectangleGeometry(TRect);
            using (DrawingContext dc = picActiveSkillIconLayer.RenderOpen())
            {
                dc.DrawGeometry(null, pen, g);
                foreach (var skillNode in SkilledNodes)
                {
                    Size isize;
                    var imageBrush = new ImageBrush();
                    var rect = IconActiveSkills.SkillPositions[Skillnodes[skillNode].IconKey].Key;
                    var bitmapImage = IconActiveSkills.Images[IconActiveSkills.SkillPositions[Skillnodes[skillNode].IconKey].Value];
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;

                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    Vector2D pos = (Skillnodes[skillNode].Position);
                    dc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }
            }
        }

        private void InitFaceBrushesAndLayer()
        {
            picFaces = new DrawingVisual();

            if (!_Initialized)
            {
                foreach (string faceName in FaceNames)
                {
                    var bi = ImageHelper.OnLoadBitmapImage(new Uri(SkillTree.AssetsFolderPath + faceName + ".png", UriKind.Absolute));
                    _FacesBrush.Add(new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                        new ImageBrush(bi)));
                }

                var bi2 = ImageHelper.OnLoadBitmapImage(new Uri(SkillTree.AssetsFolderPath + "PSStartNodeBackgroundInactive.png", UriKind.Absolute));
                if (_StartBackgrounds.ContainsKey(false))
                {
                    if (!_StartBackgrounds[false].Key.Equals(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight)))
                    {
                        _StartBackgrounds.Add(false,
                            (new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                                new ImageBrush(bi2))));
                    }
                }
                else
                {
                    _StartBackgrounds.Add(false,
                            (new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                                new ImageBrush(bi2))));
                }
            }

        }

        private void InitNodeSurround()
        {
            picSkillSurround = new DrawingVisual();
            picSkillBaseSurround = new DrawingVisual();

            if (!_Initialized)
            {
                Size sizeNot;
                var brNot = new ImageBrush();
                brNot.Stretch = Stretch.Uniform;
                BitmapImage PImageNot = _assets[NodeBackgrounds["notable"]].PImage;
                brNot.ImageSource = PImageNot;
                sizeNot = new Size(PImageNot.PixelWidth, PImageNot.PixelHeight);

                var brNotH = new ImageBrush();
                brNotH.Stretch = Stretch.Uniform;
                BitmapImage PImageNotH = _assets[NodeBackgroundsActive["notable"]].PImage;
                brNotH.ImageSource = PImageNotH;
                Size sizeNotH = new Size(PImageNotH.PixelWidth, PImageNotH.PixelHeight);


                var brKS = new ImageBrush();
                brKS.Stretch = Stretch.Uniform;
                BitmapImage PImageKr = _assets[NodeBackgrounds["keystone"]].PImage;
                brKS.ImageSource = PImageKr;
                Size sizeKs = new Size(PImageKr.PixelWidth, PImageKr.PixelHeight);

                var brKSH = new ImageBrush();
                brKSH.Stretch = Stretch.Uniform;
                BitmapImage PImageKrH = _assets[NodeBackgroundsActive["keystone"]].PImage;
                brKSH.ImageSource = PImageKrH;
                Size sizeKsH = new Size(PImageKrH.PixelWidth, PImageKrH.PixelHeight);

                var brNorm = new ImageBrush();
                brNorm.Stretch = Stretch.Uniform;
                BitmapImage PImageNorm = _assets[NodeBackgrounds["normal"]].PImage;
                brNorm.ImageSource = PImageNorm;
                Size isizeNorm = new Size(PImageNorm.PixelWidth, PImageNorm.PixelHeight);

                var brNormA = new ImageBrush();
                brNormA.Stretch = Stretch.Uniform;
                BitmapImage PImageNormA = _assets[NodeBackgroundsActive["normal"]].PImage;
                brNormA.ImageSource = PImageNormA;
                Size isizeNormA = new Size(PImageNormA.PixelWidth, PImageNormA.PixelHeight);

                var brJewel = new ImageBrush();
                brJewel.Stretch = Stretch.Uniform;
                BitmapImage PImageJewel = _assets[NodeBackgrounds["jewel"]].PImage;
                brJewel.ImageSource = PImageJewel;
                Size isSizeJewel = new Size(PImageJewel.PixelWidth, PImageJewel.PixelHeight);

                var brJewelA = new ImageBrush();
                brJewelA.Stretch = Stretch.Uniform;
                BitmapImage PImageJewelA = _assets[NodeBackgroundsActive["jewel"]].PImage;
                brJewelA.ImageSource = PImageJewelA;
                Size isSizeJewelA = new Size(PImageJewelA.PixelWidth, PImageJewelA.PixelHeight);

                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isizeNorm, brNorm));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isizeNormA, brNormA));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeKs, brKS));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeNot, brNot));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeKsH, brKSH));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeNotH, brNotH));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isSizeJewel, brJewel));
                NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isSizeJewelA, brJewelA));




                //outline generator
                foreach (var item in NodeSurroundBrush)
                {
                    var outlinecolor = _treeComparisonColor;
                    uint omask = (uint)outlinecolor.B | (uint)outlinecolor.G << 8 | (uint)outlinecolor.R << 16;

                    var bitmap = (BitmapImage)item.Value.ImageSource;
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

                        var ibr = new ImageBrush();
                        ibr.Stretch = Stretch.Uniform;
                        ibr.ImageSource = wb;

                        NodeSurroundHighlightBrush.Add(new KeyValuePair<Size, ImageBrush>(item.Key, ibr));
                    }
                    else
                    {
                        //throw??
                    }
                }
            }
        }

        private void InitOtherDynamicLayers()
        {
            picActiveLinks = new DrawingVisual();
            picPathOverlay = new DrawingVisual();
            picHighlights = new DrawingVisual();
            picSkillSurroundHighlight = new DrawingVisual();
            picPathHighlight = new DrawingVisual();
            picJewelHighlight = new DrawingVisual();
        }

        public static void ClearAssets()
        {
            _Initialized = false;
        }

        public void DrawJewelHighlight(SkillNode node)
        {
            var radiusPen = new Pen(Brushes.Cyan, 10);
            
            using (DrawingContext dc = picJewelHighlight.RenderOpen())
            {
                dc.DrawEllipse(null, radiusPen, node.Position, 800, 800);
                dc.DrawEllipse(null, radiusPen, node.Position, 1200, 1200);
                dc.DrawEllipse(null, radiusPen, node.Position, 1500, 1500);
            }
        }
    }
}