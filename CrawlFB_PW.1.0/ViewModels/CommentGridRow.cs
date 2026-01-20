using CrawlFB_PW._1._0.Enums;
using System;
namespace CrawlFB_PW._1._0.ViewModels
{
    public class CommentGridRow
    {
        public bool Select { get; set; }
        public string STT { get; set; }
        public string ActorName { get; set; }
        public FBType PosterFBType { get; set; } = FBType.Unknown;
        public DateTime? RealPostTime { get; set; }
        public string Time { get; set; }
        public string Link { get; set; }
        public string LinkView { get; set; }
        public string IDFBPerson { get; set; }
        public string Content { get; set; }
        public int Level { get; set; }
    }
}
