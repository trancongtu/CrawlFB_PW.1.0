using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CrawlFB_PW._1._0.DAO
{
    public class AutoDAO
    {
        private string dbPath => PathHelper.Instance.GetMainDatabasePath();

        private SQLiteConnection Conn()
            => SqliteHelper.Instance.GetConnection(dbPath);

        // ===========================
        // LẤY DANH SÁCH PAGE AUTO
        // ===========================
        public List<string> GetMonitorPageIDs()
        {
            var list = new List<string>();

            using (var conn = Conn())
            {
                conn.Open();
                string sql = "SELECT PageID FROM TablePageMonitor WHERE IsAuto = 1";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(rd.GetString(0));
                }
            }

            return list;
        }

        // ===========================
        // PAGE BẮT ĐẦU CHẠY
        // ===========================
        public void SetRunning(string pageId)
        {
            using (var conn = Conn())
            {
                conn.Open();

                string sql = @"UPDATE TablePageMonitor
                               SET Status='Running',
                                   LastScanTime=@t
                               WHERE PageID=@id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pageId);
                    cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===========================
        // PAGE KẾT THÚC CHẠY
        // ===========================
        public void SetDone(string pageId, int posts)
        {
            using (var conn = Conn())
            {
                conn.Open();

                string sql = @"UPDATE TablePageMonitor
                               SET Status='Done',
                                   TotalPostsScanned=@p,
                                   LastScanTime=@t
                               WHERE PageID=@id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pageId);
                    cmd.Parameters.AddWithValue("@p", posts);
                    cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===========================
        // ĐẾM PAGE GIÁM SÁT
        // ===========================
        public int CountMonitorPages()
        {
            using (var conn = Conn())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM TablePageMonitor WHERE IsAuto = 1";

                using (var cmd = new SQLiteCommand(sql, conn))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
