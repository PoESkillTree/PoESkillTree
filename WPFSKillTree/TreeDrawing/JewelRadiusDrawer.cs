using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.Equipment;
using PoESkillTree.ViewModels.PassiveTree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.TreeDrawing
{
    public class JewelRadiusDrawer
    {
        private const int RadiusPenThickness = 5;

        private static readonly IReadOnlyDictionary<JewelRadius, Brush> RadiusBrushes = new Dictionary<JewelRadius, Brush>
        {
            {JewelRadius.Large, Brushes.DarkCyan},
            {JewelRadius.Medium, Brushes.Cyan},
            {JewelRadius.Small, Brushes.LightCyan},
        };

        private readonly IReadOnlyDictionary<ushort, PassiveNodeViewModel> _skillNodes;
        private readonly IReadOnlyCollection<PassiveNodeViewModel> _skilledNodes;
        private readonly Func<PassiveNodeViewModel, Size> _getSize;
        private readonly DrawingVisual _skilledNodesVisual;
        private readonly DrawingVisual _highlightVisual;
        private IReadOnlyList<InventoryItemViewModel> _jewelViewModels;

        public JewelRadiusDrawer(IReadOnlyDictionary<ushort, PassiveNodeViewModel> skillNodes, IReadOnlyCollection<PassiveNodeViewModel> skilledNodes, Func<PassiveNodeViewModel, Size> getSize)
        {
            _skillNodes = skillNodes;
            _skilledNodes = skilledNodes;
            _getSize = getSize;
            _skilledNodesVisual = new DrawingVisual();
            _highlightVisual = new DrawingVisual();
            Visual = new DrawingVisual();
            Visual.Children.Add(_skilledNodesVisual);
            Visual.Children.Add(_highlightVisual);
            _jewelViewModels = Array.Empty<InventoryItemViewModel>();
        }

        public DrawingVisual Visual { get; }

        public IReadOnlyList<InventoryItemViewModel> JewelViewModels
        {
            get => _jewelViewModels;
            set
            {
                foreach (var oldVm in _jewelViewModels)
                {
                    oldVm.PropertyChanged -= JewelViewModelOnPropertyChanged;
                }
                foreach (var newVm in value)
                {
                    newVm.PropertyChanged += JewelViewModelOnPropertyChanged;
                }
                _jewelViewModels = value;
            }
        }

        private void JewelViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(InventoryItemViewModel.Item))
            {
                DrawSkilledNodes();
            }
        }

        public void DrawSkilledNodes()
        {
            using var dc = _skilledNodesVisual.RenderOpen();
            foreach (var item in JewelViewModels.Select(vm => vm.Item).WhereNotNull())
            {
                var node = _skillNodes[item.Socket!.Value];
                if (_skilledNodes.Contains(node))
                {
                    DrawRadius(dc, node, item.JewelRadius);
                }
            }
        }

        public void DrawHighlight(PassiveNodeViewModel node, Item? socketedJewel)
        {
            using var dc = _highlightVisual.RenderOpen();
            if (socketedJewel is null)
            {
                foreach (var jewelRadius in RadiusBrushes.Keys)
                {
                    DrawRadius(dc, node, jewelRadius);
                    DrawNodeHighlights(dc, node, jewelRadius);
                }
            }
            else
            {
                DrawRadius(dc, node, socketedJewel.JewelRadius);
                DrawNodeHighlights(dc, node, socketedJewel.JewelRadius);
            }
        }

        private void DrawRadius(DrawingContext context, PassiveNodeViewModel node, JewelRadius radiusEnum)
        {
            if (radiusEnum == JewelRadius.None)
                return;

            double radius = radiusEnum.GetRadius(node.ZoomLevel) - (RadiusPenThickness / 2);
            var pen = new Pen(RadiusBrushes[radiusEnum], RadiusPenThickness);

            context.DrawEllipse(null, pen, node.Position, radius, radius);
        }

        private void DrawNodeHighlights(DrawingContext context, PassiveNodeViewModel node, JewelRadius radiusEnum)
        {
            if (radiusEnum == JewelRadius.None)
                return;

            var radius = radiusEnum.GetRadius(node.ZoomLevel);
            var nodesInRadius = _skillNodes.Values
                .Where(n => n.PassiveNodeType != PassiveNodeType.Mastery && !n.IsRootNode && !n.IsAscendancyNode)
                .Where(n => Distance(n.Position, node.Position) <= radius);
            var pen = new Pen(RadiusBrushes[radiusEnum], RadiusPenThickness);
            foreach (var n in nodesInRadius)
            {
                var size = _getSize(n);
                var highlightRadius = Math.Sqrt((size.Width * size.Height) / Math.PI) + 10;
                context.DrawEllipse(null, pen, n.Position, highlightRadius, highlightRadius);
            }
        }

        private static double Distance(Vector2D a, Vector2D b)
        {
            var xDistance = a.X - b.X;
            var yDistance = a.Y - b.Y;
            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }

        public void ClearHighlight()
        {
            _highlightVisual.RenderOpen().Close();
        }
    }
}