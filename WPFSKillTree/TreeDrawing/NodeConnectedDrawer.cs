using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeDrawing
{
    public abstract class NodeConnectedDrawer
    {
        private readonly SkillIcons _icons;
        private readonly IReadOnlyCollection<PassiveNodeViewModel> _nodes;
        private readonly Dictionary<string, BitmapImage> _connectedAssets;

        protected NodeConnectedDrawer(SkillIcons skillIcons, IReadOnlyCollection<PassiveNodeViewModel> nodes, Dictionary<string, BitmapImage> connectedAssets)
        {
            _icons = skillIcons;
            _nodes = nodes;
            _connectedAssets = connectedAssets;
            Visual = new DrawingVisual();
        }

        public DrawingVisual Visual { get; }

        protected void Draw(Func<PassiveNodeViewModel, bool> shouldDrawNode)
        {
            using var context = Visual.RenderOpen();
            foreach (var node in _nodes.Where(shouldDrawNode))
            {
                foreach (var other in node.NeighborPassiveNodes.Values)
                {
                    Draw(context, other);
                }
            }
        }

        private void Draw(DrawingContext context, PassiveNodeViewModel node)
        {
            if (!node.IconKey.Contains("Connected")) return;
            if (!_icons.SkillPositions.ContainsKey(node.IconKey)) return;

            var rect = _icons.SkillPositions[node.IconKey];
            var radius = Math.Sqrt((rect.Width * rect.Height) / Math.PI);

            var buttonKey = $"Passive{node.PassiveNodeType}ConnectedButton";
            if (_connectedAssets.ContainsKey(buttonKey))
            {
                var connected = _connectedAssets["PassiveMasteryConnectedButton"];
                var connectedImageBrush = new ImageBrush
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = connected,
                    Viewbox = new Rect(0, 0, 1, 1)
                };

                context.DrawEllipse(connectedImageBrush, null, node.Position, connected.PixelWidth * SkillTree.PoESkillTree.MaxImageZoomLevel * 1.5, connected.PixelHeight * SkillTree.PoESkillTree.MaxImageZoomLevel * 1.5);
            }

            var image = _icons.GetSkillImage(node.IconKey);
            var imageBrush = new ImageBrush
            {
                Stretch = Stretch.Uniform,
                ImageSource = image,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewbox = new Rect(rect.X / image.PixelWidth, rect.Y / image.PixelHeight,
                    rect.Width / image.PixelWidth, rect.Height / image.PixelHeight)
            };
            context.DrawEllipse(imageBrush, null, node.Position, radius, radius);
        }

        public void Clear()
        {
            Visual.RenderOpen().Close();
        }
    }

    public class MasteryNodeConnectedDrawer : NodeConnectedDrawer
    {
        public MasteryNodeConnectedDrawer(SkillIcons skillIcons, IReadOnlyCollection<PassiveNodeViewModel> nodes, Dictionary<string, BitmapImage> connectedAssets)
            : base(skillIcons, nodes, connectedAssets)
        {
        }

        public void Draw() => Draw(n => n.NeighborPassiveNodes.Values.Count(x => x.PassiveNodeType == PassiveNodeType.Mastery) > 0);
    }
}