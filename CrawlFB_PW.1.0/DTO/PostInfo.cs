using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.DTO
{
    public class PostInfo
    {
        // ==============================
        // 1️⃣ Thông tin định danh
        // ==============================
        public string PostID { get; set; } = null;
        public string PostLink { get; set; }

        // ==============================
        // 2️⃣ Thời gian (CHỈ DÙNG DATETIME)
        // ==============================
        public DateTime? RealPostTime { get; set; }

        // ==============================
        // 3️⃣ Nội dung
        // ==============================
        public string Content { get; set; } 
        public string Attachment { get; set; } 

        // ==============================
        // 4️⃣ Người đăng
        // ==============================
        public string PosterName { get; set; } 
        public string PosterLink { get; set; } 
        public string PosterIdFB { get; set; } = null;
        public string PosterNote { get; set; } 

        // ==============================
        // 5️⃣ Trang / Group chứa bài
        // ==============================
        public string PageName { get; set; } 
        public string PageLink { get; set; } 
        public string ContainerIdFB { get; set; } = null;

        // ==============================
        // 6️⃣ Tương tác
        // ==============================
        public int? LikeCount { get; set; }
        public int? CommentCount { get; set; }
        public int? ShareCount { get; set; }

        // ==============================
        // 7️⃣ Phân loại bài
        // ==============================
        public PostType PostType { get; set; }

        // ==============================
        // 8️⃣ Share (nếu có)
        // ==============================
        public bool IsShare =>
            PostType.ToString().StartsWith("Share");

        public PostInfo SharedPost { get; set; }

        // ==============================
        // 9️⃣ Phân tích
        // ==============================
        public string Topic { get; set; } 
        public int AttentionScore { get; set; }
        public int NegativeScore { get; set; }
        public int ResultLevel { get; set; }

        // ==============================
        // 10️⃣ Dashboard highlight
        // ==============================
        public string HighlightedContent { get; set; }
    }
}
