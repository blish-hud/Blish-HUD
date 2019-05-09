using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Controls {

    public class MovedEventArgs : EventArgs {
        public Point PreviousLocation { get; }
        public Point CurrentLocation  { get; }

        public MovedEventArgs(Point previousLocation, Point currentLocation) {
            this.PreviousLocation = previousLocation;
            this.CurrentLocation  = currentLocation;
        }
    }

}
