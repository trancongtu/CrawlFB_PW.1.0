using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.ViewModels
{
    public class PostInfoRawVM
    {
        // ===== POST CORE =====
        public string PostLink { get; set; } = "N/A";
        public string PostTime { get; set; } = "N/A";
        public DateTime? RealPostTime { get; set; }

        // ===== POSTER =====
        public string PosterName { get; set; } = "N/A";
        public string PosterLink { get; set; } = "N/A";
        public FBType PosterNote { get; set; } = FBType.Unknown;

        // ===== CONTENT =====
        public string Content { get; set; } = "N/A";
        public PostType PostType { get; set; } = PostType.Page_Unknow;

        // ===== INTERACTION =====
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }

        // ===== CONTEXT =====
        public string PageName { get; set; }
        public string PageLink { get; set; }
        public string AttachmentJson { get; set; } = "N/A";
        public FBType ContainerType { get; set; } = FBType.Unknown;

        // 🔥 thêm ngay từ đầu
        public string ContainerIdFB { get; set; } = "";

        // sau này
        public string PosterIdFB { get; set; } = "";
    }

}
