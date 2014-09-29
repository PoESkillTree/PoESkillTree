using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        #region Members

        public List<KeyValuePair<Rect, ImageBrush>> FacesBrush = new List<KeyValuePair<Rect, ImageBrush>>();
        public List<KeyValuePair<Size, ImageBrush>> NodeSurroundBrush = new List<KeyValuePair<Size, ImageBrush>>();
        public DrawingVisual SkillTreeVisual;

        public Dictionary<bool, KeyValuePair<Rect, ImageBrush>> StartBackgrounds =
            new Dictionary<bool, KeyValuePair<Rect, ImageBrush>>();

        public DrawingVisual picActiveLinks;
        public DrawingVisual picBackground;
        public DrawingVisual picFaces;
        public DrawingVisual picHighlights;
        public DrawingVisual picLinks;
        public DrawingVisual picPathOverlay;
        public DrawingVisual picSkillBaseSurround;
        public DrawingVisual picSkillIconLayer;
        public DrawingVisual picSkillSurround;

        public void CreateCombineVisual()
        {
            SkillTreeVisual = new DrawingVisual();
            SkillTreeVisual.Children.Add(picBackground);
            SkillTreeVisual.Children.Add(picLinks);
            SkillTreeVisual.Children.Add(picActiveLinks);
            SkillTreeVisual.Children.Add(picPathOverlay);
            SkillTreeVisual.Children.Add(picSkillIconLayer);
            SkillTreeVisual.Children.Add(picSkillBaseSurround);
            SkillTreeVisual.Children.Add(picSkillSurround);
            SkillTreeVisual.Children.Add(picFaces);
            SkillTreeVisual.Children.Add(picHighlights);
        }

        #endregion

        public void ClearPath()
        {
            picPathOverlay.RenderOpen().Close();
        }

        private void DrawBackgroundLayer()
        {
            picBackground = new DrawingVisual();
            using (DrawingContext dc = picBackground.RenderOpen())
            {
                BitmapImage[] iscr =
                {
                    _assets["PSGroupBackground1"].PImage, _assets["PSGroupBackground2"].PImage,
                    _assets["PSGroupBackground3"].PImage
                };
                var OrbitBrush = new Brush[3];
                OrbitBrush[0] = new ImageBrush(_assets["PSGroupBackground1"].PImage);
                OrbitBrush[1] = new ImageBrush(_assets["PSGroupBackground2"].PImage);
                OrbitBrush[2] = new ImageBrush(_assets["PSGroupBackground3"].PImage);
                (OrbitBrush[2] as ImageBrush).TileMode = TileMode.FlipXY;
                (OrbitBrush[2] as ImageBrush).Viewport = new Rect(0, 0, 1, .5f);

                var BackgroundBrush = new ImageBrush(_assets["Background1"].PImage);
                BackgroundBrush.TileMode = TileMode.FlipXY;
                dc.DrawRectangle(BackgroundBrush, null, TRect);
                foreach (SkillNodeGroup ngp in NodeGroups)
                {
                    if (ngp.OcpOrb == null)
                        ngp.OcpOrb = new Dictionary<int, bool>();
                    IEnumerable<int> cgrp = ngp.OcpOrb.Keys.Where(ng => ng <= 3);
                    if (cgrp.Count() == 0) continue;
                    int maxr = cgrp.Max(ng => ng);
                    if (maxr == 0) continue;
                    maxr = maxr > 3 ? 2 : maxr - 1;
                    int maxfac = maxr == 2 ? 2 : 1;
                    dc.DrawRectangle(OrbitBrush[maxr], null,
                        new Rect(
                            ngp.Position -
                            new Vector2D(iscr[maxr].PixelWidth * 1.5, iscr[maxr].PixelHeight * 1.5 * maxfac),
                            new Size(iscr[maxr].PixelWidth * 3, iscr[maxr].PixelHeight * 3 * maxfac)));
                }
            }
        }

        private void DrawConnection(DrawingContext dc, Pen pen2, SkillNode n1, SkillNode n2)
        {
            if (n1.SkillNodeGroup == n2.SkillNodeGroup && n1.orbit == n2.orbit)
            {
                if (n1.Arc - n2.Arc > 0 && n1.Arc - n2.Arc <= Math.PI ||
                    n1.Arc - n2.Arc < -Math.PI)
                {
                    dc.DrawArc(null, pen2, n1.Position, n2.Position,
                        new Size(SkillNode.orbitRadii[n1.orbit],
                            SkillNode.orbitRadii[n1.orbit]));
                }
                else
                {
                    dc.DrawArc(null, pen2, n2.Position, n1.Position,
                        new Size(SkillNode.orbitRadii[n1.orbit],
                            SkillNode.orbitRadii[n1.orbit]));
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
                    Vector2D pos = Skillnodes.First(nd => nd.Value.name.ToUpper() == s.ToUpper()).Value.Position;
                    dc.DrawRectangle(StartBackgrounds[false].Value, null,
                        new Rect(
                            pos - new Vector2D(StartBackgrounds[false].Key.Width, StartBackgrounds[false].Key.Height),
                            pos + new Vector2D(StartBackgrounds[false].Key.Width, StartBackgrounds[false].Key.Height)));
                    if (_chartype == i)
                    {
                        dc.DrawRectangle(FacesBrush[i].Value, null,
                            new Rect(pos - new Vector2D(FacesBrush[i].Key.Width, FacesBrush[i].Key.Height),
                                pos + new Vector2D(FacesBrush[i].Key.Width, FacesBrush[i].Key.Height)));
                    }
                }
            }
        }

        public void DrawHighlights(List<SkillNode> nodes, SolidColorBrush brush = null)
        {
            var hpen = new Pen(brush == null ? Brushes.Aqua : brush, 20);
            using (DrawingContext dc = picHighlights.RenderOpen())
            {
                foreach (SkillNode node in nodes)
                {
                    dc.DrawEllipse(null, hpen, node.Position, 80, 80);
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

        private void DrawNodeBaseSurround()
        {
            using (DrawingContext dc = picSkillBaseSurround.RenderOpen())
            {
                foreach (ushort skillNode in Skillnodes.Keys)
                {
                    Vector2D pos = (Skillnodes[skillNode].Position);

                    if (Skillnodes[skillNode].not)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[3].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[3].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[3].Key.Height,
                                NodeSurroundBrush[3].Key.Width * 2,
                                NodeSurroundBrush[3].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].ks)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[2].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[2].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[2].Key.Height,
                                NodeSurroundBrush[2].Key.Width * 2,
                                NodeSurroundBrush[2].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].m)
                    {
                    }
                    else
                        dc.DrawRectangle(NodeSurroundBrush[0].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[0].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[0].Key.Height,
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

                    if (Skillnodes[skillNode].not)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[5].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[5].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[5].Key.Height,
                                NodeSurroundBrush[5].Key.Width * 2,
                                NodeSurroundBrush[5].Key.Height * 2));
                    }
                    else if (Skillnodes[skillNode].ks)
                    {
                        dc.DrawRectangle(NodeSurroundBrush[4].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[4].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[4].Key.Height,
                                NodeSurroundBrush[4].Key.Width * 2,
                                NodeSurroundBrush[4].Key.Height * 2));
                    }
                    else
                        dc.DrawRectangle(NodeSurroundBrush[1].Value, null,
                            new Rect((int) pos.X - NodeSurroundBrush[1].Key.Width,
                                (int) pos.Y - NodeSurroundBrush[1].Key.Height,
                                NodeSurroundBrush[1].Key.Width * 2,
                                NodeSurroundBrush[1].Key.Height * 2));
                }
            }
        }

        public void DrawPath(List<ushort> path)
        {
            var pen2 = new Pen(Brushes.LawnGreen, 15f);
            pen2.DashStyle = new DashStyle(new DoubleCollection {2}, 2);

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                for (int i = -1; i < path.Count - 1; i++)
                {
                    SkillNode n1 = i == -1
                        ? Skillnodes[path[i + 1]].Neighbor.First(sn => SkilledNodes.Contains(sn.id))
                        : Skillnodes[path[i]];
                    SkillNode n2 = Skillnodes[path[i + 1]];

                    DrawConnection(dc, pen2, n1, n2);
                }
            }
        }

        public void DrawRefundPreview(HashSet<ushort> nodes)
        {
            var pen2 = new Pen(Brushes.Red, 15f);
            pen2.DashStyle = new DashStyle(new DoubleCollection {2}, 2);

            using (DrawingContext dc = picPathOverlay.RenderOpen())
            {
                foreach (ushort node in nodes)
                {
                    foreach (SkillNode n2 in Skillnodes[node].Neighbor)
                    {
                        if (SkilledNodes.Contains(n2.id) && (node < n2.id || !(nodes.Contains(n2.id))))
                            DrawConnection(dc, pen2, Skillnodes[node], n2);
                    }
                }
            }
        }

        private void DrawSkillIconLayer()
        {
            var pen = new Pen(Brushes.Black, 5);
            var pen3 = new Pen(Brushes.Green, 10);
            picSkillIconLayer = new DrawingVisual();

            Geometry g = new RectangleGeometry(TRect);
            using (DrawingContext dc = picSkillIconLayer.RenderOpen())
            {
                dc.DrawGeometry(null, pen, g);
                foreach (var skillNode in Skillnodes)
                {
                    Size isize;
                    var br = new ImageBrush();
                    int icontype = skillNode.Value.not ? 1 : skillNode.Value.ks ? 2 : 0;
                    Rect r = IconActiveSkills.SkillPositions[skillNode.Value.icon].Key;
                    BitmapImage bi =
                        IconActiveSkills.Images[IconActiveSkills.SkillPositions[skillNode.Value.icon].Value];
                    br.Stretch = Stretch.Uniform;
                    br.ImageSource = bi;

                    br.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
                    br.Viewbox = new Rect(r.X / bi.PixelWidth, r.Y / bi.PixelHeight, r.Width / bi.PixelWidth,
                        r.Height / bi.PixelHeight);
                    Vector2D pos = (skillNode.Value.Position);
                    dc.DrawEllipse(br, null, pos, r.Width, r.Height);
                }
            }
        }

        private void InitFaceBrushesAndLayer()
        {
            foreach (string faceName in FaceNames)
            {
                var bi = ImageHelper.OnLoadBitmapImage(new Uri("Data\\Assets\\" + faceName + ".png", UriKind.Relative));
                FacesBrush.Add(new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                    new ImageBrush(bi)));
            }

            var bi2 = ImageHelper.OnLoadBitmapImage(new Uri("Data\\Assets\\PSStartNodeBackgroundInactive.png", UriKind.Relative));
            StartBackgrounds.Add(false,
                (new KeyValuePair<Rect, ImageBrush>(new Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight),
                    new ImageBrush(bi2))));
            picFaces = new DrawingVisual();
        }

        private void InitNodeSurround()
        {
            picSkillSurround = new DrawingVisual();
            picSkillBaseSurround = new DrawingVisual();
            Size sizeNot;
            var brNot = new ImageBrush();
            brNot.Stretch = Stretch.Uniform;
            BitmapImage PImageNot = _assets[NodeBackgrounds["notable"]].PImage;
            brNot.ImageSource = PImageNot;
            sizeNot = new Size(PImageNot.PixelWidth, PImageNot.PixelHeight);


            Size sizeKs;
            var brKS = new ImageBrush();
            brKS.Stretch = Stretch.Uniform;
            BitmapImage PImageKr = _assets[NodeBackgrounds["keystone"]].PImage;
            brKS.ImageSource = PImageKr;
            sizeKs = new Size(PImageKr.PixelWidth, PImageKr.PixelHeight);

            Size sizeNotH;
            var brNotH = new ImageBrush();
            brNotH.Stretch = Stretch.Uniform;
            BitmapImage PImageNotH = _assets[NodeBackgroundsActive["notable"]].PImage;
            brNotH.ImageSource = PImageNotH;
            sizeNotH = new Size(PImageNotH.PixelWidth, PImageNotH.PixelHeight);


            Size sizeKsH;
            var brKSH = new ImageBrush();
            brKSH.Stretch = Stretch.Uniform;
            BitmapImage PImageKrH = _assets[NodeBackgroundsActive["keystone"]].PImage;
            brKSH.ImageSource = PImageKrH;
            sizeKsH = new Size(PImageKrH.PixelWidth, PImageKrH.PixelHeight);

            Size isizeNorm;
            var brNorm = new ImageBrush();
            brNorm.Stretch = Stretch.Uniform;
            BitmapImage PImageNorm = _assets[NodeBackgrounds["normal"]].PImage;
            brNorm.ImageSource = PImageNorm;
            isizeNorm = new Size(PImageNorm.PixelWidth, PImageNorm.PixelHeight);

            Size isizeNormA;
            var brNormA = new ImageBrush();
            brNormA.Stretch = Stretch.Uniform;
            BitmapImage PImageNormA = _assets[NodeBackgroundsActive["normal"]].PImage;
            brNormA.ImageSource = PImageNormA;
            isizeNormA = new Size(PImageNormA.PixelWidth, PImageNormA.PixelHeight);

            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isizeNorm, brNorm));
            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(isizeNormA, brNormA));
            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeKs, brKS));
            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeNot, brNot));
            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeKsH, brKSH));
            NodeSurroundBrush.Add(new KeyValuePair<Size, ImageBrush>(sizeNotH, brNotH));
        }

        private void InitOtherDynamicLayers()
        {
            picActiveLinks = new DrawingVisual();
            picPathOverlay = new DrawingVisual();
            picHighlights = new DrawingVisual();
        }
    }
}