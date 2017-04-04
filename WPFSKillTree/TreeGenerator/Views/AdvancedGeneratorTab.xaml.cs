namespace POESKillTree.TreeGenerator.Views
{
    /// <summary>
    /// Interaction logic for AdvancedGeneratorTab.xaml
    /// </summary>
    public partial class AdvancedGeneratorTab
    {
        public AdvancedGeneratorTab()
        {
#if (DEBUG)
            try
            {
            InitializeComponent();
        }
            catch (System.Exception ex)
            { // Log error (including InnerExceptions!)
              // Handle exception
                System.Console.WriteLine("Exception called from AdvancedTabGenerator Initialization of type " + ex.ToString());
            }
#else
            InitializeComponent();
#endif
        }
    }
}