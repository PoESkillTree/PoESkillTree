using System;
using Newtonsoft.Json.Linq;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.JsonSettings
{
    /// <summary>
    /// Simple implementation of a setting without sub settings.
    /// Represents a key value pair with a simple value type (supported by <see cref="JValue"/>).
    /// </summary>
    /// <typeparam name="T">Type of the values of this setting.</typeparam>
    public class LeafSetting<T> : Notifier, ISetting
    {
        private T _value;
        private readonly string _key;
        private readonly Action _onChanged;
        private readonly Action<T> _onChanging;
        private readonly T _defaultValue;

        /// <summary>
        /// Gets or sets the value of this setting.
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value, _onChanged, _onChanging); }
        }

        /// <param name="key">The key of this setting.</param>
        /// <param name="defaultValue">The default value of this setting. <see cref="Value"/> will be initialised
        /// with this.</param>
        /// <param name="onChanged">An action that will be called when <see cref="Value"/> was changed.
        /// (see <see cref="Notifier.SetProperty{T}"/>'s onChanged parameter)</param>
        /// <param name="onChanging">An action that will be called when <see cref="Value"/> is changing.
        /// (see <see cref="Notifier.SetProperty{T}"/>'s onChanging parameter)</param>
        public LeafSetting(string key, T defaultValue, Action onChanged = null, Action<T> onChanging = null)
        {
            _key = key;
            _defaultValue = Value = defaultValue;
            _onChanged = onChanged;
            _onChanging = onChanging;
        }

        public void LoadFrom(JObject jObject)
        {
            JToken token;
            Value = jObject.TryGetValue(_key, out token) ? token.ToObject<T>() : _defaultValue;
        }

        public bool SaveTo(JObject jObject)
        {
            var newToken = new JValue(Value);
            var changed = !Equals(Value, _defaultValue);
            JToken oldToken;
            if (jObject.TryGetValue(_key, out oldToken))
            {
                changed = !JToken.DeepEquals(newToken, oldToken);
            }
            jObject[_key] = newToken;
            return changed;
        }

        public void Reset()
        {
            Value = _defaultValue;
        }
    }
}