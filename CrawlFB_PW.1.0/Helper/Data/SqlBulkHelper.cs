using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace CrawlFB_PW._1._0.Helper.Data
{
    public class SqlBulkHelper
    {
        public static void BulkCopy(SqlConnection conn, string table, DataTable dt)
        {
            if (dt.Rows.Count == 0) return;

            using (var bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = table;
                bulk.WriteToServer(dt);
            }
        }

        public static HashSet<string> LoadExistingPostIds(SqlConnection conn, List<string> ids)
        {
            var result = new HashSet<string>();
            if (ids.Count == 0) return result;

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                var paramNames = ids.Select((x, i) => "@id" + i).ToList();
                cmd.CommandText = $"SELECT PostID FROM TablePostInfo WHERE PostID IN ({string.Join(",", paramNames)})";

                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.AddWithValue(paramNames[i], ids[i]);

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        result.Add(rd.GetString(0));
                }
            }

            return result;
        }
    }
}
