using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using POESKillTree.Model;

/* TODO:
 * - Catalog.Unload() to unload previously used Catalog (clear messages, etc.), only one catalog should be loaded at a time (two during switch).
 * - Some tool to remove potential BOM header from translation catalogs before msgmerge, in case someone is editing them in VS/Notepad/etc.
 * - BuildLocale target should be called after regular Build of project (due to possible generated content/source)
 * - Locale folder should be copied in release.xml, to avoid forgetting to flag files in project as Copy always.
 * - Menu Theme/Accent cannot be localized due to use of MenuItem Content directly.
 * - Filter ComboBox cannot be localized due to use of ComboBoxItem Content directly.
 * - Level/Used top-middle bar doesn't have enough space for localization.
 */

namespace POESKillTree.Localization
{
    // Localization API.
    public class L10n
    {
        // The current translation catalog.
        private static Catalog Catalog;
        // The available translation catalogs.
        private static Dictionary<string, Catalog> Catalogs = new Dictionary<string, Catalog>();
        // The current CultureInfo.
        private static CultureInfo _Culture;
        // The exposed current CultureInfo.
        public static CultureInfo Culture { get { return _Culture; } }
        // The default catalog.
        private static Catalog DefaultCatalog;
        // The default catalog path.
        public static readonly string DefaultCatalogDir = "en";
        // The default language of non-translated messages.
        public static readonly string DefaultLanguage = "en-US";
        // The default language name of non-translated messages.
        public static readonly string DefaultLanguageName = "English";
        // The flag whether default language is being used (i.e. no translation occurs).
        private static bool IsDefault;
        // The current language.
        private static string _Language;
        // The exposed current language.
        public static string Language { get { return _Language; } }
        // The current display language name.
        private static string _LanguageName;
        // The exposed current display language name.
        public static string LanguageName { get { return _LanguageName; } }
        // The directory name of locale directory in application root folder.
        private static readonly string LocaleDir = "Locale";

        // Initialize everything needed for localization to work with defalult language in case Initialize wasn't invoked.
        static L10n()
        {
            // Initial language is default language.
            Catalog = DefaultCatalog = Catalog.CreateDefault(Path.Combine(LocaleDir, DefaultCatalogDir), DefaultLanguage, DefaultLanguageName);
            IsDefault = true;
            _Language = DefaultLanguage;
            _LanguageName = DefaultLanguageName;
            _Culture = CultureInfo.CreateSpecificCulture(_Language);
        }

        // Applies current language to application resources.
        private static void Apply()
        {
            // Set culture for current thread.
            System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
        }

        // Returns available languages.
        // Key: language (e.g. "en-US")
        // Value: display language name (e.g. "English")
        public static Dictionary<string, string> GetLanguages()
        {
            Dictionary<string, string> languages = new Dictionary<string, string>();

            // All available catalogs.
            foreach (string language in Catalogs.Keys)
                languages.Add(language, Catalogs[language].LanguageName);

            return languages;
        }

        // Initializes localization.
        public static void Initialize(PersistentData data)
        {
            ScanLocaleDirectory();

            // TODO: Get UI language from persistent data.
            string language = null;

            // No language in options, try to match OS language.
            if (language == null)
                language = MatchLanguage(CultureInfo.InstalledUICulture);

            // Apply default language if no suitable language was found.
            if (language == null)
                Apply();
            else
            {
                // Apply default language, if language change fails.
                if (!SetLanguage(language))
                    Apply();
            }
        }

        // Find most suitable translation catalog for specified language.
        private static string MatchLanguage(string match)
        {
            // Null or empty, no match.
            if (string.IsNullOrEmpty(match)) return null;

            // Do exact match first.
            if (Catalogs.ContainsKey(match))
                return match;

            // Match primary subtag.
            string[] matchSubtags = match.Split('-');
            foreach (string language in Catalogs.Keys)
            {
                string[] subtags = language.Split('-');

                if (subtags[0] == matchSubtags[0])
                    return language;
            }

            return null;
        }

        // Find most suitable translation catalog for specified culture.
        private static string MatchLanguage(CultureInfo culture)
        {
            return MatchLanguage(culture.Name);
        }

        // Translates message.
        // NULL message is translated to NULL.
        // Message without existing translation will be returned untranslated.
        public static string Message(string message, string context = null)
        {
            return IsDefault || message == null ? message : (Catalog.Message(message, context) ?? message);
        }

        // Translates plural message.
        // NULL message is translated to NULL.
        // Message without existing translation will be returned untranslated.
        public static string Plural(string message, string plural, uint n, string context = null)
        {
            if (IsDefault)
                return n == 1 || message == null ? message : plural;

            return message == null ? null : (Catalog.Plural(message, n, context) ?? (n == 1 ? message : plural));
        }

        // Reads all text from localized file.
        public static string ReadAllText(string filename)
        {
            string text = Catalog.ReadAllText(filename);
            if (text == null)
            {
                // Try to fallback to default language.
                if (!IsDefault)
                    text = DefaultCatalog.ReadAllText(filename);
            }

            return text;
        }

        // Scans locale directory for available catalogs.
        private static void ScanLocaleDirectory()
        {
            Catalogs = new Dictionary<string, Catalog>();

            // Add default catalog.
            Catalogs.Add(DefaultLanguage, DefaultCatalog);

            if (Directory.Exists(LocaleDir))
            {
                DirectoryInfo dirLocale = new DirectoryInfo(LocaleDir);

                foreach (DirectoryInfo dirLanguage in dirLocale.GetDirectories())
                {
                    // Skip default catalog path (it contains only resources).
                    if (dirLanguage.Name == DefaultCatalogDir) continue;

                    // Check whether directory name corresponds to supported culture name.
                    CultureInfo culture = null;
                    try
                    {
                        culture = CultureInfo.GetCultureInfo(dirLanguage.Name.Replace('_', '-'));
                    }
                    catch { }
                    if (culture == null) continue; // Not supported language.

                    Catalog catalog = Catalog.Create(dirLanguage.FullName, culture);
                    if (catalog != null)
                        Catalogs.Add(catalog.Name, catalog);
                }
            }
        }

        // Sets current language.
        public static bool SetLanguage(string language)
        {
            // No change.
            if (language == _Language) return true;

            // Set current catalog.
            if (language == DefaultLanguage)
            {
                Catalog = DefaultCatalog;
                IsDefault = true;
            }
            else if (Catalogs.ContainsKey(language))
            {
                // Failed to load translations.
                if (!Catalogs[language].Load()) return false;

                Catalog = Catalogs[language];
                IsDefault = false;
            }
            else return false; // No such language.

            // Set current language, name & culture.
            _Language = Catalog.Name;
            _LanguageName = Catalog.LanguageName;
            _Culture = CultureInfo.CreateSpecificCulture(language);

            // Apply changes.
            Apply();

            return true;
        }
    }
}
