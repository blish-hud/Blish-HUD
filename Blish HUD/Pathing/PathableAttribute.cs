using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Pathing {
    /// <summary>
    /// Represents a content-type agnostic name/value pairing.
    /// </summary>
    public struct PathableAttribute {

        private readonly string _name;
        private readonly string _value;

        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public string Value => _value;

        public PathableAttribute(string name, string value) {
            _name  = name;
            _value = value;
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{_name}={_value}";
        }

    }
}
