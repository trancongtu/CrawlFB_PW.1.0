using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0
{
    /// <summary>
    /// Cấu hình toàn cục cho toàn bộ ứng dụng
    /// Dễ dàng chỉnh ở 1 nơi, các form khác chỉ đọc
    /// </summary>
    public static class AppConfig
    {
        public static bool ENABLE_DEBUG_LOG = true;
        public static bool ENABLE_TECH_LOG = false;
        public static bool ENABLE_LOG_POST = true;
        // ✅ Bật / tắt hiển thị trình duyệt (Playwright headless mode)
        public static bool HEADLESS_MODE = false;

        // ✅ Bật / tắt ghi log chi tiết (DAO, SupervisePage, Manager, ...)
        public static bool ENABLE_LOG = true;

        // ✅ Bật / tắt ghi HTML debug ra file (chỉ dùng khi test)
        public static bool SAVE_HTML_DEBUG = false;

        // ✅ Bật / tắt chế độ TEST (FormTest chạy nhanh hơn, không ghi Excel,...)
        public static bool TEST_MODE = false;

        // ✅ Thời gian chờ load trang mặc định (ms)
        public static int DEFAULT_TIMEOUT = 45000;

        // ✅ Giới hạn số bài tối đa 1 lần quét
        public static int MAX_POSTS_DEFAULT = 25;
        public static int scrollCount = 10; // số lần cuộn / 1 lần hàm scrool
        // ⚙️ Tốc độ cuộn feed Facebook (ms mỗi lần scroll)
        public static int ScrollSpeedMs = 1000; // mặc định: 2.5s
                                                // ⚙️ Tốc độ scroll feed Facebook (ms mỗi lần kéo chuột)
        // ⚙️ Thời gian chờ giữa các lần scroll (dừng để Facebook load thêm bài)
        public static int ScrollWaitMinMs = 800;
        public static int ScrollWaitMaxMs = 1200;
        // ⚙️ Số lần cuộn liên tiếp nếu không có bài mới thì dừng
        public static int ScrollMaxNoNewRounds = 5;
        // ⚙️ Thời gian chờ khi cuộn đến điểm cần click (đợi bài hiển thị)
        public static int WaitBeforeClickMs = 4000; // mặc định: 4s

        // ⚙️ Thời gian chờ sau khi click xong để load xong popup/link
        public static int WaitAfterClickMs = 4000; // mặc định: 4s
        //-------------
       // public const int MAX_POSTS_DEFAULT = 50;
        public static int AUTO_START_HOUR = 8;
        public static int AUTO_END_HOUR = 22;
        public static int AUTO_DEFAULT_MIN_INTERVAL = 120;
        public static int AUTO_DEFAULT_MAX_INTERVAL = 150;
        public static int MaxTab = 3;
    }
}
