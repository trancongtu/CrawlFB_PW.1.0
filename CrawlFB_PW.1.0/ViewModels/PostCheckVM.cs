using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class PostCheckVM
    {
        public int STT { get; set; }
        public string PostContent { get; set; }        // full bài viết
        public string Preview { get; set; }            // text ngắn cho grid
        public List<string> MatchedKeywords { get; set; }
        public string MatchedKeywordText { get; set; } // hiển thị cột grid
    }
}
