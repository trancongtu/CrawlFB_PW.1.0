using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using DevExpress.Skins;
using DevExpress.UserSkins;


namespace CrawlFB_PW._1._0
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            InitAppFolders();                
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FMain());
        }
        private static void InitAppFolders()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string[] folders =
                {
            Path.Combine(baseDir, "crawl_data"),
            Path.Combine(baseDir, "crawl_data", "database"),
            Path.Combine(baseDir, "crawl_data", "cache"),
            Path.Combine(baseDir, "crawl_data", "exports"),
            Path.Combine(baseDir, "crawl_data", "logs"),

            // ⭐ DB2 nằm đây
            Path.Combine(baseDir, "Data"),
            Path.Combine(baseDir, "Data", "Profile")};
                foreach (var folder in folders)
                    Directory.CreateDirectory(folder);

                Console.WriteLine("✅ Tạo cấu trúc thư mục thành công.");
             
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi tạo thư mục hệ thống: " + ex.Message);
            }
        }


    }
}
