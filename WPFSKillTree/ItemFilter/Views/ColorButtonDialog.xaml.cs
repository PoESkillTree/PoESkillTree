using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit;
using POESKillTree.Localization;
using POESKillTree.Utils.Converter;
using NamedColor = POESKillTree.Utils.Converter.ColorUtils.NamedColor;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for ColorButtonDialog.xaml
    /// </summary>
    public partial class ColorButtonDialog : MetroWindow
    {
        public enum ColorType { Background, Border, Text }

        public delegate void OnSelectedColorChangeEventHandler(ColorType type, Color? color);

        public event OnSelectedColorChangeEventHandler OnSelectedColorChange;

        public ColorButtonDialog()
        {
            InitializeComponent();

            ObservableCollection<ColorItem> available = new ObservableCollection<ColorItem> { new ColorItem(null, "Default") };

            ObservableCollection<ColorItem> standard = new ObservableCollection<ColorItem>();
            foreach (NamedColor nc in ColorUtils.StandardColors)
                standard.Add(new ColorItem(nc.Color, nc.Name));

            BackgroundColorPicker.AvailableColorsHeader = BorderColorPicker.AvailableColorsHeader = TextColorPicker.AvailableColorsHeader = L10n.Message("Original color");
            BackgroundColorPicker.AvailableColors = BorderColorPicker.AvailableColors = TextColorPicker.AvailableColors = available;
            BackgroundColorPicker.StandardColorsHeader = BorderColorPicker.StandardColorsHeader = TextColorPicker.StandardColorsHeader = L10n.Message("Standard colors");
            BackgroundColorPicker.StandardColors = BorderColorPicker.StandardColors = TextColorPicker.StandardColors = standard;
            BackgroundColorPicker.RecentColorsHeader = BorderColorPicker.RecentColorsHeader = TextColorPicker.RecentColorsHeader = L10n.Message("Recent colors");
            BackgroundColorPicker.AdvancedButtonHeader = BorderColorPicker.AdvancedButtonHeader = TextColorPicker.AdvancedButtonHeader = L10n.Message("Custom", "color");
            BackgroundColorPicker.StandardButtonHeader = BorderColorPicker.StandardButtonHeader = TextColorPicker.StandardButtonHeader = L10n.Message("Standard", "color");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnInternalSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (OnSelectedColorChange != null)
            {
                ColorType type;

                if (sender == BackgroundColorPicker)
                    type = ColorType.Background;
                else if (sender == BorderColorPicker)
                    type = ColorType.Border;
                else if (sender == TextColorPicker)
                    type = ColorType.Text;
                else
                    throw new InvalidOperationException("Invalid sender");

                OnSelectedColorChange(type, e.NewValue);
            }
        }

        public void SetColors(Color? background, Color? border, Color? text)
        {
            BackgroundColorPicker.SelectedColor = background;
            BorderColorPicker.SelectedColor = border;
            TextColorPicker.SelectedColor = text;
        }
    }
}
