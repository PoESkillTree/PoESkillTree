using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeDrawing
{
    public abstract class NodeSurroundDrawer
    {
        private readonly IReadOnlyCollection<PassiveNodeViewModel> _nodes;
        private readonly double _sizeFactor;
        private readonly Func<PassiveNodeViewModel, Size> _getSize;
        private readonly Func<PassiveNodeViewModel, Brush> _getBrush;

        protected NodeSurroundDrawer(
            IReadOnlyCollection<PassiveNodeViewModel> nodes, double sizeFactor, Func<PassiveNodeViewModel, Size> getSize, Func<PassiveNodeViewModel, Brush> getBrush)
        {
            _nodes = nodes;
            _sizeFactor = sizeFactor;
            _getSize = getSize;
            _getBrush = getBrush;
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
            var size = _getSize(node);
            var brush = _getBrush(node);
            context.DrawRectangle(brush, null,
                new Rect(node.Position.X - size.Width * 0.5 * _sizeFactor,
                    node.Position.Y - size.Height * 0.5 * _sizeFactor,
                    size.Width * _sizeFactor,
                    size.Height * _sizeFactor));
        }

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }

    public class NonAscendancyNodeSurroundDrawer : NodeSurroundDrawer
    {
        public NonAscendancyNodeSurroundDrawer(
            IReadOnlyCollection<PassiveNodeViewModel> nodes, double sizeFactor, Func<PassiveNodeViewModel, Size> getSize, Func<PassiveNodeViewModel, Brush> getBrush)
            : base(nodes, sizeFactor, getSize, getBrush)
        {
        }

        public void Draw() => Draw(n => !n.IsAscendancyNode && n.PassiveNodeType != PassiveNodeType.Mastery);
    }

    public class AscendancyNodeSurroundDrawer : NodeSurroundDrawer
    {
        public AscendancyNodeSurroundDrawer(
            IReadOnlyCollection<PassiveNodeViewModel> nodes, double sizeFactor, Func<PassiveNodeViewModel, Size> getSize, Func<PassiveNodeViewModel, Brush> getBrush)
            : base(nodes, sizeFactor, getSize, getBrush)
        {
        }

        public void Draw(bool allAscendancies, string? ascendancyClassName) => Draw(n =>
            n.IsAscendancyNode && (allAscendancies || n.AscendancyName == ascendancyClassName) && n.PassiveNodeType != PassiveNodeType.Mastery);
    }
}