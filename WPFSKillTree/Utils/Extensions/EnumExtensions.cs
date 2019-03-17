using System;
using System.ComponentModel;
using System.Reflection;

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
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}