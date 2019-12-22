using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.TreeDrawing
{
    public abstract class NodeSurroundDrawer
    {
        private readonly IReadOnlyCollection<SkillNode> _nodes;
        private readonly double _sizeFactor;
        private readonly Func<SkillNode, Size> _getSize;
        private readonly Func<SkillNode, Brush> _getBrush;

        protected NodeSurroundDrawer(
            IReadOnlyCollection<SkillNode> nodes, double sizeFactor, Func<SkillNode, Size> getSize, Func<SkillNode, Brush> getBrush)
        {
            _nodes = nodes;
            _sizeFactor = sizeFactor;
            _getSize = getSize;
            _getBrush = getBrush;
            Visual = new DrawingVisual();
        }

        public DrawingVisual Visual { get; }

        protected void Draw(Func<SkillNode, bool> shouldDrawNode)
        {
            using var context = Visual.RenderOpen();
            foreach (var node in _nodes.Where(shouldDrawNode))
            {
                Draw(context, node);
            }
        }

        private void Draw(DrawingContext context, SkillNode node)
        {
            var size = _getSize(node);
            var brush = _getBrush(node);
            context.DrawRectangle(brush, null,
                new Rect(node.Position.X - size.Width * _sizeFactor,
                    node.Position.Y - size.Height * _sizeFactor,
                    size.Width * 2 * _sizeFactor,
                    size.Height * 2 * _sizeFactor));
        }

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }

    public class NonAscendancyNodeSurroundDrawer : NodeSurroundDrawer
    {
        public NonAscendancyNodeSurroundDrawer(
            IReadOnlyCollection<SkillNode> nodes, double sizeFactor, Func<SkillNode, Size> getSize, Func<SkillNode, Brush> getBrush)
            : base(nodes, sizeFactor, getSize, getBrush)
        {
        }

        public void Draw() => Draw(n => !n.IsAscendancyNode);
    }

    public class AscendancyNodeSurroundDrawer : NodeSurroundDrawer
    {
        public AscendancyNodeSurroundDrawer(
            IReadOnlyCollection<SkillNode> nodes, double sizeFactor, Func<SkillNode, Size> getSize, Func<SkillNode, Brush> getBrush)
            : base(nodes, sizeFactor, getSize, getBrush)
        {
        }

        public void Draw(bool allAscendancies, string? ascendancyClassName) => Draw(n =>
            n.IsAscendancyNode && (allAscendancies || n.AscendancyName == ascendancyClassName));
    }
}