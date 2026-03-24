using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class TopicKeywordTemplateVM
    {
        public int STT { get; set; }   // ✅ chỉ để hiển thị
        public string TopicName { get; set; }
        public string KeywordName { get; set; }

        public string Type { get; set; }      // Theo dõi / Tiêu cực / Loại trừ
        public int? Level { get; set; }       // 1–5
        public int Score { get; set; }

        public bool IsCritical { get; set; }
        public string Note { get; set; }
    }
}
