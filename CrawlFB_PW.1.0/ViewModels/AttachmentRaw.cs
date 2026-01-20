using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class AttachmentRaw
    {
        public List<VideoAttachment> Videos { get; set; }
        public List<PhotoAttachment> Photos { get; set; }

        public AttachmentRaw()
        {
            Videos = new List<VideoAttachment>();
            Photos = new List<PhotoAttachment>();
        }
    }

    public class VideoAttachment
    {
        public string Url { get; set; }
        public string Time { get; set; }
    }

    public class PhotoAttachment
    {
        public string Src { get; set; }
        public string Alt { get; set; }
    }

}
