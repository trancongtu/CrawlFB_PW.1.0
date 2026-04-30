using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper.Data
{
    public static class DataRowExtensions
    {
        public static T GetEnum<T>(this DataRow row, string column, T defaultValue)
            where T : struct, Enum
        {
            var val = row[column];

            if (val == DBNull.Value || val == null)
                return defaultValue;

            return Enum.TryParse<T>(val.ToString(), true, out var result)
                ? result
                : defaultValue;
        }
    }
}
