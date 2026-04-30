using System;
using CrawlFB_PW._1._0.Enums;
public class PersonPostVM
{
    public bool Select { get; set; }

    public string PosterName { get; set; }
    public string PosterLink { get; set; }

    public string TimeView { get; set; }
    public DateTime? RealPostTime { get; set; }

    public string PosterIDFB { get; set; }
    public FBType PosterFBType { get; set; } = FBType.Unknown;
    public string PostLink { get; set; }
    public string PostStatus { get; set; }
    public string IDFBPost { get; set; }
    public string Content { get; set; }

    public int Like { get; set; }
    public int Comment { get; set; }
    public int Share { get; set; }

    // Phục vụ sort / filter
    public int InteractionTotal => Like + Comment + Share;
}
