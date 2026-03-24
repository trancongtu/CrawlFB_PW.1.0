using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    class AttentionGroupVM
    {
        public int TrackingLevel { get; set; }   // 1,2,3
        public string GroupName { get; set; }     // "Theo dõi trọng điểm"
        public string Note { get; set; }          // mô tả nghiệp vụ
        public int KeywordCount { get; set; }     // bao nhiêu keyword
        public bool IsEnabled { get; set; }       // bật / tắt group
    }

}
