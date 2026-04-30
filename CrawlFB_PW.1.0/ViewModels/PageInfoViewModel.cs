using System;
using System.Collections.Generic;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.ViewModels
{
    /// <summary>
    /// ViewModel dùng cho toàn bộ các form liên quan Page
    /// </summary>
    public class PageInfoViewModel : BaseViewModel
    {
        public string PageID { get; set; }
        public string IDFBPage { get; set; }

        public string PageLink { get; set; }
        public string PageName { get; set; }
        public FBType PageType { get; set; } = FBType.Unknown;

        public string PageMembers { get; set; }
        public string PageInteraction { get; set; }
        public string PageEvaluation { get; set; }
        public string PageInfoText { get; set; }

        public DateTime? TimeLastPost { get; set; }

        // 🔥 chỉ hiển thị mới dùng "N/A"
        public string TimeLastPostView =>
            TimeLastPost.HasValue
                ? TimeHelper.NormalizeTime(TimeLastPost.Value)
                : "N/A";

        public bool IsScanned { get; set; } = false;

        public DateTime? PageTimeSave { get; set; }

        public string PageTimeSaveView =>
            PageTimeSave.HasValue
                ? PageTimeSave.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : "N/A";

        public List<PostInfoViewModel> Posts { get; }
            = new List<PostInfoViewModel>();
    }
}
