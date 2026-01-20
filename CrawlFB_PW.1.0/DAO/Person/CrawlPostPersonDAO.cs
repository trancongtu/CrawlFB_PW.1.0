using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using DevExpress.DocumentView;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DocumentFormat.OpenXml.Office2019.Drawing.Animation.Model3D;
using Microsoft.Playwright;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using IPage = Microsoft.Playwright.IPage;
namespace CrawlFB_PW._1._0.DAO
{
    public class CrawlPostPersonDAO
    {
        // ===============================
        // SINGLETON DAO
        // ===============================
        private static readonly Lazy<CrawlPostPersonDAO> _instance =
            new Lazy<CrawlPostPersonDAO>(() => new CrawlPostPersonDAO());
        public class PostResult
        {
            public List<PostPage> Posts { get; set; } = new List<PostPage>();
            public List<ShareItem> Shares { get; set; } = new List<ShareItem>();
        }


        public static CrawlPostPersonDAO Instance => _instance.Value;
     
        // Private constructor (DAO)
        private CrawlPostPersonDAO() { }

        // =========================================================
        // GET FEED CONTAINER – PROFILE TIMELINE
        // =========================================================
        public async Task<IElementHandle> GetFeedContainerAsync(IPage page)
        {
            try
            {
                Libary.Instance.LogTech("Lấy Feed Tường",AppConfig.ENABLE_TECH_LOG);
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // 1️⃣ Ưu tiên ProfileTimeline
                var feed = await page.QuerySelectorAsync( "div[data-pagelet='ProfileTimeline']");

                if (feed != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconOK} Feed = data-pagelet=ProfileTimeline");
                    return feed;
                }
                else
                {
                    feed = await page.QuerySelectorAsync("div[role='feed']");
                    if (feed != null)
                    {
                        Libary.Instance.LogDebug($"{Libary.IconWarn} Fallback → div[role='feed']");
                        return feed;
                    }
                    else
                    {
                        // ❌ Không tìm thấy
                        Libary.Instance.LogDebug($"{Libary.IconFail} Không tìm thấy Feed profile");
                        return null;
                    }
                }    
                // 2️⃣ Fallback role=feed         
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug( $"{Libary.IconFail} Exception GetFeedContainerAsync");
                Libary.Instance.LogError("GetFeedContainerAsync error", ex);
                return null;
            }
        }
        public async Task<PostResult> GetPostPerson(IPage page,IElementHandle postNode, string personUrl,string personName)
        {
            string PostTime = "N/A", PostLink = "N/A", OriginalPostTime = "N/A", OriginalPostLink = "N/A", PostStatus = "N/A";
            string OriginalPosterLink = "N/A", OriginalPosterName = "N/A", OriginalPosterNote = "N/A";
             string Content = "N/A", OriginalContent = "N/A", OriginalPostStatus = "N/A"; int? LikeCount = null, ShareCount = null, CommentCount = null;
            int? OriginalLikeCount = null, OriginalShareCount = null, OriginalCommentCount = null;
            DateTime? RealPostTime = null;
            DateTime? RealOriginalPostTime = null;
            PostPage postB = new PostPage();
            var results = new PostResult();
            try
            {
                Libary.Instance.LogTech("==========BẮT ĐẦU PHẦN TÍCH POST============" + "\n");
                // =================================================
                // 1️⃣ LẤY TIME + LINK
                // =================================================
                var postInfor = await postNode.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");

                if (postInfor == null || postInfor.Count == 0)
                {
                    Libary.Instance.LogTech($"{Libary.IconFail} Lỗi lấy POSTINFO ");
                    return results;               
                }                   
                var (timeList, linkList) = await PageDAO.Instance.ExtractTimeAndLinksAsync(postInfor);

                Libary.Instance.LogTech($"{Libary.IconOK}[PERSON][RAW] time={timeList.Count}, link={linkList.Count}");
                foreach ( var link in linkList ) { Libary.Instance.LogDebug($"Link tìm được: {link}"); }
                foreach (var time in timeList) { Libary.Instance.LogDebug($"Time tìm được: {time}"); }
                if (timeList.Count == 0 || linkList.Count == 0)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} Lỗi lấy Time&Link ");
                    return results;
                }
                var (hasReel, rawReelLink) = await DetectReelFromPostAsync(postNode);
                if (hasReel) { Libary.Instance.LogTech($"Có Reel: {rawReelLink}"); }
                string reelLink = NormalizeReelLink(rawReelLink);
                Libary.Instance.LogTech($"Reel sau xử lý: {reelLink}");
                // =================================================
                // 3️⃣ CASE: SHARE POST THƯỜNG (time = 2)
                // =================================================
                if (timeList.Count >= 2 && linkList.Count >= 2)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} TimeLink = 2");
                    // 🔹 POST A: bài SHARE (poster = person crawl)                  
                    PostLink = linkList[0];
                    PostTime = timeList[0];
                    OriginalPostLink = linkList[1];
                    OriginalPostTime = timeList[1];                   
                    Libary.Instance.LogTech(
                        $"[SHARE-DEBUG]\n" +
                        $" Time 1: {timeList[0]}\n" +                       
                        $" Time 2: {timeList[1]}\n" +
                        $"Link 1: {ProcessingHelper.ShortenFacebookPostLink(PostLink)} \n"+
                        $"Link 2: {ProcessingHelper.ShortenFacebookPostLink(OriginalPostLink)} \n"
                    );
                    // LẤY NỘI DUNG
                    Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                    string noidung = null, noidunggoc = null, noidunganh = null;
                    if (postInfor.Count >= 5)
                    {
                        Libary.Instance.LogDebug("Vào đến CASE Postinfo > 5");
                        var contentContainer2 = PageDAO.Instance.GetSafe(postInfor, 2);
                        var contentContainer4 = PageDAO.Instance.GetSafe(postInfor, 4);
                        Libary.Instance.LogDebug("Lấy Thử content vị trí postinfo 3");
                        string content2 = await PageDAO.Instance.GetContentTextAsync(page, contentContainer2);
                        Libary.Instance.LogDebug("Lấy Thử content vị trí postinfo 5");
                        string content4 = await PageDAO.Instance.GetContentTextAsync(page, contentContainer4);

                        bool hasContent2 = !string.IsNullOrWhiteSpace(content2) && content2 != "N/A";
                        bool hasContent4 = !string.IsNullOrWhiteSpace(content4) && content4 != "N/A";

                        if (postInfor.Count == 6)
                        {
                            Libary.Instance.LogDebug("Vào đến CASE Postinfo = 6");
                            var contentContainer5 = PageDAO.Instance.GetSafe(postInfor, 5);
                            Libary.Instance.LogDebug("Lấy Thử content vị trí postinfo 6");
                            string content5 = await PageDAO.Instance.GetContentTextAsync(page, contentContainer5);
                            bool hasContent5 = !string.IsNullOrWhiteSpace(content5) && content5 != "N/A";
                            if (hasContent2 && hasContent5)
                            {
                                noidung = content2;
                                noidunggoc = content5;
                                PostStatus = "Đủ nội dung";
                                Libary.Instance.LogTech("🟩 Lấy content share + content gốc");
                            }
                            else if (!hasContent2 && !hasContent5 && hasContent4)
                            {
                                noidunggoc = content4;
                                PostStatus = "có nội dung share";
                                Libary.Instance.LogTech("🟨 Lấy content gốc từ container 4");
                            }
                            else if (hasContent2)
                            {
                                noidung = content2;
                                PostStatus = "không kèm nội dung";
                                Libary.Instance.LogTech("🟦 Chỉ có content share");
                            }
                            else
                            {
                                Libary.Instance.LogTech("🟥 Không lấy được content text");
                            }
                        }
                    }
                    else if (postInfor.Count == 4)
                    {
                        Libary.Instance.LogTech(
                           "   🔹 Content normal rỗng, thử Background",
                           AppConfig.ENABLE_TECH_LOG
                       );
                        noidunganh = await PageDAO.Instance.BackgroundTextAllAsync(page, postNode);
                    }
                    if (!string.IsNullOrWhiteSpace(noidunggoc)) OriginalContent = noidunggoc;
                    if (!string.IsNullOrEmpty(noidung)) Content = noidung;
                    if (!string.IsNullOrEmpty(noidunganh)) Content = noidunganh;
                    // ===== 4️⃣ LOG TỔNG KẾT =====
                    if (Content != "N/A" || OriginalContent != "N/A")
                    {
                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy nội dung bài viết thành công ");
                        Libary.Instance.ClearLog($"Nội dung Share/đăng: {ProcessingDAO.Instance.PreviewText(Content)}");
                        Libary.Instance.ClearLog($"Nội dung gốc: {ProcessingDAO.Instance.PreviewText(OriginalContent)}");
                    }
                    else
                    {
                        Libary.Instance.LogTech($"{Libary.IconFail} Không có nội dung hoặc k lấy được nội dung bài viết");
                    }
                    //==LẤY TƯƠNG TÁC
                    (LikeCount, CommentCount, ShareCount) = await PageDAO.Instance.ExtractPostInteractionsAsync(postNode);                  
                    Libary.Instance.LogTech( $"Tương tác 👍={LikeCount} 💬={CommentCount} 🔁={ShareCount}");
                    if (OriginalPostLink != "N/A" && OriginalPosterLink != null)
                    {
                        Libary.Instance.LogTech(Environment.NewLine + "-----------------------LẤY CHI TIẾT BÀI GỐC -----------------------------" + Environment.NewLine);
                    postB = await PageDAO.Instance.GetPostOriginal(page, OriginalPostLink);
                    Libary.Instance.LogTech(
                       postB == null
                           ? "Không lấy được bài gốc"
                           : PageDAO.Instance.SummarizeOriginalPost(postB)
                   );
                }
                if (postB != null && postB.PostLink != "N/A")
                {                      
                        OriginalContent = postB.Content;
                        OriginalPosterName = postB.PosterName;
                        OriginalPosterLink = postB.PosterLink;
                        OriginalLikeCount = postB.LikeCount;
                        OriginalCommentCount = postB.CommentCount;
                        OriginalShareCount = postB.ShareCount;
                        OriginalPostStatus = "Bài gốc";
                        Libary.Instance.LogTech($"{Libary.IconOK}🟨 Share normal OK");
                    }
                else
                {
                    Libary.Instance.LogTech($"{Libary.IconFail}[Caller] ❌ LẤY BÀI GỐC BẰNG GetPostOriginal lỗi");
                }

            }
                else
                {
                    if(timeList.Count == 1 && linkList.Count == 1)
                    {
                        PostTime = timeList[0];
                        PostLink = linkList[0];                    
                        PostLink = ProcessingHelper.ShortenFacebookPostLink(PostLink);
                        Libary.Instance.LogTech($"Link sau xử lý: {PostLink}");
                        Libary.Instance.LogTech($"time sau xử lý: {TimeHelper.ParseFacebookTime(PostTime)}");
                        if (!hasReel || (hasReel && PostLink == reelLink))// time = 1 k phải reel
                        {                                                    
                            // LẤY NỘI DUNG
                            if (postInfor.Count == 3 || postInfor.Count == 2)
                            {
                                
                                if (postInfor.Count == 3)
                                {
                                    Libary.Instance.LogTech("Vào Postinfor3", AppConfig.ENABLE_TECH_LOG);
                                    var contentContainer = PageDAO.Instance.GetSafe(postInfor, 2);
                                    Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                    string ContentNomal = await PageDAO.Instance.GetContentTextAsync(page, contentContainer);
                                    //===log
                                    if (!string.IsNullOrWhiteSpace(ContentNomal) && ContentNomal != "N/A")
                                    {
                                        Content = ContentNomal;
                                        Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content Nomal", AppConfig.ENABLE_TECH_LOG);
                                        PostStatus = "bài đăng bình thường";
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech("Content trống, thử lấy bằng backgroud:", AppConfig.ENABLE_TECH_LOG);
                                        string ContentBackGround = await PageDAO.Instance.BackgroundTextAllAsync(page, postNode);
                                        if (!string.IsNullOrWhiteSpace(ContentBackGround) && ContentBackGround != "N/A")
                                        {
                                            Content = ContentBackGround;
                                            Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content BackGround", AppConfig.ENABLE_TECH_LOG);
                                            PostStatus = "bài đăng kèm ảnh/video";
                                        }
                                        else
                                        {
                                            PostStatus = "bài đăng không có nội dung";
                                        }
                                    }
                                    if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công, số ký tự: " + Content.Length);
                                        if (!string.IsNullOrWhiteSpace(Content))
                                        {
                                            Libary.Instance.LogTech(
                                                "preview: " +
                                                (Content.Length > 200
                                                    ? Content.Substring(0, 200) + "..."
                                                    : Content),
                                                AppConfig.ENABLE_TECH_LOG
                                            );
                                        }
                                    }
                                    else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được nội dung bài viết");
                                    Libary.Instance.LogTech("chốt kiểu bài viết: " + PostStatus);
                                }//else là 2 nền màu, ảnh
                                else
                                {
                                    Libary.Instance.LogTech("Vào Postinfor2", AppConfig.ENABLE_TECH_LOG);
                                    Content = await PageDAO.Instance.BackgroundTextAllAsync(page, postNode);
                                    if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                    {
                                        PostStatus = "bài đăng nền màu/ảnh";
                                        Libary.Instance.LogTech("chốt kiểu bài viết: " + PostStatus);
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                    }
                                    else
                                    {
                                        PostStatus = "không lấy được nội dung";
                                        Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được nội dung bài viết background");
                                    }
                                }
                            }
                            else if (postInfor.Count == 4)
                            {
                                Libary.Instance.LogTech("Vào Postinfor 4", AppConfig.ENABLE_TECH_LOG);                        
                                var contentContainer = PageDAO.Instance.GetSafe(postInfor, 2);
                                Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                Content = await PageDAO.Instance.GetContentTextAsync(page, contentContainer);
                                // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                if (string.IsNullOrWhiteSpace(Content) || Content == "N/A")
                                {
                                    Content = await PageDAO.Instance.BackgroundTextAllAsync(page, postNode);
                                    if (!string.IsNullOrWhiteSpace(Content))
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                        PostStatus = "bài đăng kèm ảnh/video";
                                        Libary.Instance.LogTech(
                                               "preview: " +
                                               (Content.Length > 200
                                                   ? Content.Substring(0, 200) + "..."
                                                   : Content),
                                               AppConfig.ENABLE_TECH_LOG
                                           );
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail} Background k thấy nội dung");
                                        PostStatus = "bài đăng không có nội dung";
                                    }
                                }
                                else
                                {
                                    Libary.Instance.LogTech($"{Libary.IconFail} Bài viết gắn link ngoài k có content");
                                    PostStatus = "bài đăng dẫn link ngoài";
                                }
                                Libary.Instance.LogTech("chốt kiểu bài viết: " + PostStatus);
                            }
                            Libary.Instance.LogTech("----  CASE 1 🔹 Đang lấy tương tác...---");
                            (LikeCount, CommentCount, ShareCount) = await PageDAO.Instance.ExtractPostInteractionsAsync(postNode);
                            if (LikeCount != 0 || CommentCount != 0 || ShareCount != 0) Libary.Instance.LogTech($"{Libary.IconOK}Lấy tương tác thành công: Like  {LikeCount}  Share: {ShareCount} Comment: {CommentCount} ");
                            else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được tương tác bài viết");
                        }
                        else
                        {
                            // BÀI SHARE
                            if (postInfor.Count == 3)
                            {
                                Libary.Instance.LogTech("Vào Postinfor3", AppConfig.ENABLE_TECH_LOG);
                                var contentContainer = PageDAO.Instance.GetSafe(postInfor, 2);
                                Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                string ContentNomal = await PageDAO.Instance.GetContentTextAsync(page, contentContainer);
                                //===log
                                if (!string.IsNullOrWhiteSpace(ContentNomal) && ContentNomal != "N/A")
                                {
                                    Content = ContentNomal;
                                    Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content Nomal", AppConfig.ENABLE_TECH_LOG);
                                    PostStatus = "bài đăng bình thường";
                                }
                                else
                                {
                                    Libary.Instance.LogTech("Content trống, thử lấy bằng backgroud:", AppConfig.ENABLE_TECH_LOG);
                                    string ContentBackGround = await PageDAO.Instance.BackgroundTextAllAsync(page, postNode);
                                    if (!string.IsNullOrWhiteSpace(ContentBackGround) && ContentBackGround != "N/A")
                                    {
                                        Content = ContentBackGround;
                                        Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content BackGround", AppConfig.ENABLE_TECH_LOG);                                      
                                    }                                  
                                }
                                if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                {
                                    Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công, số ký tự: " + Content.Length);
                                    if (!string.IsNullOrWhiteSpace(Content))
                                    {
                                        Libary.Instance.LogTech(
                                            "preview: " +
                                            (Content.Length > 200
                                                ? Content.Substring(0, 200) + "..."
                                                : Content),
                                            AppConfig.ENABLE_TECH_LOG
                                        );
                                    }
                                }
                                else Libary.Instance.LogTech($" LỖI HOẶC SHARE K NỘI DUNG");
                                
                            }
                            (LikeCount, CommentCount, ShareCount) = await PageDAO.Instance.ExtractPostInteractionsAsync(postNode);
                            if (LikeCount != 0 || CommentCount != 0 || ShareCount != 0)
                            {
                                Libary.Instance.LogTech("Tương tác bài share lại"+
                                    $"{Libary.IconOK} [Share]\n" +
                                    $"👍 Like = {LikeCount}\n" +
                                    $"💬 Comment = {CommentCount}\n" +
                                    $"🔁 Share = {ShareCount}"
                                );
                            }

                            else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được tương tác bài viết");
                            // ===== SHARE REEL =====
                            Libary.Instance.LogTech($"{Libary.IconInfo} Detect: Chia sẻ Reel");
                                // postLink = link share
                                // reelLink = link reel gốc
                                Libary.Instance.LogTech(
                                    $"🔁 Share Reel\n" +
                                    $" LinkShare: {PostLink}\n" +
                                    $" TimeShare: {PostTime}\n" +
                                    $" LinkGoc  : {reelLink}"
                                );

                                // 👉 CHỈ extract bằng link gốc
                                var reelPost = await ExtractPostReelAll(page, postNode);

                                if (reelPost != null && reelPost.PostLink != "N/A")
                                {
                                    OriginalPostLink = reelLink; // link reel gốc
                                    OriginalPostTime = reelPost.PostTime;
                                    OriginalPosterName = reelPost.PosterName;
                                    OriginalPosterLink = reelPost.PosterLink;
                                    OriginalContent = reelPost.Content;
                                    OriginalLikeCount = reelPost.LikeCount;
                                    OriginalCommentCount = reelPost.CommentCount;
                                    OriginalShareCount = reelPost.ShareCount;
                                    OriginalPostStatus = "Bài gốc";
                                    OriginalPosterNote = reelPost.PosterNote;
                            
                                    Libary.Instance.LogTech($"{Libary.IconOK}🟨 Share Reel OK");
                                }
                            

                        }
                    }    
                }
                // thêm kết quả vào post
                if (PostTime != "N/A")
                {
                    PostTime = TimeHelper.CleanTimeString(PostTime);
                    RealPostTime = TimeHelper.ParseFacebookTime(PostTime);

                }

                if (OriginalPostTime != "N/A")
                {
                    OriginalPostTime = TimeHelper.CleanTimeString(OriginalPostTime);
                    RealOriginalPostTime = TimeHelper.ParseFacebookTime(OriginalPostTime);
                    
                }
                if (PostLink != "N/A" && OriginalPostLink != "N/A")
                {
                    var postShare = new PostPage
                    {
                        PostLink = ProcessingHelper.ShortenFacebookPostLink(PostLink),
                        PostTime = PostTime,
                        RealPostTime = RealPostTime,
                        PostType = PostType.Share_WithContent.ToString(), // "Chia sẻ bài thường" | "Chia sẻ bài reel"
                        Content = Content,
                        PosterName = personName,
                        PosterLink = personUrl,
                        LikeCount = LikeCount,
                        ShareCount = ShareCount,
                        CommentCount = CommentCount,
                        PosterNote = "N/A",
                    };

                    results.Posts.Add(postShare);
                    var postOriginal = new PostPage
                    {
                        PostLink = OriginalPostLink,
                        PostTime = OriginalPostTime,
                        PosterName = OriginalPosterName,
                        PosterLink = OriginalPosterLink,
                        RealPostTime = RealOriginalPostTime,
                        PostType = OriginalPostStatus,
                        Content = OriginalContent,
                        LikeCount = OriginalLikeCount,
                        ShareCount = OriginalShareCount,
                        CommentCount = OriginalCommentCount
                    };

                    results.Posts.Add(postOriginal);
                    //==
                    var share = new ShareItem
                    {                            
                        PostLinkB = OriginalPostLink,   // bài gốc
                        ShareTimeRaw = PostTime,
                        ShareTimeReal = RealPostTime,
                        PersonLinkShare = personUrl,
                        Note = PostStatus
                    };
                    results.Shares.Add(share);
                }
                else
                {
                    // ❌ KHÔNG ADD nếu PostLink invalid
                    if (string.IsNullOrWhiteSpace(PostLink) || PostLink == "N/A")
                    {
                        Libary.Instance.LogTech("❌ SKIP ADD POST: PostLink = N/A");
                    }
                    else
                    {
                        var postnormal = new PostPage
                        {
                            PostLink = ProcessingHelper.ShortenFacebookPostLink(PostLink),
                            PostTime = PostTime,
                            RealPostTime = RealPostTime,
                            PostType = PostStatus,
                            Content = Content,
                            PosterName = personName,
                            PosterLink = personUrl,
                            LikeCount = LikeCount,
                            ShareCount = ShareCount,
                            CommentCount = CommentCount,
                            PosterNote = "PERSON"
                        };
                        results.Posts.Add(postnormal);
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech( "[PERSON][GetPostPerson] ❌ " + ex.Message
                );
            }
            return results;
        }

        // lấy link share bài reel
        public async Task<(bool hasReel, string reelLink)> DetectReelFromPostAsync( IElementHandle post)
        {
            try
            {
                var links = await post.QuerySelectorAllAsync("a[href]");
                Libary.Instance.LogDebug($"-------KIỂM TRA BÀI REEL-----------");
                Libary.Instance.LogDebug($"[ReelDetect] Found {links.Count} <a> tags");
                foreach (var a in links)
                {
                    string href = await a.GetAttributeAsync("href");
                    if (string.IsNullOrWhiteSpace(href))
                        continue;

                  

                    // normalize link
                    href = ProcessingHelper.ShortenFacebookPostLink(href);

                    // ✅ CHECK REEL
                    if (href.Contains("/reel/") ||
                        href.Contains("/videos/") ||
                        href.Contains("/watch/"))
                    {
                        Libary.Instance.LogDebug( $"[REEL] ✅ Phát hiện Reel link: {href}");
                        return (true, href);
                    }
                }
                // ❌ không có reel
                Libary.Instance.LogDebug("[REEL] ❌ Post không chứa Reel");
                return (false, null);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("[REEL] ❌ DetectReelFromPostAsync error: " + ex.Message);
                return (false, null);
            }
        }

        // Lấy tên FB
        // CrawlPostPersonDAO.cs
        public async Task<string> GetFacebookNamePersonAsync(IPage page)
        {
            try
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} BẮT ĐẦU LẤY NAME FB");
                // ===============================
                // 1️⃣ ƯU TIÊN H1 (html-h1)
                // ===============================
                var h1s = await page.QuerySelectorAllAsync( "h1.html-h1");
                foreach (var h1 in h1s)
                {
                    if (!await h1.IsVisibleAsync())
                        continue;

                    string name = (await h1.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        Libary.Instance.LogDebug(
                            $"{Libary.IconOK} [PERSON-NAME][H1-VISIBLE] {name}"
                        );
                        return name;
                    }
                }
                // ===============================
                // 2️⃣ FALLBACK DIV
                // ===============================
                var div = await page.QuerySelectorAsync("div.x1e56ztr.x1xmf6yo" );

                if (div != null)
                {
                    string name = (await div.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        Libary.Instance.LogDebug($"{Libary.IconOK}[PERSON-NAME][FALLBACK DIV] {name}");
                        return name;
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug( $"{Libary.IconFail}[PERSON-NAME] ❌ " + ex.Message);
            }
            return "N/A";
        }
        private async Task<PersonInfo> BuildPersonInfoAsync(IPage page, string personUrl, ProfileDB profile)
        {
            try
            {
                // ===============================
                // 1️⃣ LẤY TÊN PERSON
                // ===============================
                string personName =
                    await CrawlPostPersonDAO.Instance.GetFacebookNamePersonAsync(page);

                if (string.IsNullOrWhiteSpace(personName))
                    personName = "N/A";

                // ===============================
                // 2️⃣ CHECK TYPE (PERSON / PAGE / GROUP / UNKNOWN)
                // ===============================
                FBType personType = await PageDAO.Instance.CheckFBTypeAsync(page);



                // ===============================
                // 3️⃣ BUILD DTO PersonInfo (ĐÚNG FIELD)
                // ===============================
                var person = new PersonInfo
                {
                    PersonID = "",
                    IDFBPerson = "N/A",                 // chưa extract
                    PersonLink = personUrl,
                    PersonName = personName,
                    PersonInfoText = "N/A",             // bio chưa crawl
                    PersonNote = personType.ToString(),            // type từ PageDAO
                    PersonTimeSave = DateTime.Now
                        .ToString("yyyy-MM-dd HH:mm:ss")
                };

                // ===============================
                // 4️⃣ LOG FORM
                // ===============================
               
                return person;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech( "❌ BuildPersonInfoAsync error: " + ex.Message );
                return null;
            }
        }
        public string NormalizeReelLink(string reelLink)
        {
            if (string.IsNullOrWhiteSpace(reelLink))
                return "N/A";

            reelLink = reelLink.Trim();

            // Đã là link đầy đủ
            if (reelLink.StartsWith("http://") || reelLink.StartsWith("https://"))
                return reelLink;

            // Dạng /reel/xxxx
            if (reelLink.StartsWith("/reel/"))
                return "https://fb.com" + reelLink;

            // Dạng reel/xxxx
            if (reelLink.StartsWith("reel/"))
                return "https://fb.com/" + reelLink;

            // fallback
            return reelLink;
        }
        public async Task<PostPage> ExtractPostReelAll(IPage mainPage, IElementHandle post)
        {
            var reel = new PostPage();
            Libary.Instance.LogDebug($"{Libary.IconInfo}----PHÂN TÍCH POST REEL -----------");
        /*    try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo}----PHÂN TÍCH POST REEL -----------");
                var posterReel = await post.QuerySelectorAsync("a[class*='x1i10hfl xjbqb8w x1ejq31n x18oe1m7 x1sy0etr xstzfhl x972fbf x10w94by x1qhh985 x14e42zd x9f619 x1ypdohk xt0psk2 x3ct3a4 xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x16tdsg8 x1hl2dhg xggy1nq x1a2a7pz x1heor9g xkrqix3 x1sur9pj x1s688f']");
                if (posterReel != null)
                {
                    // Lấy link người đăng
                    string rawHref = await posterReel.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(rawHref))
                    {
                        reel.PosterLink = ProcessingDAO.Instance.ShortenPosterLinkReel(rawHref);
                    }
                }
                else
                {
                    var a = await post.QuerySelectorAsync("a[role='link'][href*='/user/']");
                    if (a != null)
                    {
                        string posterlink = await a.GetAttributeAsync("href");
                        if (!string.IsNullOrWhiteSpace(posterlink))
                        {
                            reel.PosterLink = ProcessingDAO.Instance.ShortenPosterLinkReel(posterlink);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} PosterRell k lấy được link: {ex.Message}");
            }
        */
            try
            {
                var aTags = await post.QuerySelectorAllAsync("a[href]");
                string reelLink = "N/A";
                string posterLink = "N/A";
                string postTime = "N/A";
                string posterName = "N/A";
                foreach (var a in aTags)
                {
                    string href = await a.GetAttributeAsync("href") ?? "";
                    string text = (await a.InnerTextAsync() ?? "").Trim();

                    if (string.IsNullOrEmpty(href))
                        continue;
                    // 2) Tìm reel
                    if (href.Contains("/reel/") && reelLink == "N/A")
                    {
                        reelLink = ProcessingHelper.ShortenFacebookPostLink(href);
                        Libary.Instance.LogDebug($"{Libary.IconOK} 🎞️ Found ReelLink = {reelLink}");
                    }
                    reel.PostLink = ProcessingHelper.ShortenFacebookPostLink(reelLink);
                }
                IElementHandle TimeTag = await post.QuerySelectorAsync("span[class= 'html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x4k7w5x x1h91t0o x1h9r5lt x1jfb8zj xv2umb2 x1beo9mf xaigb6o x12ejxvf x3igimt xarpa2k xedcshv x1lytzrv x1t2pt76 x7ja8zs x1qrby5j']");
                if (TimeTag != null)
                {
                    string time = (await TimeTag.InnerTextAsync() ?? "").Trim();
                    if (TimeHelper.IsTime(time))
                    {
                        postTime = TimeHelper.CleanTimeString(time);
                        Libary.Instance.LogDebug($"{Libary.IconOK} 🕒 Found PostTime reel: {postTime}");
                        reel.PostTime = postTime;
                    }
                }
                if (reelLink == "N/A")
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail}❌ [ReelExtract] Không có /reel/ để mở tab → return basic info");
                    return reel;
                }
                (posterName, posterLink) = await ExtractPosterInfoAsync(post);

                if (posterName != "N/A")
                    reel.PosterName = posterName;

                if (posterLink != "N/A") 
                {
                    string normalized = ProcessingDAO.Instance.NormalizePersonProfileLink(posterLink);
                    reel.PosterLink = normalized ?? posterLink;

                    Libary.Instance.LogTech($"{Libary.IconOK} PosterLink OK: {reel.PosterLink}");

                }                       
                // ============================================
                // 4️⃣ MỞ TAB BÀI REEL → LẤY CONTENT + TƯƠNG TÁC
                // ============================================
                Libary.Instance.LogDebug($"{Libary.IconInfo} 🌐 [ReelExtract] Mở tab mới để lấy nội dung reel…");
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync($"window.open('{reelLink}', '_blank');");
                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));
                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} ❌ [ReelExtract] Timeout mở tab reel");
                    return reel;
                }
                var newPage = await popupTask;
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(500);

                // ========== LẤY CONTENT REEL ==========
                Libary.Instance.LogDebug($"{Libary.IconInfo} ✍️ [ReelExtract] Lấy caption reel…");
                reel.Content = await GetReelTextAsync(newPage, await newPage.QuerySelectorAsync("body"));
                /*var Divpostername = await newPage.QuerySelectorAsync("span[class='xjp7ctv']>a");
                if (Divpostername != null)
                {
                    posterName = (await Divpostername.InnerTextAsync()).Trim();
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} Không lấy được PosterName");
                }*/
                // ========== LẤY TƯƠNG TÁC ==========
                Libary.Instance.LogDebug($"{Libary.IconInfo} 📊 [ReelExtract] Lấy tương tác reel…");
                var (likes, comments, shares) = await ExtractReelInteractionsAsync(newPage);
                reel.LikeCount = likes;
                reel.CommentCount = comments;
                reel.ShareCount = shares;
                reel.PosterName = posterName;
                await newPage.CloseAsync();
                return reel;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ [ReelExtract] Lỗi: {ex.Message}");
                return reel;
            }
        }
        public async Task<(int like, int comment, int share)> ExtractReelInteractionsAsync(IPage page)
        {
            int likes = 0, comments = 0, shares = 0;

            try
            {
                Libary.Instance.LogDebug("[Reel] 🎬 Bắt đầu đọc tương tác Reel");

                // =================================================
                // 1️⃣ LẤY TOÀN BỘ KHỐI INTERACTION
                // =================================================
                var interactBlocks =
                    await page.QuerySelectorAllAsync("div.xuk3077.x78zum5");

                if (interactBlocks == null || interactBlocks.Count == 0)
                {
                    Libary.Instance.LogDebug("[Reel] ⚠️ Không tìm thấy block tương tác");
                    return (0, 0, 0);
                }

                // =================================================
                // 2️⃣ DUYỆT TỪNG BLOCK → TÌM THEO aria-label
                // =================================================
                foreach (var block in interactBlocks)
                {
                    // 👍 LIKE
                    var likeEl = await block.QuerySelectorAsync("div[aria-label='Thích']");
                    if (likeEl != null)
                    {
                        string txt = (await likeEl.InnerTextAsync() ?? "").Trim();
                        likes = PageDAO.Instance.ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] 👍 Like = {likes}");
                    }

                    // 💬 COMMENT
                    var cmtEl = await block.QuerySelectorAsync("div[aria-label='Bình luận']");
                    if (cmtEl != null)
                    {
                        string txt = (await cmtEl.InnerTextAsync() ?? "").Trim();
                        comments = PageDAO.Instance.ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] 💬 Comment = {comments}");
                    }

                    // 🔁 SHARE
                    var shareEl = await block.QuerySelectorAsync("div[aria-label='Chia sẻ']");
                    if (shareEl != null)
                    {
                        string txt = (await shareEl.InnerTextAsync() ?? "").Trim();
                        shares = PageDAO.Instance.ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] 🔁 Share = {shares}");
                    }

                    // 👉 đã lấy đủ thì break
                    if (likes > 0 || comments > 0 || shares > 0)
                        break;
                }

                Libary.Instance.LogDebug(
                    $"[Reel] ✅ Kết quả cuối: Like={likes}, Comment={comments}, Share={shares}"
                );

                return (likes, comments, shares);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [Reel] Lỗi khi đọc tương tác: {ex.Message}");
                return (likes, comments, shares);
            }
        }
        public async Task<(string PosterName, string PosterLink)> ExtractPosterInfoAsync(IElementHandle postNode)
        {
            string posterName = "N/A";
            string posterLink = "N/A";

            try
            {
                // ============================================
                // 1️⃣ ƯU TIÊN: thẻ <a> có aria-label
                // ============================================
                var aProfiles = await postNode.QuerySelectorAllAsync("a[aria-label='Xem trang cá nhân của chủ sở hữu']");

                if (aProfiles != null && aProfiles.Count >= 2)
                {
                    var aProfile = aProfiles[1]; // 👉 lấy cái thứ 2

                    posterLink = await aProfile.GetAttributeAsync("href") ?? "N/A";
                    posterName = (await aProfile.InnerTextAsync())?.Trim() ?? "N/A";

                    Libary.Instance.LogDebug(
                        $"[Poster] ✅ aria-label (2nd) | Name='{posterName}' | Link='{posterLink}'"
                    );

                    return (posterName, posterLink);
                }

                // ============================================
                // 2️⃣ FALLBACK: span class username
                // ============================================
                var spanName = await postNode.QuerySelectorAsync(
                    "span.x1lliihq.x6ikm8r.x10wlt62.x1n2onr6"
                );

                if (spanName != null)
                {
                    posterName = (await spanName.InnerTextAsync())?.Trim() ?? "N/A";

                    Libary.Instance.LogDebug(
                        $"[Poster] 🟡 fallback span | Name='{posterName}'"
                    );

                    return (posterName, posterLink);
                }

                Libary.Instance.LogDebug("[Poster] ❌ Không tìm thấy PosterName / PosterLink");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"[Poster] ❌ Lỗi ExtractPosterInfoAsync: {ex.Message}");
            }

            return (posterName, posterLink);
        }

        public async Task<string> GetReelTextAsync(IPage page, IElementHandle post)
        {
            try
            {
                // 🎯 Tìm vùng caption của reel
                var captionDiv = await post.QuerySelectorAsync("div[class = 'xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']");

                if (captionDiv == null)
                {
                    Libary.Instance.LogDebug("⚠️ Không tìm thấy vùng caption reel.");
                    return "N/A";
                }
                // 🔍 1) Tìm nút 'Xem thêm' trong vùng caption
                var seeMoreBtn = await captionDiv.QuerySelectorAsync(
                    "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                );

                if (seeMoreBtn != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} [Reel] 🔽 Tìm thấy 'Xem thêm' → click");

                    try
                    {
                        // Scroll vào giữa màn hình để tránh bị che
                        await page.EvaluateAsync("el => el.scrollIntoView({block:'center'})", seeMoreBtn);
                        await page.WaitForTimeoutAsync(150);

                        await seeMoreBtn.ClickAsync(new ElementHandleClickOptions
                        {
                            Timeout = 1500
                        });

                        await page.WaitForTimeoutAsync(250);
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogDebug($"{Libary.IconFail} [Reel] ⚠️ Click thường lỗi → fallback JS click: " + ex.Message);
                        await page.EvaluateAsync("(el)=>{ try { el.click(); } catch {} }", seeMoreBtn);
                        await page.WaitForTimeoutAsync(200);
                    }
                }
                // 🔹 2) Lấy toàn bộ text sau khi mở rộng
                var spans = await captionDiv.QuerySelectorAllAsync(
                    "span[dir='auto'], div[dir='auto']"
                );

                var allLines = new List<string>();

                foreach (var span in spans)
                {
                    string text = (await span.InnerTextAsync())?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(text))
                        allLines.Add(text);
                }

                // 🧹 3) LOẠI TRÙNG caption (do FB render collapsed/expanded)
                allLines = allLines
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .Distinct()
                    .ToList();

                string content = string.Join(" ", allLines).Trim();

                // 4) fallback nếu vẫn rỗng
                if (string.IsNullOrWhiteSpace(content))
                {
                    var raw = await captionDiv.GetPropertyAsync("textContent");
                    content = (raw?.ToString() ?? "").Trim('"');
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} ⚠️ Không lấy được caption reel.");
                    return "N/A";
                }

                Libary.Instance.LogDebug($"🎉 [Reel] Caption: {content.Length} ký tự");
                return content;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("❌ Lỗi GetReelTextAsync: " + ex.Message);
                return "N/A";
            }
        }
    }
}
