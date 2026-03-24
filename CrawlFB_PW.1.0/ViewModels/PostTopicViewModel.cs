using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class PostTopicViewModel
    {
        public bool Select { get; set; }
        public int STT { get; set; }

        public string PostId { get; set; }
        public string TopicName { get; set; }
        public string PostContent { get; set; }
        public string PostContentRaw { get; set; }   // 👈 thêm
        // 🔹 hiển thị thời gian bài viết
        public string TimeView =>
            RealPostTime.HasValue
                ? TimeHelper.NormalizeTime(RealPostTime.Value)
                : "N/A";

        // 🔹 thời gian bài viết thật (để so sánh, filter)
        public DateTime? RealPostTime { get; set; }

        // 🔹 thời gian convert (hệ thống sinh)
        public DateTime ConvertTime { get; set; }


        

        public string ViewDetail => "Xem";
    }


}
