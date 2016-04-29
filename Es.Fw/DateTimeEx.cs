using System;
// ReSharper disable UnusedMember.Global

namespace Es.Fw
{
    public static class DateTimeEx
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AsUtc();
        public static readonly TimeSpan Infinite = TimeSpan.FromMilliseconds(-1);

        public static long ToEpochTimeSeconds(this DateTime d)
        {
            return (long) (d.AsUtc() - Epoch).TotalSeconds;
        }

        public static long ToEpochTimeMilliseconds(this DateTime d)
        {
            return (long) (d.AsUtc() - Epoch).TotalMilliseconds;
        }

        public static DateTime? AsUtc(this DateTime? dateTime)
        {
            return dateTime?.AsUtc();
        }

        public static DateTime AsUtc(this DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime();

                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                default:
                    return dateTime;
            }
        }
    }
}