using System.Windows;
using System.Windows.Media;

namespace PoESkillTree.Views.Equipment
{
    /// <summary>
    /// Interaction logic for SkillTreeAreaView.xaml
    /// </summary>
    public partial class SkillTreeAreaView
    {
        public static readonly DependencyProperty SkillTreeVisualProperty = DependencyProperty.Register(
            "SkillTreeVisual", typeof(DrawingVisual), typeof(SkillTreeAreaView), new PropertyMetadata(default(DrawingVisual)));

        public DrawingVisual SkillTreeVisual
        {
            get => (DrawingVisual) GetValue(SkillTreeVisualProperty);
            set => SetValue(SkillTreeVisualProperty, value);
        }

        public SkillTreeAreaView()
        {
            InitializeComponent();
        }
    }
}
