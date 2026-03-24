using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    public class NegativeMonitorViewModel
    {
        public bool Select { get; set; }

        public string PostID { get; set; }

        // 👇 Nội dung gốc
        public string PostContent { get; set; }
        public DateTime? RealPostTime { get; set; }

        public int AttentionScore { get; set; }
        public int AttentionLevel { get; set; }

        public int NegativeScore { get; set; }
        public int NegativeLevel { get; set; }

        public int ResultLevel { get; set; }

        // JSON lưu keyword
        public string AttentionKeywordIdsJson { get; set; }
        public string NegativeKeywordIdsJson { get; set; }

        public string ViewDetail => "Xem";
    }
}
