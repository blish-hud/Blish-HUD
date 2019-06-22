using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Pathing.Behaviors.Activator {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WhenActivatorNameAttribute : Attribute {

        public static IEnumerable<Type> GetTypes(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(WhenActivatorNameAttribute), true).Any()) {
                    yield return type;
                }
            }
        }

        public static WhenActivatorNameAttribute GetAttributesOnType(Type type) {
            return (WhenActivatorNameAttribute)type.GetCustomAttribute(typeof(WhenActivatorNameAttribute), true);
        }

        public string ActivatorName { get; }

        /// <summary>
        /// Identifies the *-when value that loads this activator.
        /// </summary>
        /// <param name="attributeName">The name of the activator.  This match is not case-sensitive.</param>
        public WhenActivatorNameAttribute(string attributePrefix) {
            this.ActivatorName = attributePrefix;
        }

    }

}
