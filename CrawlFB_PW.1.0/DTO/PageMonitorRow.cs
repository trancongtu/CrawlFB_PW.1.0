using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class PageMonitorRow
    {
        public string PageID { get; set; }
        public string Status { get; set; }
        public string FirstScanTime { get; set; }
        public string LastScanTime { get; set; }
        public int TotalPostsScanned { get; set; }
    }
}
