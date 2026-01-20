using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClosedXML.Excel;


namespace CrawlFB_PW._1._0.DAO
{
    public class PathHelper
    {
        private static PathHelper instance;
        public static PathHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new PathHelper();
                return instance;
            }
            private set { instance = value; }
        }

        private PathHelper() { }

        /// <summary>
        /// Trả về đường dẫn thư mục Data\Profile nằm cùng nơi file exe
        /// </summary>
        public string GetProfilesFolder()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory?.Trim() ?? Directory.GetCurrentDirectory();
            string folder = Path.Combine(baseDir, "Data", "Profile");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
        public string GetDatPostPageFolder()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory?.Trim() ?? Directory.GetCurrentDirectory();
            string folder = Path.Combine(baseDir, "Data", "Page");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
        public string GetCacheFolder()
        {
            // thư mục chứa exe đang chạy
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // thư mục cache nằm cạnh exe
            string cacheDir = Path.Combine(exeDir, "crawl_data", "cache");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            return cacheDir;
        }
        /// <summary>
        /// Trả về đường dẫn file profiles.json (mặc định)
        /// </summary>
        public string GetProfilesFilePath()
        {
            return Path.Combine(GetProfilesFolder(), "profiles.json");
        }

        /// <summary>
        /// Tạo thư mục Logfile nếu chưa có (nếu bạn muốn log dùng chung)
        /// </summary>
        public string GetLogFolder()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory?.Trim() ?? Directory.GetCurrentDirectory();
            string folder = Path.Combine(baseDir, "Logfile");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        /// <summary>
        /// Trả về đường dẫn file log chung (logfile.txt)
        /// </summary>
        public string GetLogFilePath()
        {
            return Path.Combine(GetLogFolder(), "logfile.txt");
        }
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string ToSafeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "unknown";

            // 1) convert unicode -> no mark
            string noUnicode = RemoveUnicode(raw);

            // 2) bỏ ký tự lạ, giữ a-zA-Z0-9 và _
            string cleaned = Regex.Replace(noUnicode, @"[^a-zA-Z0-9]+", "_");

            // 3) loại bỏ _ thừa
            cleaned = Regex.Replace(cleaned, "_+", "_").Trim('_');

            return cleaned;
        }
        public string GetMainDatabasePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string folder = Path.Combine(baseDir, "crawl_data", "database");
            Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "MainDatabase.db");
            return dbPath;
        }

        public string GetProfileDatabasePath()
        {
            return Path.Combine(GetProfilesFolder(), "ProfileInfo.db");
        }

    }
}
