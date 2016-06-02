using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using POESKillTree.Views;
using ColorType = POESKillTree.Views.ColorButtonDialog.ColorType;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for ColorPickerButton.xaml
    /// </summary>
    public partial class ColorPickerButton : UserControl
    {
        public Brush BackgroundColor
        {
            get { return (Brush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(ColorPickerButton), new UIPropertyMetadata(BackgroundOrBorderColorPropertyChanged));

        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Brush), typeof(ColorPickerButton), new UIPropertyMetadata(BackgroundOrBorderColorPropertyChanged));

        public Brush DisplayBorderColor
        {
            get { return (Brush)GetValue(DisplayBorderColorProperty); }
            set { SetValue(DisplayBorderColorProperty, value); }
        }

        public static readonly DependencyProperty DisplayBorderColorProperty = DependencyProperty.Register("DisplayBorderColor", typeof(Brush), typeof(ColorPickerButton), new UIPropertyMetadata(null));

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(ColorPickerButton), new UIPropertyMetadata(null));

        public ColorPickerButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorButtonDialog dialog = new ColorButtonDialog { Owner = Window.GetWindow(this) };

            dialog.OnSelectedColorChange += OnSelectedColorChanged;

            dialog.SetColors(BackgroundColor == null ? null : (Color?)(BackgroundColor as SolidColorBrush).Color,
                             BorderColor == null ? null : (Color?)(BorderColor as SolidColorBrush).Color,
                             TextColor == null ? null : (Color?)(TextColor as SolidColorBrush).Color);

            dialog.ShowDialog();
        }

        private static void BackgroundOrBorderColorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerButton button = (obj as ColorPickerButton);

            if (e.Property == BorderColorProperty)
                button.DisplayBorderColor = e.NewValue == null ? button.BackgroundColor : button.BorderColor;
            else
                button.DisplayBorderColor = button.BorderColor == null ? button.BackgroundColor : button.BorderColor;
        }

        private void OnSelectedColorChanged(ColorType type, Color? color)
        {
            switch (type)
            {
                case ColorType.Text:
                    TextColor = color == null ? null : new SolidColorBrush((Color)color);
                    break;

                case ColorType.Background:
                    BackgroundColor = color == null ? null : new SolidColorBrush((Color)color);
                    break;

                case ColorType.Border:
                    BorderColor = color == null ? null : new SolidColorBrush((Color)color);
                    break;
            }
        }
    }
}
