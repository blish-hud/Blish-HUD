using System;

namespace Glide
{
	public abstract class MemberLerper
	{
		[Flags]
		public enum Behavior
		{
			None = 0,
			Reflect = 1,
			Rotation = 2,
			RotationRadians = 4,
			RotationDegrees = 8,
			Round = 16
		}
		
		protected const float DEG = 180f / (float) Math.PI;
		protected const float RAD = (float) Math.PI / 180f;
		
		public abstract void Initialize(Object fromValue, Object toValue, Behavior behavior);
		public abstract object Interpolate(float t, object currentValue, Behavior behavior);
	}
}
