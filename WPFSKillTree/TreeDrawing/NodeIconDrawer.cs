using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeDrawing
{
    public abstract class NodeIconDrawer
    {
        private readonly SkillIcons _icons;
        private readonly IReadOnlyCollection<PassiveNodeViewModel> _nodes;

        protected NodeIconDrawer(SkillIcons skillIcons, IReadOnlyCollection<PassiveNodeViewModel> nodes)
        {
            _icons = skillIcons;
            _nodes = nodes;
            Visual = new DrawingVisual();
        }

        public DrawingVisual Visual { get; }

        protected void Draw(Func<PassiveNodeViewModel, bool> shouldDrawNode)
        {
            using var context = Visual.RenderOpen();
            foreach (var node in _nodes.Where(shouldDrawNode))
            {
                Draw(context, node);
            }
        }

        private void Draw(DrawingContext context, PassiveNodeViewModel node)
        {
            var rect = _icons.SkillPositions[node.IconKey];
            var image = _icons.GetSkillImage(node.IconKey);
            var imageBrush = new ImageBrush
            {
                Stretch = Stretch.Uniform,
                ImageSource = image,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewbox = new Rect(rect.X / image.PixelWidth, rect.Y / image.PixelHeight,
                    rect.Width / image.PixelWidth, rect.Height / image.PixelHeight)
            };

            var radius = Math.Sqrt((rect.Width * rect.Height) / Math.PI);
            context.DrawEllipse(imageBrush, null, node.Position, radius, radius);
        }

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }

    public class NonAscendancyNodeIconDrawer : NodeIconDrawer
    {
        public NonAscendancyNodeIconDrawer(SkillIcons skillIcons, IReadOnlyCollection<PassiveNodeViewModel> nodes)
            : base(skillIcons, nodes)
        {
        }

        public void Draw() => Draw(n => !n.IsAscendancyNode);
    }

    public class AscendancyNodeIconDrawer : NodeIconDrawer
    {
        public AscendancyNodeIconDrawer(SkillIcons skillIcons, IReadOnlyCollection<PassiveNodeViewModel> nodes) : base(skillIcons, nodes)
        {
        }

        public void Draw(bool allAscendancies, string? ascendancyClassName) => Draw(n =>
            n.IsAscendancyNode && (allAscendancies || n.AscendancyName == ascendancyClassName));
    }
}