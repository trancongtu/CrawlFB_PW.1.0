using System;
using System.Collections.Generic;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.ViewModels
{
    /// <summary>
    /// ViewModel dùng cho toàn bộ các form liên quan Page
    /// </summary>
    public class PageInfoViewModel : BaseViewModel
    {
        public string PageID { get; set; } = "N/A";
        public string IDFBPage { get; set; } = "N/A";

        public string PageLink { get; set; } = "N/A";
        public string PageName { get; set; } = "N/A";
        public string PageType { get; set; } = "Unknown";

        public string PageMembers { get; set; } = "N/A";
        public string PageInteraction { get; set; } = "N/A";
        public string PageEvaluation { get; set; } = "N/A";
        public string PageInfoText { get; set; } = "N/A";

        public DateTime? TimeLastPost { get; set; }

        public string TimeLastPostView =>
            TimeLastPost.HasValue
                ? TimeHelper.NormalizeTime(TimeLastPost.Value)
                : "N/A";

        public bool IsScanned { get; set; } = false;

        public string PageTimeSave { get; set; }
            = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public List<PostInfoViewModel> Posts { get; }
            = new List<PostInfoViewModel>();
    }
}
