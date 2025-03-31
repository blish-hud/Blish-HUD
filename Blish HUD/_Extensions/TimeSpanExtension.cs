using System;

namespace Blish_HUD {
    public static class TimeSpanExtension {
        public static TimeSpan Multiply(this TimeSpan timeSpan, int multiplier) => TimeSpan.FromTicks(timeSpan.Ticks * multiplier);

        public static TimeSpan Multiply(this TimeSpan timeSpan, decimal multiplier) => TimeSpan.FromTicks((long)(timeSpan.Ticks * multiplier));

        public static TimeSpan Divide(this TimeSpan timeSpan, int dividor) => TimeSpan.FromTicks(timeSpan.Ticks / dividor);

        public static TimeSpan Divide(this TimeSpan timeSpan, decimal dividor) => TimeSpan.FromTicks((long)(timeSpan.Ticks / dividor));
    }
}
