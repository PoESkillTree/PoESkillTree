using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PoESkillTree.Engine.GameModel.Items;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.SkillTreeFiles
{
    public class JewelRadiusDrawer
    {
        private const int RadiusPenThickness = 8;

        private static readonly IReadOnlyDictionary<JewelRadius, Brush> RadiusBrushes = new Dictionary<JewelRadius, Brush>
        {
            {JewelRadius.Large, Brushes.DarkCyan},
            {JewelRadius.Medium, Brushes.Cyan},
            {JewelRadius.Small, Brushes.LightCyan},
        };

        private readonly PoESkillTreeOptions _options;
        private readonly IReadOnlyCollection<SkillNode> _skillNodes;

        public JewelRadiusDrawer(PoESkillTreeOptions options, IReadOnlyCollection<SkillNode> skillNodes)
        {
            _options = options;
            _skillNodes = skillNodes;
            Visual = new DrawingVisual();
        }

        public DrawingVisual Visual { get; }

        public void Draw(SkillNode node, Item? socketedJewel)
        {
            using var dc = Visual.RenderOpen();
            if (socketedJewel is null)
            {
                foreach (var (jewelRadius, brush) in RadiusBrushes)
                {
                    Draw(dc, node, jewelRadius, brush);
                }
            }
            else
            {
                var jewelRadius = socketedJewel.JewelRadius;
                if (jewelRadius != JewelRadius.None)
                {
                    Draw(dc, node, jewelRadius, RadiusBrushes[jewelRadius]);
                }
            }
        }

        private void Draw(DrawingContext context, SkillNode node, JewelRadius radiusEnum, Brush brush)
        {
            DrawRadius(context, node, radiusEnum, brush);
            DrawNodeHighlights(context, node, radiusEnum, brush);
        }

        private void DrawRadius(DrawingContext context, SkillNode node, JewelRadius radiusEnum, Brush brush)
        {
            double radius = radiusEnum.GetRadius();
            if (_options?.Circles != null && _options.Circles.TryGetValue(radiusEnum.ToString(), out var circles)
                                          && Constants.AssetZoomLevel < circles.Count)
            {
                var circle = circles[Constants.AssetZoomLevel];
                radius = Math.Round(circle.Width / circle.ZoomLevel / 2);
            }

            radius -= RadiusPenThickness / 2;
            var pen = new Pen(brush, RadiusPenThickness);

            context.DrawEllipse(null, pen, node.Position, radius, radius);
        }

        private void DrawNodeHighlights(DrawingContext context, SkillNode node, JewelRadius radiusEnum, Brush brush)
        {
            var radius = radiusEnum.GetRadius();
            var nodesInRadius = _skillNodes
                .Where(n => !n.IsMastery && !n.IsRootNode && !n.IsAscendancyNode)
                .Where(n => Distance(n.Position, node.Position) <= radius);
            var pen = new Pen(brush, RadiusPenThickness);
            foreach (var n in nodesInRadius)
            {
                context.DrawEllipse(null, pen, n.Position, 60, 60);
            }
        }

        private static double Distance(Vector2D a, Vector2D b)
        {
            var xDistance = a.X - b.X;
            var yDistance = a.Y - b.Y;
            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }
}