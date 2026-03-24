using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class PostInfoViewModel : BaseViewModel
    {
        public string PostID { get; set; } = "N/A";
        public string PostLink { get; set; } = "N/A";
        public string TimeView =>
       RealPostTime.HasValue
           ? TimeHelper.NormalizeTime(RealPostTime.Value)
           : "N/A";
        public string Content { get; set; } = "N/A";
        //=====Người đăng
        public string PosterName { get; set; } = "N/A";
        public string PosterLink { get; set; } = "N/A";
        public string PosterNote { get; set; } = "N/A";
        public string PosterIdFB { get; set; }

        //=========
        // 🔹 Trang hoặc Group chứa bài đăng
        public string PageName { get; set; } = "N/A";
        public string PageLink { get; set; } = "N/A";
        public string ContainerIdFB { get; set; }
        public int Like { get; set; }
        public int Comment { get; set; }
        public int Share { get; set; }
        //============================
        public string Attachment { get; set; } = "N/A";  // 🆕 link hoặc thumbnail
        public string AttachmentView { get; set; } = "N/A"; // URL

        // 🔹 Trạng thái bài viết
        public PostType PostType { get; set; } // bài đăng gốc / share / reels / nền màu...

        // 🔹 Chủ đề phân tích (AI phân loại sau này)
        public string Topic { get; set; } = "N/A";
        public string PostTimeRaw { get; set; } = "N/A";

        public bool HasVideo { get; set; }
        public bool HasPhoto { get; set; }
        public bool HasReel { get; set; }
        public DateTime? RealPostTime { get; set; }
       
        public PageInfoViewModel ParentPage { get; set; }
    }
}
