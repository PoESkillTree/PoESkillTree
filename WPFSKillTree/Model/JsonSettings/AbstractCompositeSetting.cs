using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils;

namespace POESKillTree.Model.JsonSettings
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    /// <summary>
    /// Abstract base class for composite settings that assemble multiple sub settings under one key in the
    /// JSON object tree. Implementations only need to provide the key and the sub settings.
    /// </summary>
    public abstract class AbstractCompositeSetting : Notifier, ISetting
    {
        /// <summary>
        /// Gets the key of these settings.
        /// </summary>
        protected abstract string Key { get; }

        /// <summary>
        /// Gets the sub settings assembled under this setting.
        /// </summary>
        protected abstract IReadOnlyList<ISetting> SubSettings { get; }

        public void LoadFrom(JObject jObject)
        {
            try
            {
                JToken token;
#if (DEBUG)
                string DebugTest = jObject.ToString();
                if (DebugTest != null) { System.Console.WriteLine("Loaded jObject value " + DebugTest); }
#endif
                if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
                {
                    Reset();
                    return;
                }
                SubSettings.ForEach(s => s.LoadFrom((JObject)token));
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine("Loaded AbstractCompositeSetting JObject Exception of " + ex.ToString());
            }
        }

        public bool SaveTo(JObject jObject)
        {
            try
            {
                JToken token;
#if (DEBUG)
                string DebugTest = jObject.ToString();
                if (DebugTest != null) { System.Console.WriteLine("Saved jObject value" + DebugTest); }
#endif
                if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
                {
                    jObject[Key] = token = new JObject();
                }
                if (!SubSettings.Any())
                    return false;
                var obj = (JObject)token;
                var changed = false;
                foreach (var s in SubSettings)
                {
                    if (s.SaveTo(obj))
                        changed = true;
                }
                return changed;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Saved AbstractCompositeSetting JObject Exception of " + ex.ToString());
            }
            return false;
        }

        public void Reset()
        {
            SubSettings.ForEach(s => s.Reset());
        }
    }
}