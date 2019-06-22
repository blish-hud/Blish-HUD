using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blish_HUD.Pathing.Behaviors {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IdentifyingBehaviorAttributePrefixAttribute : Attribute {

        public static IEnumerable<Type> GetTypes(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(IdentifyingBehaviorAttributePrefixAttribute), true).Any()) {
                    yield return type;
                }
            }
        }

        public static IdentifyingBehaviorAttributePrefixAttribute GetAttributesOnType(Type type) {
            return (IdentifyingBehaviorAttributePrefixAttribute)type.GetCustomAttribute(typeof(IdentifyingBehaviorAttributePrefixAttribute), true);
        }

        public string AttributePrefix { get; }

        /// <summary>
        /// Identifies the attribute required in order to activate a behavior on a pathable.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.  This match is not case-sensitive.</param>
        public IdentifyingBehaviorAttributePrefixAttribute(string attributePrefix) {
            this.AttributePrefix = attributePrefix;
        }

    }
}
