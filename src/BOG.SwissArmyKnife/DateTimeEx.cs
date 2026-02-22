using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOG.SwissArmyKnife.Extensions
{
    public static class DateTimeEx
    {
        /// <summary>
        /// Return the earliest datetime entry in an array of datetimes, or the original date, 
        /// whichever is chronologically earlier.
        /// </summary>
        /// <param name="datetimes">list of DateTime values.</param>
        /// <returns>The chronological earliest value found.</returns>
        public static DateTime Earliest(this DateTime original, DateTime[] datetimes)
        {
            var result = original;
            foreach (var dt in datetimes) if (dt < result) result = dt;
            return result;
        }

        /// <summary>
        /// Return the latest datetime entry in an array of datetimes, or the original date, 
        /// whichever is chronologically latest.
        /// </summary>
        /// <param name="datetimes">list of DateTime values.</param>
        /// <returns>The chronological latest value from the set</returns>
        public static DateTime Latest(this DateTime original, DateTime[] datetimes)
        {
            var result = original;
            foreach (var dt in datetimes) if (dt > result) result = dt;
            return result;
        }
    }
}
