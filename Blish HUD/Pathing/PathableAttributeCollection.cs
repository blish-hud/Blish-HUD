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
    public sealed class PathableAttributeCollection : KeyedCollection<string, PathableAttribute> {

        public PathableAttributeCollection() : base(StringComparer.OrdinalIgnoreCase) { /* NOOP */ }

        /// <summary>
        /// Create a <see cref="PathableAttributeCollection"/> from an existing <see cref="IEnumerable{PathableAttribute}"/>.
        /// </summary>
        /// <param name="attributeCollection"></param>
        public PathableAttributeCollection(IEnumerable<PathableAttribute> attributeCollection) : base(StringComparer.OrdinalIgnoreCase) {
            this.AddRange(attributeCollection.GroupBy(a => a.Name).Select(g => g.Last()));
        }

        /// <summary>
        /// If an attribute with the provided <param name="attribute"></param>s
        /// name already exists, it is replaced with the provided attribute.
        /// Otherwise, the attribute is added to the collection.
        /// </summary>
        /// <param name="attribute">The attribute to update or insert into the collection.</param>
        public void AddOrUpdateAttribute(PathableAttribute attribute) {
            if (this.Contains(attribute.Name)) {
                this.Remove(attribute.Name);
            }
            this.Add(attribute);
        }

        /// <summary>
        /// Calls <see cref="AddOrUpdateAttribute"/> on each of the provided attributes
        /// in <param name="attributes"></param>.
        /// </summary>
        /// <param name="attributes">The <see cref="PathableAttribute"/>s to add to the collection.</param>
        public void AddOrUpdateAttributes(IEnumerable<PathableAttribute> attributes) {
            foreach (var attribute in attributes) {
                AddOrUpdateAttribute(attribute);
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
