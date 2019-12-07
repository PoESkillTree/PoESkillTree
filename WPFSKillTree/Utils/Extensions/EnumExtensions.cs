using System;
using System.ComponentModel;

namespace PoESkillTree.Utils.Extensions
{
    /// <summary>
    /// Provides extension methods for Enums.
    /// </summary>
    public static class EnumExtensions
    {
        // source: http://stackoverflow.com/a/1415187
        /// <summary>
        /// Returns the value of the Description attribute of the given Enum or null
        /// if it has no such attribute.
        /// </summary>
        public static string? GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}