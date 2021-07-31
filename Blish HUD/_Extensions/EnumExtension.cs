using System;
using System.ComponentModel;
using System.Reflection;
namespace Blish_HUD {
    public static class EnumExtension {
        /// <summary>
        /// Gets the description for a given value of an enum that has the Description attribute.
        /// </summary>
        /// <returns>The description of the enumerated value or its name if it has no description.</returns>
        public static string GetDescription(this Enum value) {
            Type   type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    if (Attribute.GetCustomAttribute(field,
                                                     typeof(DescriptionAttribute)) is DescriptionAttribute attr) {
                        return attr.Description;
                    }
                }
            }
            return name;
        }
    }
}
