using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class TopicViewModel
    {
        // UI
        public int STT { get; set; }
        public bool Select { get; set; }

        // Core
        public int TopicId { get; set; }
        public string TopicName { get; set; }

        public string NoteTopic { get; set; }

        // Thống kê
        public int CountKeyword { get; set; }
    }

}
