using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.Runtime
{
    public class PageRuntimeContext
    {
        public string PageID { get; set; }
        public string PageName { get; set; }

        // 🔥 dữ liệu runtime
        public List<PostInfoViewModel> Posts { get; set; } = new List<PostInfoViewModel>();

        // 🔥 share (nếu có)
        public List<ShareItem> Shares { get; set; } = new List<ShareItem>();

        // 🔥 lock riêng cho page
        public object LockObj { get; } = new object();

        // 🔥 state
        public int Countdown { get; set; }
        public int DelayExtra { get; set; }

        // 🔥 tracking
        public DateTime? LastScanTime { get; set; }
    }
}
