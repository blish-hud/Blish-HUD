using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {

    public class ResizedEventArgs : EventArgs {
        public Point PreviousSize { get; }
        public Point CurrentSize  { get; }

        public ResizedEventArgs(Point previousSize, Point currentSize) {
            this.PreviousSize = previousSize;
            this.CurrentSize  = currentSize;
        }
    }

}
