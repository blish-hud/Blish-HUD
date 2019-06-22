using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blish_HUD.Pathing.Behaviors {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PathingBehaviorAttribute : Attribute {

        public static IEnumerable<Type> GetTypes(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(PathingBehaviorAttribute), true).Any()) {
                    yield return type;
                }
            }
        }

        public static PathingBehaviorAttribute GetAttributesOnType(Type type) {
            return (PathingBehaviorAttribute)type.GetCustomAttribute(typeof(PathingBehaviorAttribute), true);
        }

        public string AttributePrefix { get; }

        /// <summary>
        /// Identifies the attribute required in order to activate a behavior on a pathable.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.  This match is not case-sensitive.</param>
        public PathingBehaviorAttribute(string attributePrefix) {
            this.AttributePrefix = attributePrefix;
        }

    }
}
