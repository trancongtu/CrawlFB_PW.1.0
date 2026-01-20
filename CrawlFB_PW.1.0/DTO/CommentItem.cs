using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class CommentItem
    {
        public string CommentId { get; set; }
        public string PosterName { get; set; }
        public string PosterLink { get; set; }
        public string Content { get; set; }
        public string TimeRaw { get; set; }
        public DateTime? RealCommentTime { get; set; }
        public string Status { get; set; } // "Gốc" | "Phản hồi"
        public string ParentCommentId { get; set; } // null = comment gốc
    }
}
