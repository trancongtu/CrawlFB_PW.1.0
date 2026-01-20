using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class PageInfo
    {
        // ⭐ PageID luôn là string, KHÔNG ĐỂ NULL.
        // Nếu chưa có, gán tạm "N/A", sau đó InsertOrIgnore sẽ tự sinh.
        public string PageID { get; set; } = "N/A";

        // ⭐ ID Facebook thực (không để null)
        public string IDFBPage { get; set; } = "N/A";

        // ⭐ Thông tin cơ bản
        public string PageLink { get; set; } = "N/A";
        public string PageName { get; set; } = "N/A";

        // ⭐ Loại Page: Fanpage | GroupOn | GroupOff | Person | PersonKOL | Unknown
        public string PageType { get; set; } = "Unknown";

        // ⭐ Thông tin thêm
        public string PageMembers { get; set; } = "N/A";
        public string PageInteraction { get; set; } = "N/A";
        public string PageEvaluation { get; set; } = "N/A";
        // ⭐ Thời gian bài đăng gần nhất
        public DateTime? TimeLastPost { get; set; }
        // ⭐ Mô tả page
        public string PageInfoText { get; set; } = "N/A";
        // ⭐ Page đã được crawl chưa?
        public bool IsScanned { get; set; } = false;
        // ⭐ Thời gian lưu page vào DB
        public string PageTimeSave { get; set; } =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public PageInfo() { }

        public PageInfo(string pageId, string pageLink, string pageName, string pageType)
        {
            PageID = string.IsNullOrEmpty(pageId) ? "N/A" : pageId;
            PageLink = pageLink;
            PageName = pageName;
            PageType = pageType;
        }

        public override string ToString()
        {
            return $"{PageName} ({PageType}) - {PageLink}";
        }
    }
}
