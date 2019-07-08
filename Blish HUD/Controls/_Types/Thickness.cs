using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Controls {

    /// <summary>
    /// Used to define a 'thickness' around each side of a rectangle.
    /// </summary>
    public struct Thickness {

        public float Top { get; }
        public float Right { get; }
        public float Bottom { get; }
        public float Left { get; }

        #region Constructors

        public Thickness(float topThickness, float rightThickness, float bottomThickness, float leftThickness) {
            Top    = topThickness;
            Right  = rightThickness;
            Bottom = bottomThickness;
            Left   = leftThickness;
        }

        public Thickness(float verticalThickness, float horizontalThickness) : this(verticalThickness, horizontalThickness, verticalThickness, horizontalThickness) { /* ALIAS */ }

        public Thickness(float topThickness,      float horizontalThickness, float bottomThickness) : this(topThickness, horizontalThickness, bottomThickness, horizontalThickness) { /* ALIAS */ }

        public Thickness(float thickness) : this(thickness, thickness, thickness, thickness) { /* ALIAS */ }

        #endregion
        
        #region Operator Behavior
        public static Thickness operator +(Thickness t1, Thickness t2) {
            return new Thickness(t1.Top + t2.Top, t1.Right + t2.Right, t1.Bottom + t2.Bottom, t1.Left + t2.Left);
        }
        public static Thickness operator -(Thickness t1, Thickness t2) {
            return new Thickness(t1.Top - t2.Top, t1.Right - t2.Right, t1.Bottom - t2.Bottom, t1.Left - t2.Left);
        }

        #endregion

        /// <summary>
        /// Represents a <see cref="Thickness"/> where the <see cref="Top"/>, <see cref="Right"/>, <see cref="Bottom"/>, and <see cref="Left"/> are 0.
        /// </summary>
        public static Thickness Zero = new Thickness(0, 0, 0, 0);

    }
}
