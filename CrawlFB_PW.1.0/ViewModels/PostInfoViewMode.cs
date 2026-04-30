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
        public string PostID { get; set; }
        public string PostLink { get; set; }
        public string TimeView =>
       RealPostTime.HasValue
           ? TimeHelper.NormalizeTime(RealPostTime.Value)
           : "N/A";
        public string Content { get; set; }
        //=====Người đăng
        public string PosterName { get; set; } 
        public string PosterLink { get; set; }
        public FBType PosterNote { get; set; }
        public string PosterIdFB { get; set; }

        //=========
        // 🔹 Trang hoặc Group chứa bài đăng       
        public string PageID { get; set; }
        public string PageName { get; set; } 
        public string PageLink { get; set; } 
        public string ContainerIdFB { get; set; }
        public FBType ContainerType { get; set; } = FBType.Unknown;
        public int Like { get; set; }
        public int Comment { get; set; }
        public int Share { get; set; }
        //============================
        public string Attachment { get; set; }  // 🆕 link hoặc thumbnail
       

        // 🔹 Trạng thái bài viết
        public PostType PostType { get; set; } // bài đăng gốc / share / reels / nền màu...

        // 🔹 Chủ đề phân tích (AI phân loại sau này)
        public string Topic { get; set; }  
        public string PostTimeRaw { get; set; } 

        public bool HasVideo { get; set; }
        public bool HasPhoto { get; set; }
        public bool HasReel { get; set; }
        public DateTime? RealPostTime { get; set; }
        public string PostTypeView =>ProcessingHelper.GetPostTypeView(PostType);
        public string AttachmentView => AttachmentHelper.GetAttachmentForView(Attachment);
        public PageInfoViewModel ParentPage { get; set; }
    }
}
