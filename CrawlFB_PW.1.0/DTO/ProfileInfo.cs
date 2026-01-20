using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class ProfileInfo
    {
        public int STT { get; set; }
        public string ProfileId { get; set; }
        public string Name { get; set; }            // tên gợi nhớ trong tool
        public int MaxTabs { get; set; } = 3;
        public int IsActive { get; set; }       // 1 = OK (login được), 0 = lỗi / không dùng
        public int CurrentTabs { get; set; }    // số tab hiện đang chạy

        public string ProfilePath { get; set; }     // đường dẫn profile (local)
        public string FacebookName { get; set; }    // tên FB thật
        public string Status { get; set; }          // ✅ / ❌ hoặc mô tả trạng thái
    }
}
