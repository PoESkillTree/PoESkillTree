using System.Configuration;
using POESKillTree.Utils;

namespace POESKillTree.Properties {

    // In case this gets used at some point, there are no changes needed for it to
    // use the correct config files ("user.config" in the "Settings" folder).
    [SettingsProvider(typeof (CustomSettingsProvider))]
    internal sealed partial class Settings
    {

        /// <summary>
        /// The path the Settings files are stored in. Different for portable installations.
        /// </summary>
        public static string SettingsPath = AppData.GetFolder("Settings");

        public Settings()
            : base("user")
        {
        }
    }
}
