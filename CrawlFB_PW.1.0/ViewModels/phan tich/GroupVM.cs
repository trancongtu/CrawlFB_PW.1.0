using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.phan_tich
{
    class GroupVM
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }

        public int KeywordCount { get; set; }

        public bool IsEnabled { get; set; }
    }

}
