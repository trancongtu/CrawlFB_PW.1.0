using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class SharePostVM
    {
        // ===== UI chung =====
        public bool Select { get; set; }

        // ===== SHARE INFO =====
        public string SharerName { get; set; }          // Người Share
        public string SharerLink { get; set; }
        public string SharerLinkView => "👤 Người share";
        public string TargetName { get; set; }          // Nơi Share
        public string TargetLink { get; set; }
        public string TargetLinkView => "📍 Nơi share";
        public string TimeShare { get; set; }           // TimeShare (text đẹp)
        public DateTime? RealShareTime { get; set; }    // Time chuẩn      
        public string PostLinkShare { get; set; }
        public string PostLinkShareView => "🔗 Mở bài";
        // ===== COMMENT =====
        public int TotalComment => Comments?.Count ?? 0;

        // 👉 bấm “Xem bình luận” sẽ dùng
        public List<CommentGridRow> Comments { get; set; }
            = new List<CommentGridRow>();
        public string ViewComments => "💬 Xem";
    }
}
