using Newtonsoft.Json.Linq;

namespace PoESkillTree.Model.JsonSettings
{
    /// <summary>
    /// Interface for setting classes that can be converted to and from JSON.
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Loads this component's values from <paramref name="jObject"/>.
        /// </summary>
        void LoadFrom(JObject jObject);
        /// <summary>
        /// Saves this component's values to <paramref name="jObject"/>.
        /// </summary>
        /// <returns>True iff this operation changed <paramref name="jObject"/></returns>
        bool SaveTo(JObject jObject);
        /// <summary>
        /// Resets this component's values to their default values.
        /// </summary>
        void Reset();
    }
}