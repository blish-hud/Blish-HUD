using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Pathing.Behaviors {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IdentifyingBehaviorAttributePrefixAttribute : Attribute {

        public static IEnumerable<Type> GetTypes(Assembly assembly) {
            foreach (Type type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(IdentifyingBehaviorAttributePrefixAttribute), true).Length > 0) {
                    yield return type;
                }
            }
        }

        public static List<IdentifyingBehaviorAttributePrefixAttribute> GetAttributesOnType(Type type) {
            return type.GetCustomAttributes(typeof(IdentifyingBehaviorAttributePrefixAttribute), true).Cast<IdentifyingBehaviorAttributePrefixAttribute>().ToList();
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
