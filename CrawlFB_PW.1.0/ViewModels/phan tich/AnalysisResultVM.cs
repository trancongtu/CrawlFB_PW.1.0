using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    class AnalysisResultVM
    {
        public long PostId { get; set; }

        public int Level { get; set; }
        public string LevelName { get; set; }   // Xấu độc / Tiêu cực

        public List<string> TrackingKeywords { get; set; }
        public List<string> NegativeKeywords { get; set; }

        public int TrackingScore { get; set; }
        public int NegativeScore { get; set; }
        public int InteractionScore { get; set; }

        public string SummaryReason { get; set; }
    }

}
