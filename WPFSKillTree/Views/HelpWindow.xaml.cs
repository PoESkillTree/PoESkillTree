using PoESkillTree.Localization;

namespace PoESkillTree.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow
    {
        public string Document { get; set; }

        public HelpWindow()
        {
            Document = L10n.ReadAllText("Help.md");
            if (string.IsNullOrEmpty(Document))
                Document = L10n.Message("Help file not found");

            InitializeComponent();
        }
    }
}
