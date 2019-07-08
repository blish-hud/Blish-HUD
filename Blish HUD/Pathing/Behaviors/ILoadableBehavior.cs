using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Blish_HUD.Pathing.Behaviors {
    public interface ILoadableBehavior {

        void LoadWithAttributes(IEnumerable<XmlAttribute> attributes);

    }
}
