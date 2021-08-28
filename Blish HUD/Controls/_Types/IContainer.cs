using System.Collections.Generic;

namespace Blish_HUD.Controls {
    public interface IContainer {

        ControlCollection<Control> Children { get; }

        IEnumerable<Control> GetDescendants();

    }
}
