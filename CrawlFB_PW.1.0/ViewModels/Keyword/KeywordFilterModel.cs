using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels.Keyword
{
    public class KeywordFilterModel
    {
        public string Group { get; set; }          // All, Attention, Negative, Exclude
        public int? Level { get; set; }            // level lọc
        public bool OnlyNoScore { get; set; }      // chưa chấm điểm
        public string SearchText { get; set; }
    }

}
