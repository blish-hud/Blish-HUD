using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Blish_HUD {
    public enum TyrianTime {
        /// <summary>
        /// Escape
        /// </summary>
        [EnumMember(Value = "none")] None,
        /// <summary>
        /// 5 minutes 05:00–06:00
        /// </summary>
        [EnumMember(Value = "dawn")] Dawn,
        /// <summary>
        /// 70 minutes 06:00–20:00
        /// </summary>
        [EnumMember(Value = "day")] Day,
        /// <summary>
        /// 5 minutes 20:00–21:00
        /// </summary>
        [EnumMember(Value = "dusk")] Dusk,
        /// <summary>
        /// 40 minutes 21:00–05:00
        /// </summary>
        [EnumMember(Value = "night")] Night
    }
    public static class TyrianTimeUtil
    {
        private static IReadOnlyDictionary<TyrianTime, (TimeSpan,TimeSpan)> _timeInterval = new Dictionary<TyrianTime, (TimeSpan, TimeSpan)>() {
            { TyrianTime.Dawn, (new TimeSpan(05,0,0), new TimeSpan(06,0,0)) },
            { TyrianTime.Day, (new TimeSpan(06,0,0), new TimeSpan(20,0,0)) },
            { TyrianTime.Dusk, (new TimeSpan(20,0,0), new TimeSpan(21,0,0)) },
            { TyrianTime.Night, (new TimeSpan(21,0,0), new TimeSpan(05,0,0)) }
        };

        /// <summary>
        /// Checks which day cycle currently prevails.
        /// </summary>
        /// <returns>The current day cycle.</returns>
        public static TyrianTime GetDayCycle() {
            var currentTime = DateTime.Now.ToUniversalTime();

            var tyrianTime = FromRealDateTime(currentTime);

            foreach (var timePair in _timeInterval) {
                var key = timePair.Key;
                var value = timePair.Value;

                if (!TimeBetween(tyrianTime, value.Item1, value.Item2))
                    continue;
                return key;
            }
            return TyrianTime.None;
        }

        /// <summary>
        /// Converts a DateTime object to a TimeSpan representing Tyrian time.
        /// </summary>
        /// <param name="realTime">A DateTime object representing real time.</param>
        /// <returns>A TimeSpan representing the Tyrian Time conversion of the input time.</returns>
        public static TimeSpan FromRealDateTime(DateTime realTime)
        {
            TimeSpan currentDayTimespan;
            double currentCycleSeconds;

            // Retrieves a timespan that represents the time from 00:00 of the given realTime to the current time
            // of the given realTime
            currentDayTimespan = realTime - realTime.Date;

            /**
             * A single Tyrian day consists of 7200 real-time seconds, so we divide the current real-time seconds of
             * the day by 7200 and then return the remainder - this gives us the current total seconds that have passed
             * in Tyria for the current Tyrian day/cycle.
             */
            currentCycleSeconds = currentDayTimespan.TotalSeconds % 7200;

            /**
             * For every second that passes in real time, 12 pass in Tyrian time, so we simply need to multiply
             * the current cycle seconds by 12 to convert them from a 2 hour cycle to
             */
            return TimeSpan.FromSeconds(currentCycleSeconds * 12);
        }

        private static bool TimeBetween(TimeSpan time, TimeSpan start, TimeSpan end)
        {
            if (start < end)
                return start <= time && time <= end;
            return !(end < time && time < start);
        }
    }
}