using System;

namespace Blish_HUD {
    public static class TimeSpanExtension
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, int multiplier)
        {
            return TimeSpan.FromTicks(timeSpan.Ticks * multiplier);
        }

        public static TimeSpan Multiply(this TimeSpan timeSpan, decimal multiplier)
        {
            return TimeSpan.FromTicks((long)(timeSpan.Ticks * multiplier));
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, int dividor)
        {
            return TimeSpan.FromTicks(timeSpan.Ticks / dividor);
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, decimal dividor)
        {
            return TimeSpan.FromTicks((long)(timeSpan.Ticks / dividor));
        }
    }
}