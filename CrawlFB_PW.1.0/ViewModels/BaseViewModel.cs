using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class BaseViewModel
    {
        public bool Select { get; set; }
        public UIStatus Status { get; set; } = UIStatus.Pending;
    }

}
