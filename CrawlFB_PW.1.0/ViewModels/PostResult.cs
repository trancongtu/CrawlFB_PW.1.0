using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class PostResult
    {
        public List<PostPage> Posts { get; set; } = new List<PostPage>();
        public List<ShareItem> Shares { get; set; } = new List<ShareItem>();
    }
}
