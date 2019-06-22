using System.Collections.Generic;
using System.Xml;

namespace Blish_HUD.Pathing.Behaviors {
    public interface ILoadableBehavior {

        void LoadWithAttributes(IEnumerable<XmlAttribute> attributes);

        void Load();

    }
}
