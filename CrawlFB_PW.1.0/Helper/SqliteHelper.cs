using System;
using System.IO;
using System.Data.SQLite;

namespace CrawlFB_PW._1._0.DAO
{
    public class SqliteHelper
    {
        private static SqliteHelper instance;
        public static SqliteHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new SqliteHelper();
                return instance;
            }
        }

        private SqliteHelper() { }
        private const string MAIN_DB = "MainDatabase.db";
        /// <summary>
        /// Lấy connection đến file SQLite.
        /// - Nếu là MainDatabase.db → vào thư mục crawl_data/database/
        /// - Còn lại (mỗi page riêng) → vào thư mục crawl_data/pages/
        /// </summary>
        /*  public SQLiteConnection GetConnection(string dbName = MAIN_DB)
          {
              string baseDir = AppDomain.CurrentDomain.BaseDirectory;

              // ✅ Nếu là đường dẫn tuyệt đối thì dùng luôn
              if (Path.IsPathRooted(dbName))
              {
                  Directory.CreateDirectory(Path.GetDirectoryName(dbName));
                  return new SQLiteConnection($"Data Source={dbName};Version=3;");
              }

              // ✅ Nếu chỉ là tên file (không có path)
              string folder;
              if (dbName.Equals("MainDatabase.db", StringComparison.OrdinalIgnoreCase))
                  folder = Path.Combine(baseDir, "crawl_data", "database");
              else
                  folder = Path.Combine(baseDir, "crawl_data", "cache"); // nên đổi "pages" → "cache" cho đúng chức năng


              Directory.CreateDirectory(folder);
              string dbPath = Path.Combine(folder, dbName);
              return new SQLiteConnection($"Data Source={dbPath};Version=3;");
          }*/
        public SQLiteConnection GetConnection(string dbPath)
        {
            bool isMainDb = false;

            if (!Path.IsPathRooted(dbPath))
            {
                if (dbPath.Equals("MainDatabase.db", StringComparison.OrdinalIgnoreCase))
                {
                    dbPath = PathHelper.Instance.GetMainDatabasePath();
                    isMainDb = true;
                }
                else
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string cacheFolder = Path.Combine(baseDir, "crawl_data", "cache");
                    Directory.CreateDirectory(cacheFolder);
                    dbPath = Path.Combine(cacheFolder, dbPath);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            if (isMainDb)
            {
                // ⭐ MAIN DB = DELETE + NO POOLING (ỔN ĐỊNH, KHÔNG BAO GIỜ LOCK)
                return new SQLiteConnection(
                    $"Data Source={dbPath};Version=3;Journal Mode=DELETE;Pooling=False;Synchronous=Off;BusyTimeout=5000;");
            }
            else
            {
                // ⭐ TEMP DB = DELETE + NO POOLING
                return new SQLiteConnection(
                    $"Data Source={dbPath};Version=3;Journal Mode=DELETE;Pooling=False;Synchronous=Off;BusyTimeout=5000;");
            }
        }

    }
}
