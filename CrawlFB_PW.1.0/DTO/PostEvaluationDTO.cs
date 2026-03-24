using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class PostEvaluationDTO
    {
        public string PostID { get; set; }

        public int AttentionScore { get; set; }
        public int NegativeScore { get; set; }
        public int InteractionScore { get; set; }

        public int TotalScore { get; set; }
        public string RiskLevel { get; set; }

        // CSV KeywordId
        public string AttentionKeywordIds { get; set; }
        public string NegativeKeywordIds { get; set; }

        public DateTime EvaluatedAt { get; set; }
    }

}
