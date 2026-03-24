using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.ViewModels.Keyword;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    public class PostHighlightDTO
    {
        public string PostId { get; set; }
        public string Content { get; set; }
        public List<KeywordMatchDTO> Attention { get; set; }
        public List<KeywordMatchDTO> Negative { get; set; }
    }
}
