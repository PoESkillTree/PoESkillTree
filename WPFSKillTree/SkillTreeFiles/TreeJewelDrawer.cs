using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.SkillTreeFiles
{
    public class TreeJewelDrawer
    {
        private enum JewelType
        {
            Abyss,
            Blue,
            Green,
            Red,
            Prismatic,
        }

        private static readonly IReadOnlyDictionary<JewelType, string> AssetNames = new Dictionary<JewelType, string>
        {
            {JewelType.Abyss, "JewelSocketActiveAbyss"},
            {JewelType.Blue, "JewelSocketActiveBlue"},
            {JewelType.Green, "JewelSocketActiveGreen"},
            {JewelType.Red, "JewelSocketActiveRed"},
            {JewelType.Prismatic, "JewelSocketActivePrismatic"},
        };

        private readonly IReadOnlyDictionary<string, BitmapImage> _assets;
        private readonly IReadOnlyDictionary<ushort, SkillNode> _skillNodes;
        private readonly DrawingVisual _drawingVisual;
        private readonly Dictionary<JewelType, (Size, ImageBrush)> _brushes = new Dictionary<JewelType, (Size, ImageBrush)>();
        private IReadOnlyList<InventoryItemViewModel> _jewelViewModels;

        public TreeJewelDrawer(
            IReadOnlyDictionary<string, BitmapImage> assets, IReadOnlyDictionary<ushort, SkillNode> skillNodes,
            DrawingVisual drawingContext)
        {
            _assets = assets;
            _skillNodes = skillNodes;
            _drawingVisual = drawingContext;
            _jewelViewModels = Array.Empty<InventoryItemViewModel>();
        }

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
                Draw();
            }
        }

        public void Draw()
        {
            var stopwatch = Stopwatch.StartNew();
            using var dc = _drawingVisual.RenderOpen();
            foreach (var item in JewelViewModels.Select(vm => vm.Item).WhereNotNull())
            {
                if (GetJewelType(item.Tags) is JewelType jewelType)
                {
                    Draw(dc, item.Socket!.Value, jewelType);
                }
            }
            stopwatch.Stop();
            LogManager.GetCurrentClassLogger().Info($"Draw: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void Draw(DrawingContext drawingContext, ushort nodeId, JewelType jewelType)
        {
            var (size, brush) = _brushes.GetOrAdd(jewelType, CreateBrush);
            var node = _skillNodes[nodeId];
            drawingContext.DrawRectangle(brush, null,
                new Rect(node.Position.X - size.Width,
                    node.Position.Y - size.Height,
                    size.Width * 2,
                    size.Height * 2));
        }

        private static JewelType? GetJewelType(Tags tags)
        {
            if (tags.HasFlag(Tags.AbyssJewel))
                return JewelType.Abyss;
            if (tags.HasFlag(Tags.StrJewel) && tags.HasFlag(Tags.DexJewel) && tags.HasFlag(Tags.IntJewel))
                return JewelType.Prismatic;
            if (tags.HasFlag(Tags.StrJewel))
                return JewelType.Red;
            if (tags.HasFlag(Tags.DexJewel))
                return JewelType.Green;
            if (tags.HasFlag(Tags.IntJewel))
                return JewelType.Blue;
            return null;
        }

        private (Size, ImageBrush) CreateBrush(JewelType jewelType)
        {
            var assetName = AssetNames[jewelType];
            var image = _assets[assetName];
            var size = new Size(image.PixelWidth, image.PixelHeight);
            var brush = new ImageBrush
            {
                Stretch = Stretch.Uniform,
                ImageSource = image,
            };
            return (size, brush);
        }
    }
}