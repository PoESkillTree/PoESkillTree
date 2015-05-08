using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using POESKillTree.Model;

/* TODO:
 * - Catalog.Unload() to unload previously used Catalog (clear messages, etc.), only one catalog should be loaded at a time (two during switch).
 * - SetLanguage must return bool to indicate successul language change.
 * - MakrdownSharp NuGet package for displaying md documents with XAML (for localized Help).
 * - Some tool to remove potential BOM header from translation catalogs before msgmerge, in case someone is editing them in VS/Notepad/etc.
 * - en/en-US locale folder must exists containing Help.md, must be excluded from BuildLocaleCatalog target.
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
        // The file name of language catalog.
        private static readonly string LanguageCatalogFilename = "Messages.txt";
        // The directory name of locale directory in application root folder.
        private static readonly string LocaleDirectoryName = "Locale";

        static L10n()
        {
            // Initial language is default language.
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
        // Key: language (e.g. en-US)
        // Value: display language name (e.g. English)
        public static Dictionary<string, string> GetLanguages()
        {
            Dictionary<string, string> languages = new Dictionary<string, string>()
            {
                { DefaultLanguage, DefaultLanguageName }
            };

            // All available catalogs.
            foreach (string language in Catalogs.Keys)
                languages.Add(language, Catalogs[language].LanguageName);

            return languages;
        }

        // Initializes localization.
        public static void Initialize(PersistentData data)
        {
            ScanLocaleDirectory();

            string language = null;
            // TODO: Get UI language from persistent data.

            // No language in options, try to match OS language.
            if (language == null)
                language = MatchLanguage(CultureInfo.InstalledUICulture);

            // Apply default language if no suitable language was found.
            if (language == null)
                Apply();
            else
                SetLanguage(language);
        }

        // Find most suitable translation catalog for specified culture.
        private static string MatchLanguage(CultureInfo culture)
        {
            string match = culture.Name;

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

        // Scans locale directory for available catalogs.
        private static void ScanLocaleDirectory()
        {
            if (Directory.Exists(LocaleDirectoryName))
            {
                DirectoryInfo dirLocale = new DirectoryInfo(LocaleDirectoryName);

                foreach (DirectoryInfo dirLanguage in dirLocale.GetDirectories())
                {
                    // Check whether directory name corresponds to supported culture name.
                    CultureInfo culture = null;
                    try
                    {
                        culture = CultureInfo.GetCultureInfo(dirLanguage.Name.Replace('_', '-'));
                    }
                    catch { }
                    if (culture == null) continue; // Not supported language.

                    string path = Path.Combine(dirLanguage.FullName, LanguageCatalogFilename);
                    if (File.Exists(path))
                    {
                        Catalog catalog = Catalog.Create(path, culture);
                        if (catalog != null)
                            Catalogs.Add(catalog.Name, catalog);
                    }
                }
            }
        }

        // Sets current language.
        public static void SetLanguage(string language)
        {
            // No change.
            if (language == _Language) return;

            // Set current catalog.
            if (language == DefaultLanguage)
            {
                // No catalog for default language.
                Catalog = null;
                IsDefault = true;
                _LanguageName = DefaultLanguageName;
            }
            else if (Catalogs.ContainsKey(language))
            {
                // Failed to load translations.
                if (!Catalogs[language].Load()) return;

                Catalog = Catalogs[language];
                IsDefault = false;
                _LanguageName = Catalog.LanguageName;
            }
            else return; // No such language.

            // Set current language & culture.
            _Language = language;
            _Culture = CultureInfo.CreateSpecificCulture(language);

            // Apply changes.
            Apply();
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
    }
}
