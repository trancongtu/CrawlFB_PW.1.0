using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.DTO
{
    public class PageInfo
    {
        // ⭐ PageID luôn là string, KHÔNG ĐỂ NULL.
        // Nếu chưa có, gán tạm "N/A", sau đó InsertOrIgnore sẽ tự sinh.
        public string PageID { get; set; }

        // ⭐ ID Facebook thực (không để null)
        public string IDFBPage { get; set; }

        // ⭐ Thông tin cơ bản
        public string PageLink { get; set; } 
        public string PageName { get; set; } 
        // ⭐ Loại Page: Fanpage | GroupOn | GroupOff | Person | PersonKOL | Unknown
        public FBType PageType { get; set; } 

        // ⭐ Thông tin thêm
        public string PageMembers { get; set; } 
        public string PageInteraction { get; set; } 
        public string PageEvaluation { get; set; } 
        // ⭐ Thời gian bài đăng gần nhất
        public DateTime? TimeLastPost { get; set; }
        // ⭐ Mô tả page
        public string PageInfoText { get; set; } 
        // ⭐ Page đã được crawl chưa?
        public bool IsScanned { get; set; } = false;
        // ⭐ Thời gian lưu page vào DB
        public string PageTimeSave { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public PageInfo() { }

        public PageInfo(string pageId, string pageLink, string pageName, FBType pageType)
        {
            PageID = string.IsNullOrWhiteSpace(pageId) ? null : pageId.Trim();
            PageLink = string.IsNullOrWhiteSpace(pageLink) ? null : pageLink.Trim();
            PageName = string.IsNullOrWhiteSpace(pageName) ? null : pageName.Trim();
            PageType = pageType;
        }

        public override string ToString()
        {
            return $"{PageName} ({PageType}) - {PageLink}";
        }
    }
}
