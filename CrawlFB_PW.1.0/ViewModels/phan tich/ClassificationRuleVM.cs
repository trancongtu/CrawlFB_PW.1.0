using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    class ClassificationRuleVM
    {
        public int ResultLevel { get; set; }   // 1 = Xấu độc
        public string ResultName { get; set; } // "Xấu độc"

        public List<int> RequiredTrackingLevels { get; set; }
        public List<int> RequiredNegativeLevels { get; set; }

        public string Note { get; set; }
        public bool IsEnabled { get; set; }
    }

}
