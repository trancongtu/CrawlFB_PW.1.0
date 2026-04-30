using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Enums
{
    public enum PageSourceType
    {
        PageInfo,// tổng page
        PageAdd,// page người dùng add
        PageCrawl,// page từ crawl lưu về
        PageNote,
        PageMonitor,
        PostInfo,
        PersonInfo
    }

    public enum FBType
    {
        GroupOn,
        GroupOff,
        Person,
        PersonKOL,
        PersonHidden,
        Fanpage,
        Unknown
    }

    public enum PageStatus
    {
        New,
        Scanned,
        Error
    }
    public enum GridDataType
    {
        Page,
        Post,
        Person
    }
    public enum UIStatus
    {
        Added,
        Pending,
        Running,
        Done,
        Skip,
        Error
    }
    public enum PostType
    {
        Page_Normal,
        Page_Reel_NoCap,
        page_Real_Cap,
        Page_Video_Nocap,
        Page_Video_Cap,
        Page_BackGround,
        Page_BackGround_Nocap,
        Page_Photo_NoCap,
        Page_Photo_Cap,
        Page_Unknow,
        Page_NoConent,
        Page_LinkWeb,
        Share_NoContent,
        Share_WithContent,
        Share_Reel_NoContent,
        Share_Reel_ConTent,
        Person_Normal, 
        Person_Reel_ConTent,
        Person_Unknow,
        Person_Reel_NoConent,
        Person_Photo_Nocap,
        Person_Photo_cap,
        Person_video_Nocap,
        Person_video_cap, 
        UnknowType
    }
    public enum PostKind
    {
        Normal,         // bài đăng thường
        ShareNormal,    // share bài thường
        Reel,
        ReelHasTime,
        ReelUnknow,// reel gốc
        ShareReel       // share reel
    }
    public enum ShareMode
    {
        Normal,    // share bài thường
        Reel       // share reel
    }
    public enum CrawlContext
    {
        Fanpage,
        Group
    }
    public enum MediaDetectType
    {
        None,
        Reel,
        Video,
        Photo
    }


}
