using System;

namespace Xtricate.UnitTests.TestHelpers
{
    public static class Extensions
    {
        /// <summary>
        ///     Gets the period of time in seconds based on input date
        /// </summary>
        /// <param name="date">DateTime value</param>
        /// <returns>Time value in seconds as a long value</returns>
        public static long Epoch(this DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var unixTimeSpan = date - unixEpoch;
            return (long) unixTimeSpan.TotalSeconds;
        }

        public static void Times(this int repeatCount, Action<int> action)
        {
            for (var i = 1; i <= repeatCount; i++)
                action(i);
        }
    }
}