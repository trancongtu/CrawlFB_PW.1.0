using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class KeywordViewModel
    {
        public bool Select { get; set; }
        public int STT { get; set; }
        public int KeywordId { get; set; }
        public string KeywordName { get; set; }
        public int CountTopic { get; set; }

        // ===== ATTENTION =====
        public int AttentionScore { get; set; }
        public int? TrackingLevel { get; set; }

        // ===== NEGATIVE =====
        public int NegativeScore { get; set; }
        public int? NegativeLevel { get; set; }
        public bool IsCritical { get; set; }
        public bool IsExcluded { get; set; } 
        public int? ExcludeLevel { get; set; }
        // 🔥 LOẠI TRỪ
        public string Note { get; set; }
    }




}


