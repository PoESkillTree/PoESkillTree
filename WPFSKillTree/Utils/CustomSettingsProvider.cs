using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PoESkillTree.Utils
{
    // Adjusted version of the one at http://stackoverflow.com/a/11398536 by Chuck Rostance
    // Uses the specified settingsKey to store the file at <Settings.SettingsPath>/<settingsKey>.config
    // This allows for multiple files since this Provider is not able to combine multiple Settings into one file.
    public class CustomSettingsProvider : SettingsProvider
    {
        private const string NameKey = "name";
        private const string SerializeAsKey = "serializeAs";
        private const string ConfigKey = "configuration";
        private const string UserSettingsKey = "userSettings";
        private const string SettingKey = "setting";

        private readonly string _settingsKey;

        /// <summary>
        /// Loads the file into memory.
        /// </summary>
        public CustomSettingsProvider(string settingsKey)
        {
            SettingsDictionary = new Dictionary<string, SettingStruct>();
            _settingsKey = settingsKey;
        }

        /// <summary>
        /// Override.
        /// </summary>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        /// <summary>
        /// Override.
        /// </summary>
        public override string ApplicationName
        {
            get => System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name;
            set
            {
                //do nothing
            }
        }

        /// <summary>
        /// Must override this, this is the bit that matches up the designer properties to the dictionary values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            //load the file
            if (!_loaded)
            {
                _loaded = true;
                LoadValuesFromFile();
            }

            //collection that will be returned.
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            //iterate thought the properties we get from the designer, checking to see if the setting is in the dictionary
            foreach (var setting in collection.OfType<SettingsProperty>())
            {
                SettingsPropertyValue value = new SettingsPropertyValue(setting) {IsDirty = false};

                if (SettingsDictionary.ContainsKey(setting.Name))
                {
                    value.SerializedValue = SettingsDictionary[setting.Name].value;
                }
                else //use defaults in the case where there are no settings yet
                {
                    value.SerializedValue = setting.DefaultValue == null ? string.Empty : setting.DefaultValue.ToString();
                }

                values.Add(value);
            }
            return values;
        }

        /// <summary>
        /// Must override this, this is the bit that does the saving to file.  Called when Settings.Save() is called
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            //grab the values from the collection parameter and update the values in our dictionary.
            foreach (var value in collection.OfType<SettingsPropertyValue>())
            {
                var setting = new SettingStruct
                {
                    value = value.PropertyValue == null ? string.Empty : value.SerializedValue.ToString()!,
                    name = value.Name,
                    serializeAs = value.Property.SerializeAs.ToString()
                };

                if (!SettingsDictionary.ContainsKey(value.Name))
                {
                    SettingsDictionary.Add(value.Name, setting);
                }
                else
                {
                    SettingsDictionary[value.Name] = setting;
                }
            }

            //now that our local dictionary is up-to-date, save it to disk.
            SaveValuesToFile();
        }

        /// <summary>
        /// Loads the values of the file into memory.
        /// </summary>
        private void LoadValuesFromFile()
        {
            if (!File.Exists(UserConfigPath))
            {
                //if the config file is not where it's supposed to be create a new one.
                CreateEmptyConfig();
            }

            try
            {
                //load the xml
                var configXml = XDocument.Load(UserConfigPath);

                //get all of the <setting name="..." serializeAs="..."> elements.
                var settingElements = configXml.Element(ConfigKey)?.Element(UserSettingsKey)?.Elements(SettingKey);

                if (settingElements is null)
                {
                    File.Delete(UserConfigPath);
                    LoadValuesFromFile();
                    return;
                }

                //iterate through, adding them to the dictionary, (checking for nulls, xml no likey nulls)
                //using "String" as default serializeAs...just in case, no real good reason.
                foreach (var element in settingElements)
                {
                    var newSetting = new SettingStruct
                    {
                        name = element.Attribute(NameKey) == null ? string.Empty : element.Attribute(NameKey).Value,
                        serializeAs = element.Attribute(SerializeAsKey) == null ? "String" : element.Attribute(SerializeAsKey).Value,
                        value = element.Value
                    };
                    SettingsDictionary.Add(element.Attribute(NameKey).Value, newSetting);
                }
            }
            catch
            {
                if (File.Exists(UserConfigPath))
                    File.Delete(UserConfigPath);
            }
        }

        /// <summary>
        /// Creates an empty user.config file...looks like the one MS creates.  
        /// This could be overkill a simple key/value pairing would probably do.
        /// </summary>
        private void CreateEmptyConfig()
        {
            var doc = new XDocument();
            var declaration = new XDeclaration("1.0", "utf-8", "true");
            var config = new XElement(ConfigKey);
            var userSettings = new XElement(UserSettingsKey);
            config.Add(userSettings);
            doc.Add(config);
            doc.Declaration = declaration;
            doc.Save(UserConfigPath);
        }

        /// <summary>
        /// Saves the in memory dictionary to the user config file
        /// </summary>
        private void SaveValuesToFile()
        {
            if (!File.Exists(UserConfigPath))
            {
                //if the config file is not where it's supposed to be create a new one.
                CreateEmptyConfig();
            }

            //load the current xml from the file.
            var import = XDocument.Load(UserConfigPath);

            //get the settings group (e.g. <Company.Project.Desktop.Settings>)
            var settingsSection = import.Element(ConfigKey).Element(UserSettingsKey);

            //iterate though the dictionary, either updating the value or adding the new setting.
            foreach (var entry in SettingsDictionary)
            {
                var setting = settingsSection.Elements().FirstOrDefault(e => e.Attribute(NameKey)?.Value == entry.Key);
                if (setting == null) //this can happen if a new setting is added via the .settings designer.
                {
                    var newSetting = new XElement(SettingKey);
                    newSetting.Add(new XAttribute(NameKey, entry.Value.name));
                    newSetting.Add(new XAttribute(SerializeAsKey, entry.Value.serializeAs));
                    newSetting.Value = (entry.Value.value ?? string.Empty);
                    settingsSection.Add(newSetting);
                }
                else //update the value if it exists.
                {
                    setting.Value = (entry.Value.value ?? string.Empty);
                }
            }
            import.Save(UserConfigPath);
        }

        private string UserConfigPath => Path.Combine(AppData.GetFolder("Settings"), _settingsKey) + ".config";

        /// <summary>
        /// In memory storage of the settings values
        /// </summary>
        private Dictionary<string, SettingStruct> SettingsDictionary { get; }

        /// <summary>
        /// Helper struct.
        /// </summary>
        private struct SettingStruct
        {
            internal string name;
            internal string serializeAs;
            internal string value;
        }

        private bool _loaded;
    }
}