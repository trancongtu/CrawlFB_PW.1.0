using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper.Data
{
    public static class DataReaderExtensions
    {
        public static T GetEnum<T>(this IDataRecord rd, string column, T defaultValue)
     where T : struct, Enum
        {
            var val = rd[column];

            if (val == DBNull.Value || val == null)
                return defaultValue;

            // 🔥 nếu là số → cast thẳng
            if (val is int i)
                return (T)Enum.ToObject(typeof(T), i);

            return Enum.TryParse<T>(val.ToString(), true, out var result)
                ? result
                : defaultValue;
        }
        public static string GetStringOrNull(this IDataRecord rd, string column)
        {
            var val = rd[column];
            return val == DBNull.Value ? null : val.ToString();
        }
        public static int? GetIntOrNull(this IDataRecord rd, string column)
        {
            var val = rd[column];
            return val == DBNull.Value ? (int?)null : Convert.ToInt32(val);
        }
        public static DateTime? GetDateTimeOrNull(this IDataRecord rd, string column)
        {
            var val = rd[column];
            return val == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(val);
        }
    }
}
