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
using POESKillTree.Utils;
using POESKillTree.Model;
using System.Text.RegularExpressions;

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
        
        private readonly NodeHighlighter _nodeHighlighter = new NodeHighlighter();
        private readonly PersistentData _persistentData = App.PersistentData;
        private List<Tuple<int, Vector2D>> _originalPositions = new List<Tuple<int, Vector2D>>();
        public bool drawAscendancy = false;

        public DrawingVisual SkillTreeVisual;
        public DrawingVisual picBackground;
        public DrawingVisual picNodeTreeComparisonHighlight;
        public DrawingVisual picPathTreeComparisonHighlight;
        public DrawingVisual picLinks;
        public DrawingVisual picActiveLinks;
        public DrawingVisual picPathOverlay;
        public DrawingVisual picSkillIconLayer;
        public DrawingVisual picActiveSkillIconLayer;
        public DrawingVisual picNodeNormalSurround;
        public DrawingVisual picNodeHighlightSurround;
        public DrawingVisual picFaces;
        public DrawingVisual picHighlights;
        public DrawingVisual picJewelHighlight;

        public DrawingVisual AscSkilltreeVisual;
        public DrawingVisual picAscendancyClasses;
        public DrawingVisual picAscendancyButton;
        public DrawingVisual picAscNodeTreeComparisonHighlight;
        public DrawingVisual picAscPathTreeComparisonHighlight;
        public DrawingVisual picAscLinks;
        public DrawingVisual picAscActiveLinks;
        public DrawingVisual picAscPathOverlay;
        public DrawingVisual picAscSkillIconLayer;
        public DrawingVisual picAscActiveSkillIconLayer;
        public DrawingVisual picAscNodeNormalSurround;
        public DrawingVisual picAscNodeHighlightSurround;


        public void CreateCombineVisual()
        {
            SkillTreeVisual = new DrawingVisual();
            AscSkilltreeVisual = new DrawingVisual();
            SkillTreeVisual.Children.Add(picBackground);
            SkillTreeVisual.Children.Add(picPathTreeComparisonHighlight);
            SkillTreeVisual.Children.Add(picNodeTreeComparisonHighlight);
            SkillTreeVisual.Children.Add(picLinks);
            SkillTreeVisual.Children.Add(picActiveLinks);
            SkillTreeVisual.Children.Add(picPathOverlay);
            SkillTreeVisual.Children.Add(picSkillIconLayer);
            SkillTreeVisual.Children.Add(picActiveSkillIconLayer);
            SkillTreeVisual.Children.Add(picNodeNormalSurround);
            SkillTreeVisual.Children.Add(picNodeHighlightSurround);
            SkillTreeVisual.Children.Add(picFaces);

            //Draw Ascendancy stuff on top of the normal tree
            AscSkilltreeVisual.Children.Add(picAscendancyClasses);
            AscSkilltreeVisual.Children.Add(picAscPathTreeComparisonHighlight);
            AscSkilltreeVisual.Children.Add(picAscNodeTreeComparisonHighlight);
            AscSkilltreeVisual.Children.Add(picAscLinks);
            AscSkilltreeVisual.Children.Add(picAscActiveLinks);
            AscSkilltreeVisual.Children.Add(picAscPathOverlay);
            AscSkilltreeVisual.Children.Add(picAscSkillIconLayer);
            AscSkilltreeVisual.Children.Add(picAscActiveSkillIconLayer);
            AscSkilltreeVisual.Children.Add(picAscNodeNormalSurround);
            AscSkilltreeVisual.Children.Add(picAscNodeHighlightSurround);
            SkillTreeVisual.Children.Add(AscSkilltreeVisual);

            //These need to drawn on top of all other elements
            SkillTreeVisual.Children.Add(picAscendancyButton);
            SkillTreeVisual.Children.Add(picHighlights);
            SkillTreeVisual.Children.Add(picJewelHighlight);
        }

        #endregion

        public void ClearPath()
        {
            picPathOverlay.RenderOpen().Close();
            picAscPathOverlay.RenderOpen().Close();
        }

        public void ClearJewelHighlight()
        {
            picJewelHighlight.RenderOpen().Close();
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
                var nodeList = Skillnodes.Where(x => x.Value.ascendancyName == AscendancyClasses.GetClassName(className, AscType) && !x.Value.IsAscendancyStart);
                var worldPos = Skillnodes.First(x => x.Value.Name.ToUpperInvariant() == CharName[_chartype]).Value.Position;
                var ascStartNode = Skillnodes.First(x => x.Value.ascendancyName == AscendancyClasses.GetClassName(className, AscType) && x.Value.IsAscendancyStart).Value;
                var ascNodeOriginalPos = ascStartNode.SkillNodeGroup.Position;
                if (!_originalPositions.Any(x => x.Item1 == ascStartNode.G))
                    _originalPositions.Add(new Tuple<int, Vector2D>(ascStartNode.G, new Vector2D(ascNodeOriginalPos.ToContainingPoint())));

                var imageName = "Classes" + ascStartNode.ascendancyName;
                var bitmap = _assets[imageName].PImage;

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
                var imageCX = worldPos.X + (distanceFromStartNodeCenter + bitmap.Height * 1.25) * Math.Cos(ascButtonRot + Math.PI / 2);
                var imageCY = worldPos.Y + (distanceFromStartNodeCenter + bitmap.Width * 1.25) * Math.Sin(ascButtonRot + Math.PI / 2);

                ascStartNode.SkillNodeGroup.Position = new Vector2D(imageCX, imageCY);
                var done = new List<SkillNodeGroup> { ascStartNode.SkillNodeGroup };
                
                foreach (var n in nodeList)
                {
                    if (done.Contains(n.Value.SkillNodeGroup))
                        continue;
                    if (!_originalPositions.Any(x => x.Item1 == n.Value.G))
                        _originalPositions.Add(new Tuple<int, Vector2D>(n.Value.G, new Vector2D(n.Value.SkillNodeGroup.Position.ToContainingPoint())));
                    var diffDist = ascNodeOriginalPos - n.Value.SkillNodeGroup.Position;
                    distToCentre = Math.Sqrt(worldPos.X * worldPos.X + worldPos.Y * worldPos.Y);
                    isCentered = Math.Abs(worldPos.X) < 10.0 && Math.Abs(worldPos.Y) < 10.0;
                    if (!isCentered)
                    {
                        dirX = worldPos.X / distToCentre;
                        dirY = -worldPos.Y / distToCentre;
                    }
                    ascButtonRot = Math.Atan2(dirX, dirY);
                    imageCX = ascStartNode.SkillNodeGroup.Position.X - diffDist.X;
                    imageCY = ascStartNode.SkillNodeGroup.Position.Y - diffDist.Y;

                    n.Value.SkillNodeGroup.Position = new Vector2D(imageCX, imageCY);
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
                    6 * backgroundBrush.ImageSource.Height / TRect.Height);
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
                    if (skillNodeGroup.Nodes.Where(n => n.ascendancyName != null).ToArray().Length > 0)
                        continue;
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
                drawingContext.Close();
            }
        }

        private void DrawConnection(DrawingContext dc, Pen pen2, SkillNode n1, SkillNode n2)
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
                    if (n1.IsMastery || n2.IsMastery)
                        draw = false;
                    if (draw)
                        dc.DrawLine(pen2, n1.Position, n2.Position);
                }
            }
        }

        public void DrawFaces()
        {
            using (DrawingContext dc = picFaces.RenderOpen())
            {
                for (int i = 0; i < CharName.Count; i++)
                {
                    string s = CharName[i];
                    Vector2D pos = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == s).Value.Position;
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
                dc.Close();
            }
        }

        public void DrawAscendancyClasses()
        {
            using (DrawingContext dc = picAscendancyClasses.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dc.Close();
                    return;
                }
                foreach (var node in Skillnodes)
                {
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    if (node.Value.IsAscendancyStart && (_persistentData.Options.ShowAllAscendancyClasses || node.Value.ascendancyName == AscendancyClasses.GetClassName(className, AscType)))
                    {
                        var imageName = "Classes" + node.Value.ascendancyName;
                        var bitmap = _assets[imageName].PImage; 
                        var brush = new ImageBrush(_assets[imageName].PImage);
                        var pos = node.Value.Position;
                        dc.DrawRectangle(brush, null,
                            new Rect(
                                pos -
                                new Vector2D(bitmap.PixelWidth * 1.25, bitmap.PixelHeight * 1.25),
                                new Size(bitmap.PixelWidth * 2.5, bitmap.PixelHeight * 2.5)));
                        var currentClass = AscendancyClasses.GetClass(node.Value.ascendancyName);
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
            using (DrawingContext dc = picAscendancyButton.RenderOpen())
            {
                if (AscType != 0 && !_persistentData.Options.ShowAllAscendancyClasses)
                {
                    foreach (var i in rootNodeList)
                    {
                        if (!SkilledNodes.Contains((ushort)i))
                            continue;
                        var x = SkilledNodes;
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
                            case "Normal":
                            default:
                                imageName = "PassiveSkillScreenAscendancyButton";
                                break;
                        }

                        var b = _assets[imageName].PImage;
                        var brush = new ImageBrush(_assets[imageName].PImage);

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
                        var buttonCX = worldPos.X + distanceFromStartNodeCenter * Math.Cos(ascButtonRot + Math.PI / 2);
                        var buttonCY = worldPos.Y + distanceFromStartNodeCenter * Math.Sin(ascButtonRot + Math.PI / 2);
                        var buttonPoint = new Vector2D(buttonCX, buttonCY);

                        var rect = new Rect(buttonCX - b.Height * 1.75, buttonCY - b.Width * 1.75, b.Width * 2.5, b.Height * 2.5);
                        dc.PushTransform(new RotateTransform(ascButtonRot * (180 / Math.PI), buttonCX, buttonCY));
                        dc.DrawRectangle(brush, null, rect);

                        ascedancyButtonPos = buttonPoint;
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
            using (DrawingContext dc = picHighlights.RenderOpen())
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
            picLinks = new DrawingVisual();
            var pen2 = new Pen(Brushes.DarkSlateGray, 20f);
            using (DrawingContext dc = picLinks.RenderOpen())
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
            using (DrawingContext dcAsc = picAscLinks.RenderOpen())
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
                    if (_persistentData.Options.ShowAllAscendancyClasses || (n1.ascendancyName == AscendancyClasses.GetClassName(className, AscType) && n2.ascendancyName == AscendancyClasses.GetClassName(className, AscType)))
                        DrawConnection(dcAsc, pen2, n1, n2);
                }
                dcAsc.Close();
            }
        }

        internal void DrawTreeComparisonHighlight()
        {
            const float factor = 1.2f;
            var className = CharacterNames.GetClassNameFromChartype(_chartype);
            var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
            using (DrawingContext dc = picNodeTreeComparisonHighlight.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscNodeTreeComparisonHighlight.RenderOpen())
                {
                    if (HighlightedNodes != null)
                    {
                        foreach (ushort skillNode in HighlightedNodes)
                        {
                            var pos = Skillnodes[skillNode].Position;
                            if (Skillnodes[skillNode].IsAscendancyStart)
                            {
                                //already drawn, but needs to be here to prevent highlighting
                            }
                            else if (Skillnodes[skillNode].ascendancyName != null && Skillnodes[skillNode].IsNotable)
                            {
                                if (!drawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                                    dcAsc.DrawRectangle(NodeSurroundHighlightBrush[5].Value, null,
                                        new Rect((int)pos.X - NodeSurroundHighlightBrush[5].Key.Width * .875 * factor,
                                            (int)pos.Y - NodeSurroundHighlightBrush[5].Key.Height * .875 * factor,
                                            NodeSurroundHighlightBrush[5].Key.Width * 1.75 * factor,
                                            NodeSurroundHighlightBrush[5].Key.Height * 1.75 * factor));
                            }
                            else if (Skillnodes[skillNode].ascendancyName != null)
                            {
                                if (!drawAscendancy) continue;

                                if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                                    dcAsc.DrawRectangle(NodeSurroundHighlightBrush[4].Value, null,
                                        new Rect((int)pos.X - NodeSurroundHighlightBrush[4].Key.Width * factor,
                                            (int)pos.Y - NodeSurroundHighlightBrush[4].Key.Height * factor,
                                            NodeSurroundHighlightBrush[4].Key.Width * 2 * factor,
                                            NodeSurroundHighlightBrush[4].Key.Height * 2 * factor));
                            }
                            else if (Skillnodes[skillNode].IsNotable)
                            {
                                dc.DrawRectangle(NodeSurroundHighlightBrush[1].Value, null,
                                    new Rect((int)pos.X - NodeSurroundHighlightBrush[1].Key.Width * factor,
                                        (int)pos.Y - NodeSurroundHighlightBrush[1].Key.Height * factor,
                                        NodeSurroundHighlightBrush[1].Key.Width * 2 * factor,
                                        NodeSurroundHighlightBrush[1].Key.Height * 2 * factor));
                            }
                            else if (Skillnodes[skillNode].IsKeyStone)
                            {
                                dc.DrawRectangle(NodeSurroundHighlightBrush[2].Value, null,
                                    new Rect((int)pos.X - NodeSurroundHighlightBrush[2].Key.Width * factor,
                                        (int)pos.Y - NodeSurroundHighlightBrush[2].Key.Height * factor,
                                        NodeSurroundHighlightBrush[2].Key.Width * 2 * factor,
                                        NodeSurroundHighlightBrush[2].Key.Height * 2 * factor));
                            }
                            else if (Skillnodes[skillNode].IsMastery)
                            {
                                //Needs to be here so that "Masteries" (Middle images of nodes) don't get anything drawn around them.
                            }
                            else if (Skillnodes[skillNode].IsJewelSocket)
                            {
                                dc.DrawRectangle(NodeSurroundHighlightBrush[3].Value, null,
                                    new Rect((int)pos.X - NodeSurroundHighlightBrush[3].Key.Width * factor,
                                        (int)pos.Y - NodeSurroundHighlightBrush[3].Key.Height * factor,
                                        NodeSurroundHighlightBrush[3].Key.Width * 2 * factor,
                                        NodeSurroundHighlightBrush[3].Key.Height * 2 * factor));
                            }
                            else
                                dc.DrawRectangle(NodeSurroundHighlightBrush[0].Value, null,
                                    new Rect((int)pos.X - NodeSurroundHighlightBrush[0].Key.Width * factor,
                                        (int)pos.Y - NodeSurroundHighlightBrush[0].Key.Height * factor,
                                        NodeSurroundHighlightBrush[0].Key.Width * 2 * factor,
                                        NodeSurroundHighlightBrush[0].Key.Height * 2 * factor));
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }

            var pen2 = new Pen(new SolidColorBrush(_treeComparisonColor), 25 * factor);
            using (DrawingContext dc = picPathTreeComparisonHighlight.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscPathTreeComparisonHighlight.RenderOpen())
                {
                    if (HighlightedNodes != null)
                    {
                        foreach (ushort n1 in HighlightedNodes)
                        {
                            foreach (SkillNode n2 in Skillnodes[n1].VisibleNeighbors)
                            {
                                if (!HighlightedNodes.Contains(n2.Id)) continue;

                                if (n2.ascendancyName != null && Skillnodes[n1].ascendancyName != null)
                                {
                                    if (!drawAscendancy) continue;

                                    if (_persistentData.Options.ShowAllAscendancyClasses || (Skillnodes[n1].ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
                                        DrawConnection(dcAsc, pen2, n2, Skillnodes[n1]);
                                }
                                else
                                    DrawConnection(dc, pen2, n2, Skillnodes[n1]);
                            }
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
            }
        }

        private void DrawNodeNormalSurround()
        {
            using (DrawingContext dc = picNodeNormalSurround.RenderOpen())
            {
                foreach (ushort skillNode in Skillnodes.Keys)
                {
                    var pos = (Skillnodes[skillNode].Position);
                    if (Skillnodes[skillNode].IsAscendancyStart || Skillnodes[skillNode].ascendancyName != null)
                    {
                        //Needs to be here so that Ascendancy nodes don't get anything drawn around them.
                    }
                    else if (Skillnodes[skillNode].IsNotable)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[2].Value, null,
                            new Rect((int)pos.X - NodeSurroundBrush[2].Key.Width,
                                (int)pos.Y - NodeSurroundBrush[2].Key.Height,
                                NodeSurroundBrush[2].Key.Width * 2,
                                NodeSurroundBrush[2].Key.Height * 2));
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
                dc.Close();
            }
        }

        private void DrawAscendancyNodeNormalSurround()
        {
            using (DrawingContext dcAsc = picAscNodeNormalSurround.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
                foreach (ushort skillNode in Skillnodes.Keys)
                {
                    var pos = (Skillnodes[skillNode].Position);
                    if (Skillnodes[skillNode].IsAscendancyStart)
                    {
                        if (!drawAscendancy) continue;

                        const string imageName = "PassiveSkillScreenAscendancyMiddle";
                        var bitmap = _assets[imageName].PImage;
                        var brush = new ImageBrush(_assets[imageName].PImage);

                        if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                            dcAsc.DrawRectangle(brush, null,
                                new Rect(
                                    pos -
                                    new Vector2D(bitmap.PixelWidth, bitmap.PixelHeight),
                                    new Size(bitmap.PixelWidth * 2, bitmap.PixelHeight * 2)));
                    }
                    else if (Skillnodes[skillNode].ascendancyName != null && Skillnodes[skillNode].IsNotable)
                    {
                        if (!drawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                            dcAsc.DrawRectangle(NodeSurroundBrush[10].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[10].Key.Width * .875,
                                    (int)pos.Y - NodeSurroundBrush[10].Key.Height * .875,
                                    NodeSurroundBrush[10].Key.Width * 1.75,
                                    NodeSurroundBrush[10].Key.Height * 1.75));
                    }
                    else if (Skillnodes[skillNode].ascendancyName != null)
                    {
                        if (!drawAscendancy) continue;

                        if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                            dcAsc.DrawRectangle(NodeSurroundBrush[8].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[8].Key.Width,
                                    (int)pos.Y - NodeSurroundBrush[8].Key.Height,
                                    NodeSurroundBrush[8].Key.Width * 2,
                                    NodeSurroundBrush[8].Key.Height * 2));
                    }
                }
                dcAsc.Close();
            }
        }

        private void DrawNodeHighlightSurround()
        {
            using (DrawingContext dc = picNodeHighlightSurround.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscNodeHighlightSurround.RenderOpen())
                {
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
                    foreach (ushort skillNode in SkilledNodes)
                    {
                        var pos = (Skillnodes[skillNode].Position);
                        if (Skillnodes[skillNode].IsAscendancyStart)
                        {
                            //already drawn, but needs to be here to prevent highlighting
                        }
                        else if (Skillnodes[skillNode].ascendancyName != null && Skillnodes[skillNode].IsNotable)
                        {
                            if (!drawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                                dcAsc.DrawRectangle(NodeSurroundBrush[11].Value, null,
                                    new Rect((int)pos.X - NodeSurroundBrush[11].Key.Width * .875,
                                        (int)pos.Y - NodeSurroundBrush[11].Key.Height * .875,
                                        NodeSurroundBrush[11].Key.Width * 1.75,
                                        NodeSurroundBrush[11].Key.Height * 1.75));
                        }
                        else if (Skillnodes[skillNode].ascendancyName != null)
                        {
                            if (!drawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                                dcAsc.DrawRectangle(NodeSurroundBrush[9].Value, null,
                                    new Rect((int)pos.X - NodeSurroundBrush[9].Key.Width,
                                        (int)pos.Y - NodeSurroundBrush[9].Key.Height,
                                        NodeSurroundBrush[9].Key.Width * 2,
                                        NodeSurroundBrush[9].Key.Height * 2));
                        }
                        else if (Skillnodes[skillNode].IsNotable)
                        {
                            dc.DrawRectangle(NodeSurroundBrush[3].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[3].Key.Width,
                                    (int)pos.Y - NodeSurroundBrush[3].Key.Height,
                                    NodeSurroundBrush[3].Key.Width * 2,
                                    NodeSurroundBrush[3].Key.Height * 2));
                        }
                        else if (Skillnodes[skillNode].IsKeyStone)
                        {
                            dc.DrawRectangle(NodeSurroundBrush[5].Value, null,
                                new Rect((int)pos.X - NodeSurroundBrush[5].Key.Width,
                                    (int)pos.Y - NodeSurroundBrush[5].Key.Height,
                                    NodeSurroundBrush[5].Key.Width * 2,
                                    NodeSurroundBrush[5].Key.Height * 2));
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
                    dcAsc.Close();
                }
                dc.Close();
            }
        }

        public void DrawPath(List<ushort> path)
        {
            var pen2 = new Pen(Brushes.LawnGreen, 15f);
            pen2.DashStyle = new DashStyle(new DoubleCollection { 2 }, 2);

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscPathOverlay.RenderOpen())
                {
                    // Draw a connection from a skilled node to the first path node.
                    var skilledNeighbors = new List<SkillNode>();
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);

                    if (path.Count > 0)
                        skilledNeighbors = Skillnodes[path[0]].VisibleNeighbors.Where(sn => SkilledNodes.Contains(sn.Id)).ToList();
                    // The node might not be connected to a skilled node through visible neighbors
                    // in which case we don't want to draw a connection anyway.
                    if (skilledNeighbors.Any())
                    {
                        if (skilledNeighbors.First().ascendancyName != null && Skillnodes[path[0]] != null)
                        {
                            if (drawAscendancy)
                            {
                                if (_persistentData.Options.ShowAllAscendancyClasses || (Skillnodes[path[0]].ascendancyName == ascendancyClassName && skilledNeighbors.First().ascendancyName == ascendancyClassName))
                                    DrawConnection(dcAsc, pen2, skilledNeighbors.First(), Skillnodes[path[0]]);
                            }
                        }
                        else
                            DrawConnection(dc, pen2, skilledNeighbors.First(), Skillnodes[path[0]]);
                    }

                    // Draw connections for the path itself (only those that should be visible).
                    for (var i = 0; i < path.Count - 1; i++)
                    {
                        var n1 = Skillnodes[path[i]];
                        var n2 = Skillnodes[path[i + 1]];
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

        public void DrawRefundPreview(HashSet<ushort> nodes)
        {
            var pen2 = new Pen(Brushes.Red, 15f) {DashStyle = new DashStyle(new DoubleCollection {2}, 2)};

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscPathOverlay.RenderOpen())
                {
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
                    foreach (ushort node in nodes)
                    {
                        foreach (SkillNode n2 in Skillnodes[node].VisibleNeighbors)
                        {
                            if (SkilledNodes.Contains(n2.Id) && (node < n2.Id || !(nodes.Contains(n2.Id))))
                            {
                                if (Skillnodes[node].ascendancyName != null && n2.ascendancyName != null)
                                {
                                    if (!drawAscendancy) continue;

                                    if (_persistentData.Options.ShowAllAscendancyClasses || (Skillnodes[node].ascendancyName == ascendancyClassName && n2.ascendancyName == ascendancyClassName))
                                        DrawConnection(dcAsc, pen2, Skillnodes[node], n2);
                                }
                                else
                                    DrawConnection(dc, pen2, Skillnodes[node], n2);
                            }
                        }
                    }
                    dcAsc.Close();
                }
                dc.Close();
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
                    if (skillNode.Value.ascendancyName != null) continue;

                    var imageBrush = new ImageBrush();
                    var rect = IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Key;
                    var bitmapImage = IconInActiveSkills.Images[IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Value];
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;

                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    var pos = (skillNode.Value.Position);
                    dc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }
            }
        }

        private void DrawAscendancySkillIconLayer()
        {
            var pen = new Pen(Brushes.Black, 5);
            Geometry g = new RectangleGeometry(TRect);
            using (DrawingContext dcAsc = picAscSkillIconLayer.RenderOpen())
            {
                if (!drawAscendancy)
                {
                    dcAsc.Close();
                    return;
                }

                dcAsc.DrawGeometry(null, pen, g);
                var className = CharacterNames.GetClassNameFromChartype(_chartype);
                var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
                foreach (var skillNode in Skillnodes.Where(x => x.Value.ascendancyName != null))
                {
                    var imageBrush = new ImageBrush();
                    var rect = IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Key;
                    var bitmapImage = IconInActiveSkills.Images[IconInActiveSkills.SkillPositions[skillNode.Value.IconKey].Value];
                    imageBrush.Stretch = Stretch.Uniform;
                    imageBrush.ImageSource = bitmapImage;
                    imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                        rect.Height / bitmapImage.PixelHeight);
                    var pos = (skillNode.Value.Position);

                    if (_persistentData.Options.ShowAllAscendancyClasses || skillNode.Value.ascendancyName == ascendancyClassName)
                        dcAsc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                }
                dcAsc.Close();
            }
        }

        private void DrawActiveNodeIcons()
        {
            var pen = new Pen(Brushes.Black, 5);

            Geometry g = new RectangleGeometry(TRect);
            using (DrawingContext dc = picActiveSkillIconLayer.RenderOpen())
            {
                using (DrawingContext dcAsc = picAscActiveSkillIconLayer.RenderOpen())
                {
                    dc.DrawGeometry(null, pen, g);
                    dcAsc.DrawGeometry(null, pen, g);
                    var className = CharacterNames.GetClassNameFromChartype(_chartype);
                    var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
                    foreach (var skillNode in SkilledNodes)
                    {
                        var imageBrush = new ImageBrush();
                        var rect = IconActiveSkills.SkillPositions[Skillnodes[skillNode].IconKey].Key;
                        var bitmapImage = IconActiveSkills.Images[IconActiveSkills.SkillPositions[Skillnodes[skillNode].IconKey].Value];
                        imageBrush.Stretch = Stretch.Uniform;
                        imageBrush.ImageSource = bitmapImage;

                        imageBrush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                        imageBrush.Viewbox = new Rect(rect.X / bitmapImage.PixelWidth, rect.Y / bitmapImage.PixelHeight, rect.Width / bitmapImage.PixelWidth,
                            rect.Height / bitmapImage.PixelHeight);
                        var pos = (Skillnodes[skillNode].Position);
                        if (Skillnodes[skillNode].ascendancyName != null)
                        {
                            if (!drawAscendancy) continue;

                            if (_persistentData.Options.ShowAllAscendancyClasses || Skillnodes[skillNode].ascendancyName == ascendancyClassName)
                                dcAsc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                        }
                        else
                            dc.DrawEllipse(imageBrush, null, pos, rect.Width, rect.Height);
                    }
                    dcAsc.Close();
                }
                dc.Close();
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
            picNodeHighlightSurround = new DrawingVisual();
            picNodeNormalSurround = new DrawingVisual();
            picAscNodeHighlightSurround = new DrawingVisual();
            picAscNodeNormalSurround = new DrawingVisual();

            if (!_Initialized)
            {
                foreach(var background in NodeBackgrounds)
                {
                    if (NodeBackgroundsActive.ContainsKey(background.Key))
                    {
                        Size normalSize;
                        var normalBrush = new ImageBrush();
                        normalBrush.Stretch = Stretch.Uniform;
                        BitmapImage normalBrushPImage = _assets[NodeBackgrounds[background.Key]].PImage;
                        normalBrush.ImageSource = normalBrushPImage;
                        normalSize = new Size(normalBrushPImage.PixelWidth, normalBrushPImage.PixelHeight);

                        Size activeSize;
                        var activeBrush = new ImageBrush();
                        activeBrush.Stretch = Stretch.Uniform;
                        BitmapImage activeBrushPImage = _assets[NodeBackgroundsActive[background.Key]].PImage;
                        activeBrush.ImageSource = activeBrushPImage;
                        activeSize = new Size(activeBrushPImage.PixelWidth, activeBrushPImage.PixelHeight);

                        NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(normalSize, normalBrush));
                        NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(activeSize, activeBrush));

                        //tree comparison highlight generator
                        var outlinecolor = _treeComparisonColor;
                        uint omask = (uint)outlinecolor.B | (uint)outlinecolor.G << 8 | (uint)outlinecolor.R << 16;

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

                            var ibr = new ImageBrush();
                            ibr.Stretch = Stretch.Uniform;
                            ibr.ImageSource = wb;

                            NodeSurroundHighlightBrush.Add(new KeyValuePair<Size, ImageBrush>(normalSize, ibr));
                        }
                        else
                        {
                            //throw??
                        }
                    }
                }
            }
        }

        private void InitializeLayers()
        {
            InitNodeSurround();
            InitSkillIconLayers();
            InitFaceBrushesAndLayer();
            
            picActiveLinks = new DrawingVisual();
            picPathOverlay = new DrawingVisual();
            picHighlights = new DrawingVisual();
            picNodeTreeComparisonHighlight = new DrawingVisual();
            picPathTreeComparisonHighlight = new DrawingVisual();
            picJewelHighlight = new DrawingVisual();
            picAscendancyButton = new DrawingVisual();
            picAscendancyClasses = new DrawingVisual();
            picAscLinks = new DrawingVisual();
            picAscActiveLinks = new DrawingVisual();
            picAscPathOverlay = new DrawingVisual();
            picAscSkillIconLayer = new DrawingVisual();
            picAscActiveSkillIconLayer = new DrawingVisual();
            picAscNodeTreeComparisonHighlight = new DrawingVisual();
            picAscPathTreeComparisonHighlight = new DrawingVisual();
        }

        private void DrawInitialLayers()
        {
            DrawNodeNormalSurround();
            DrawSkillIconLayer();
            DrawBackgroundLayer();
            DrawLinkBackgroundLayer(_links);
            DrawAscendancyNodeNormalSurround();
            DrawAscendancySkillIconLayer();
            DrawAscendancyLinkBackgroundLayer(_links);
            DrawAscendancyClasses();
            DrawDynamicLayers();
        }

        private void DrawDynamicLayers()
        {
            DrawActiveNodeIcons();
            DrawNodeHighlightSurround();
            DrawFaces();
        }

        /// <summary>
        /// Only draws what is needed for Ascendancy show/hide
        /// </summary>
        private void DrawAscendancyLayers()
        {
            if (drawAscendancy)
            {
                UpdateAscendancyClassPositions();
                DrawAscendancyNodeNormalSurround();
                DrawAscendancySkillIconLayer();
                DrawAscendancyLinkBackgroundLayer(_links);
                DrawAscendancyClasses();
                UpdateAvailNodes(true);
            }
            else
            {
                picAscendancyClasses.RenderOpen().Close();
                picAscPathTreeComparisonHighlight.RenderOpen().Close();
                picAscNodeTreeComparisonHighlight.RenderOpen().Close();
                picAscLinks.RenderOpen().Close();
                picAscActiveLinks.RenderOpen().Close();
                picAscPathOverlay.RenderOpen().Close();
                picAscSkillIconLayer.RenderOpen().Close();
                picAscActiveSkillIconLayer.RenderOpen().Close();
                picAscNodeNormalSurround.RenderOpen().Close();
                picAscNodeHighlightSurround.RenderOpen().Close();   
            }
        }

        public static void ClearAssets()
        {
            _Initialized = false;
        }

        public void DrawJewelHighlight(SkillNode node)
        {
            const int thickness = 10;
            var radiusPen = new Pen(Brushes.Cyan, thickness);

            const int smallRadius = 800 - thickness / 2;
            const int mediumRadius = 1200 - thickness / 2;
            const int largeRadius = 1500 - thickness / 2;

            using (DrawingContext dc = picJewelHighlight.RenderOpen())
            {
                dc.DrawEllipse(null, radiusPen, node.Position, smallRadius, smallRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, mediumRadius, mediumRadius);
                dc.DrawEllipse(null, radiusPen, node.Position, largeRadius, largeRadius);
                dc.Close();
            }
        }
    }
}