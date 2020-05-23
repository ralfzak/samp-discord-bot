using System;
using System.Collections.Generic;
using System.Text;

namespace main.Core
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }

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
        public static string ToHumanReadableString(this DateTime dateTimeUtc)
        {
            return dateTimeUtc.ToString("dddd, dd MMMM yyyy");
        }
    }
}
