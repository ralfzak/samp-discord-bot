using System;

namespace main.Core
{
    /// <summary>
    /// Current time provider class.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Provides the current application time.
        /// </summary>
        DateTime Now { get; }
        
        /// <summary>
        /// Provides the current time in UTC. 
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Calculates the seconds difference from epoch timestamp
        /// </summary>
        /// <param name="ticks">Number of seconds</param>
        /// <returns>Number of seconds from epoch time difference</returns>
        long GetElapsedFromEpoch(long ticks);
    }

    public class CurrentTimeProvider : ITimeProvider
    {
        public virtual DateTime UtcNow => DateTime.UtcNow;
        public virtual DateTime Now => DateTime.Now;

        public virtual long GetElapsedFromEpoch(long ticks) =>
            new TimeSpan(ticks).Subtract(new TimeSpan(UtcNow.Ticks)).Ticks;
    }

    public static class DateTimeExtension
    {
        /// <summary>
        /// Converts a given time object to a readable string in the form: dddd, dd MMMM yyyy.
        /// 
        /// That is "01-01-2020" translates to "Wednesday, 01 January 2020".
        /// </summary>
        /// <param name="dateTimeUtc"></param>
        /// <returns>A human readable <paramref name="dateTimeUtc"/> string</returns>
        public static string ToHumanReadableString(this DateTime dateTimeUtc)
        {
            return dateTimeUtc.ToString("dddd, dd MMMM yyyy");
        }
    }
}
