using System;
using Glide;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Library.Glide.CustomLerpers {
    public class PointLerper : MemberLerper {

        private Point _pointFrom;
        private Point _pointTo;
        private Point _pointRange;

        public override void Initialize(object fromValue, object toValue, Behavior behavior) {
            _pointFrom = (Point) fromValue;
            _pointTo = (Point) toValue;
            _pointRange = _pointTo - _pointFrom;
        }

        public override object Interpolate(float t, object currentValue, Behavior behavior) {
            float x = _pointFrom.X + _pointRange.X * t;
            float y = _pointFrom.Y + _pointRange.Y * t;

            // Only a subtle difference since Point only supports int anyways
            if (behavior.HasFlag(Behavior.Round)) {
                x = (float) Math.Round(x);
                y = (float) Math.Round(y);
            }

            var current = (Point) currentValue;

            if (_pointRange.X != 0) current.X = (int) x;
            if (_pointRange.Y != 0) current.Y = (int) y;

            return current;
        }

    }
}
