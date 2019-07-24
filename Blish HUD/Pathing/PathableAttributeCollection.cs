using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;

namespace Blish_HUD.Pathing {
    /// <summary>
    /// A collection of <see cref="PathableAttribute"/>.
    /// </summary>
    public class PathableAttributeCollection : KeyedCollection<string, PathableAttribute> {

        public PathableAttributeCollection() : base(StringComparer.OrdinalIgnoreCase) { /* NOOP */ }

        /// <summary>
        /// Create a <see cref="PathableAttributeCollection"/> from an existing <see cref="IEnumerable{PathableAttribute}"/>.
        /// </summary>
        /// <param name="attributeCollection"></param>
        public PathableAttributeCollection(IEnumerable<PathableAttribute> attributeCollection) : base(StringComparer.OrdinalIgnoreCase) {
            this.AddRange(attributeCollection);
        }

        public void SetOrUpdateAttributes(PathableAttributeCollection attributes) {
            foreach (var newAttribute in attributes) {
                if (this.Contains(newAttribute.Name)) {
                    this.Remove(newAttribute.Name);
                }
                this.Add(newAttribute);
            }
        }

        /// <inheritdoc />
        protected override string GetKeyForItem(PathableAttribute item) => item.Name;

        /// <inheritdoc />
        public override string ToString() {
            return string.Join(", ", this);
        }

    }
}
