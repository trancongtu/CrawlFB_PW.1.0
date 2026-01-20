using System;

namespace CrawlFB_PW._1._0.DTO
{
    //CLASS HỖ TRWOJ LƯU TABLE SHARE
    public class ShareItem
    {
        public string PageID_A { get; set; }      // Page A ID (tốt nhất dùng PageID)
        public string PageLinkA { get; set; }     // Page A link

        public string PostID_B { get; set; }      // bài gốc ID (nếu có)
        public string PostLinkB { get; set; }     // bài gốc link

        public string ShareTimeRaw { get; set; }  // raw time
        public DateTime? ShareTimeReal { get; set; } // yyyy-MM-dd or yyyy-MM-dd HH:mm:ss

        public string PersonIDShare { get; set; }  // optional nếu người share là person
        public string PersonLinkShare { get; set; }

        public string Note { get; set; }           // comment tùy ý
    }
}
