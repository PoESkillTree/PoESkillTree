using System;
using NLog.LayoutRenderers;
using PoESkillTree.Utils;
using PoESkillTree.Views;

namespace PoESkillTree.SkillTreeFiles
{
    // Application entry class.
    class Bootstrap : MarshalByRefObject
    {
        // Entry point method.
        [STAThread]
        public static void Main(string[] arguments)
        {
            LayoutRenderer.Register("appData", _ => AppData.GetFolder());
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
