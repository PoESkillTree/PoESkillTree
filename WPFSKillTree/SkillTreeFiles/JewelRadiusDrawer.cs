using System;
using System.Collections.Generic;
using System.Windows.Media;
using PoESkillTree.Engine.GameModel.Items;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.SkillTreeFiles
{
    public class JewelRadiusDrawer
    {
        private const int RadiusPenThickness = 10;

        private static readonly IReadOnlyDictionary<JewelRadius, Brush> RadiusBrushes = new Dictionary<JewelRadius, Brush>
        {
            {JewelRadius.Small, Brushes.LightCyan},
            {JewelRadius.Medium, Brushes.Cyan},
            {JewelRadius.Large, Brushes.DarkCyan},
        };

        private readonly PoESkillTreeOptions _options;

        public JewelRadiusDrawer(PoESkillTreeOptions options)
        {
            _options = options;
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

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }
}