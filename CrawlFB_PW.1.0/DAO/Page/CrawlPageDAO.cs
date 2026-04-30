using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.DAO.Post;
using System.Runtime.Remoting.Messaging;
namespace CrawlFB_PW._1._0.DAO.Page
{
    public class CrawlPageDAO
    {
        public CrawlPageDAO() { }
        private static CrawlPageDAO _instance;
        public static CrawlPageDAO Instance
        {
            get
            {
                if (_instance == null) _instance = new CrawlPageDAO();
                return _instance;
            }
        }
        // =====================================================
        // PUBLIC ENTRY
        // =====================================================
        public class RawPostInfo
        {
            // ======================
            // CONTEXT
            // ======================
            public CrawlContext Context { get; set; }
            public IPage Page { get; set; }
            public IElementHandle PostNode { get; set; }

            // ======================
            // PAGE / GROUP CONTEXT
            // ======================
            public string PageName { get; set; }        // tên page/group đang crawl
            public string PageLink { get; set; }
            public string PageID { get; set; }
            // 🔥 thêm ngay từ đầu
            public string ContainerIdFB { get; set; } = "";

            // sau này
            public string PosterIdFB { get; set; } = "";// link page/group

            // ======================
            // RAW DOM BLOCKS
            // ======================
            public IReadOnlyList<IElementHandle> PostInfor { get; set; }

            // ======================
            // TIME & LINK (RAW)
            // ======================
            public List<string> TimeList { get; set; } = new List<string>();
            public List<string> LinkList { get; set; } = new List<string>();

            // ======================
            // MAIN POST (SAU DETECTOR)
            // ======================
            public string PostTime { get; set; } 
            public string PostLink { get; set; } 

            // ======================
            // ORIGINAL POST (NẾU CÓ)
            // ======================
            public string OriginalPostTime { get; set; } 
            public string OriginalPostLink { get; set; } 

            // ======================
            // REEL DETECT
            // ======================
            public bool HasReel { get; set; }
            public string ReelLink { get; set; } 
            public string PostTimeReel { get; set; }
            //============= video Detect
            public bool HasVideo { get; set; }
            public string VideoLink { get; set; } 
            public string PostTimeVideo { get; set; } 
            //photo Detect
            public bool HasPhoto { get; set; }
            public List<(string Src, string Alt)> PhotoList { get; set; }

            // ======================
            // FLAGS (CHƯA LOGIC)
            // ======================
            public int TimeCount => TimeList?.Count ?? 0;
            public int LinkCount => LinkList?.Count ?? 0;
            public int PostInforCount => PostInfor?.Count ?? 0;
        }
        public async Task<PostResult> CrawlPagePostAsync(IPage page,IElementHandle postNode, string pageName,string pageLink,CrawlContext context, string pageIdCrawl,string idFBPageCrawl)
        {
            var raw = await CollectRawInfoAsync(
                page,
                postNode,
                pageName,
                pageLink,
                pageIdCrawl,
                idFBPageCrawl
            );
            raw.Context = context;
            PostKind kind = DetectPostKind(raw);
            return await ParseByKindAsync(raw, kind);
        }
        // =====================================================
        // STEP 1 — COLLECT RAW INFO
        // =====================================================
        public async Task<RawPostInfo> CollectRawInfoAsync(IPage page,IElementHandle postNode,string pageName,string pageLink,string pageIdCrawl,string idFBPageCrawl)
        {
            var raw = new RawPostInfo
            {
                Page = page,
                PostNode = postNode,
                PageName = pageName,
                PageLink = pageLink,
                PageID = Clean(pageIdCrawl),
                ContainerIdFB = Clean(idFBPageCrawl)
            };
            // ========================
            // CONTEXT (FANPAGE / GROUP)
            // ========================
            raw.Context = pageLink != null && pageLink.Contains("/groups/")
                ? CrawlContext.Group
                : CrawlContext.Fanpage;
            Libary.StartPost("BẮT ĐẦU PHÂN TÍCH");
            Libary.Instance.LogDebug("================BẮT ĐẦU PHÂN TÍCH==============");
            Libary.Instance.LogTech($"[CollectRawInfo] ▶ Start | Page={pageName} | Context={raw.Context} | IDFB = {raw.ContainerIdFB}");
            try
            {
                // ========================
                // 1️⃣ DOM postinfor
                // ========================
                raw.PostInfor = await postNode.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");
                Libary.Instance.LogTech($"[CollectRawInfo] postinfor.Count = {raw.PostInfor?.Count ?? 0}");

                // ========================
                // 2️⃣ TIME & LINK
                // ========================
                (raw.TimeList, raw.LinkList) = await CrawlBaseDAO.Instance.ExtractTimeAndLinksAsync(raw.PostInfor);

                Libary.Instance.LogTech( $"[CollectRawInfo] timeList={raw.TimeList?.Count ?? 0}, linkList={raw.LinkList?.Count ?? 0}");

                // ========================
                // 3️⃣ REEL DETECT
                // ========================
                var (hasReel, rawReelLink, timeraw) = await CrawlBaseDAO.Instance.DetectReelFromPostAsync(postNode);

                raw.HasReel = hasReel;
                raw.ReelLink = ProcessingHelper.NormalizeReelLink(rawReelLink);
                raw.PostTimeReel = timeraw;
                Libary.Instance.LogTech( $"[CollectRawInfo] hasReel={raw.HasReel}, reelLink={raw.ReelLink}");

                // ===== VIDEO =====
                var (hasvideo, rawvideolink, timevideoraw) =await CrawlBaseDAO.Instance.DetectVideoFromPostAsync(postNode);

                raw.HasVideo = hasvideo;
                raw.VideoLink = ProcessingHelper.NormalizeReelLink(rawvideolink);
                raw.PostTimeVideo = timevideoraw;

                // ===== PHOTO =====
                List<(string Src, string Alt)> photos =await CrawlBaseDAO.Instance.DetectPhotosFromPostAsync(postNode);

                raw.HasPhoto = photos != null && photos.Count > 0;
                raw.PhotoList = photos;
          
                // ========================
                // 4️⃣ POST TIME / LINK
                // ========================
                (raw.PostTime,
                 raw.OriginalPostTime,
                 raw.PostLink,
                 raw.OriginalPostLink) = await CrawlBaseDAO.Instance.PostTypeDetectorAsync(raw.TimeList, raw.LinkList);
                Libary.Instance.LogTech( "[CollectRawInfo] Detect result\n" +
                    $"   🔗 PostLink        : {raw.PostLink}\n" +
                    $"   ⏰ PostTime        : {raw.PostTime}\n" +
                    $"   🔁 OriginalLink    : {raw.OriginalPostLink}\n" +
                    $"   ⏳ OriginalTime    : {raw.OriginalPostTime}"
                );
                Libary.Instance.LogTech( "[CollectRawInfo] ◀ End OK");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech( $"[CollectRawInfo] ❌ Exception: {ex.Message}");
            }
            return raw;
        }
        // =====================================================
        // STEP 2 — DETECT POST KIND
        // =====================================================
        private PostKind DetectPostKind(RawPostInfo raw)
        {
            // ========================
            // 1️⃣ REEL GỐC (không có time)
            // ========================
            if (raw.HasReel && raw.TimeCount == 0)
            {
                Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Reel (Reel + TimeCount = 0)");
                return PostKind.Reel;
            }
            // ========================
            // 2️⃣ CÓ REEL
            // ========================
            if (raw.HasReel)
            {
                // GROUP + 1 TIME → Reel (chưa rõ gốc/share)
                if (raw.Context == CrawlContext.Group && raw.TimeCount == 1)
                {
                    Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ReelUnknow (Group + 1 time + Reel)");
                    return PostKind.ReelUnknow;
                }

                // FANPAGE + 1 TIME
                if (raw.Context == CrawlContext.Fanpage && raw.TimeCount == 1)
                {
                    if (raw.PostLink == raw.ReelLink)
                    {
                        Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Reel (Fanpage + 1 time + Reel)");
                        return PostKind.ReelHasTime;
                    }

                    Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ShareReel (Fanpage + 1 time + Reel)");
                    return PostKind.ShareReel;
                }

                // GROUP + >=2 TIME → SHARE REEL
                if (raw.Context == CrawlContext.Group && raw.TimeCount >= 2)
                {
                    Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ShareReel (Group + >=2 time + Reel)");
                    return PostKind.ShareReel;
                }
            }
            // ========================
            // 3️⃣ KHÔNG REEL + >=2 TIME
            // ========================
            if (!raw.HasReel && raw.TimeCount >= 2)
            {
                Libary.Instance.LogTech("[DetectPostKind] 👉 Result = ShareNormal (>=2 time, no reel)");
                return PostKind.ShareNormal;
            }

            // ========================
            // 4️⃣ CÒN LẠI
            // ========================
            Libary.Instance.LogTech("[DetectPostKind] 👉 Result = Normal");
            return PostKind.Normal;
        }
        // =====================================================
        // STEP 3 — PARSE BY KIND
        // =====================================================
        private async Task<PostResult> ParseByKindAsync(RawPostInfo raw, PostKind kind)
        {
            switch (kind)
            {
                case PostKind.Normal:
                    return await ParseNormalPostAsync(raw);

                case PostKind.ShareNormal:
                    return await ParseSharePostAsync(raw, ShareMode.Normal);

                case PostKind.ShareReel:
                    return await ParseSharePostAsync(raw, ShareMode.Reel);

                case PostKind.Reel:
                    return await ParseReelAsync(raw);
                case PostKind.ReelUnknow:
                    return await ParseReelUnknowAsync(raw);
                case PostKind.ReelHasTime:
                    return await ParseReelHasTimeAsync(raw);
                default:
                    return new PostResult();
            }
        }
        // =====================================================
        // PARSE — NORMAL POST
        // =====================================================
        private async Task<PostResult> ParseNormalPostAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };
            Libary.Instance.LogTech($"{Libary.IconInfo}▶ Start | Context={raw.Context} | Page={raw.PageName}");
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            // ========================
            // INIT RAW INFO
            // ========================
            var info = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink,
                PageID = raw.PageID,
                ContainerIdFB = raw.ContainerIdFB,
                PostType = PostType.Page_Normal
            };
            Libary.Instance.LogTech($"[ParseNormalPost] PostLink={info.PostLink} | PostTime={info.PostTime}");
            // ========================
            // 1️⃣ POSTER
            // ========================
            if (raw.Context == CrawlContext.Fanpage)
            {
                info.PosterName = raw.PageName;
                info.PosterLink = raw.PageLink;
                info.PosterIdFB = raw.ContainerIdFB;
                info.PosterNote = FBType.Fanpage;
                info.ContainerType = FBType.Fanpage;
                Libary.Instance.LogTech("[ParseNormalPost] Poster = Fanpage (gán cứng)");
            }
            else
            {
                info.ContainerType = FBType.GroupOn;
                await CrawlBaseDAO.Instance.FillPosterInfoAsync(info, page, post, postinfor);
            }
            // ========================
            // 2️⃣ CONTENT
            // ========================
            await CrawlBaseDAO.Instance.FillFullContentPostNormalAsync(info, page, post, postinfor);        
            //=======
            // 🔗 ATTACHMENT
            info.AttachmentJson = AttachmentHelper.BuildAttachmentJson(
                raw.HasVideo,
                raw.VideoLink,
                raw.PostTimeVideo,
                raw.HasPhoto ? raw.PhotoList : null
            );
            // ========================
            // ========================
            // 🖼 GỘP ALT ẢNH VÀO CONTENT
            // ========================
            if (raw.HasPhoto && raw.PhotoList != null && raw.PhotoList.Count > 0)
            {
                for (int i = 0; i < raw.PhotoList.Count; i++)
                {
                    string alt = raw.PhotoList[i].Alt;
                    if (!string.IsNullOrWhiteSpace(alt))
                    {
                        info.Content += "\n" + alt;

                    }
                }
            }

            // ========================
            // 3️⃣ XÁC ĐỊNH POST TYPE (FINAL)
            // ========================
            bool hasContent = ProcessingHelper.IsValidContent(info.Content);

            if (raw.HasPhoto)
            {
                info.PostType = hasContent
                    ? PostType.Page_Photo_Cap
                    : PostType.Page_Photo_NoCap;
            }
            else
            {
                info.PostType = hasContent
                    ? PostType.Page_Normal
                    : PostType.Page_NoConent;
            }
            // ========================
            // 3️⃣ INTERACTION
            // ========================
            (info.LikeCount, info.CommentCount, info.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            Libary.Instance.LogTech($"[ParseNormalPost] 👍{info.LikeCount} 💬{info.CommentCount} 🔁{info.ShareCount}");
            // ========================
            // 4️⃣ BUILD POST
            // ========================
            var postPage = BuildPostPage(info);
            result.Posts.Add(postPage);
            Libary.Instance.LogTech($"[ParseNormalPost] ✅ ADD POST | Link={postPage.PostLink} | Type={postPage.PostType}");
            Libary.Instance.LogTech("[ParseNormalPost] ◀ End | Normal post OK");
            return result;
        }
        //===================
        // PARSE - REEL VẪN LẤY KIỂU THƯỜNG
        //===============
        private async Task<PostResult> ParseReelHasTimeAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            Libary.Instance.LogTech($"{Libary.IconInfo}▶ Start | Context={raw.Context} | Page={raw.PageName}");
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            // =================================================
            // INIT INFO
            // =================================================
            var info = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink,
                PageID = raw.PageID,
                ContainerIdFB = raw.ContainerIdFB
            };

            // =================================================
            // 1️⃣ POSTER
            // =================================================
            if (raw.Context == CrawlContext.Fanpage)
            {
                info.PosterName = raw.PageName;
                info.PosterLink = raw.PageLink;
                info.PosterNote = FBType.Fanpage;
                info.PosterIdFB = raw.ContainerIdFB;
                info.ContainerType = FBType.Fanpage;
            }
            else
            {
                info.ContainerType = FBType.GroupOn;
                await CrawlBaseDAO.Instance.FillPosterInfoAsync(info, page, post, postinfor);
            }

            // =================================================
            // 2️⃣ CONTENT + INTERACTION (FEED)
            // =================================================
            int c = postinfor?.Count ?? 0;
            if (c >= 3)
            {
                info.Content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);
            }
            info.PostType = ProcessingHelper.IsValidContent(info.Content) ? PostType.page_Real_Cap : PostType.Page_Reel_NoCap;   // hoặc PostType.Page_Unknow

            (info.LikeCount, info.CommentCount, info.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            // =================================================
            // 3️⃣ BỔ SUNG REEL DETAIL NẾU THIẾU
            // =================================================
            if (CrawlBaseDAO.Instance.NeedFetchReelDetail(info))
            {
                Libary.Instance.LogTech( "[ParseReelHasTime] 🔎 Thiếu dữ liệu → mở Reel để lấy tiếp");

                var reel = await CrawlPostReelDAO.Instance.ExtractPostReelAll(page, post);

                if (reel != null && !string.IsNullOrWhiteSpace(reel.PostLink))
                {
                    CrawlBaseDAO.Instance.MergeReelInfoIfEmpty(info, reel);
                }
                else
                {
                    Libary.Instance.LogTech("[ParseReelHasTime] ⚠️ Không lấy được Reel detail");
                }
            }

            // =================================================
            // 4️⃣ BUILD POST
            // =================================================
            var postPage = BuildPostPage(info);
            result.Posts.Add(postPage);

            Libary.Instance.LogTech($"[ParseReelHasTime] ✅ ADD POST | Link={postPage.PostLink} | Type={postPage.PostType}");

            Libary.Instance.LogTech(
                "[ParseReelHasTime] ◀ End | ReelHasTime OK");

            return result;
        }
        // =====================================================
        // PARSE — SHARE POST (NORMAL + REEL)
        // =====================================================
        private async Task<PostResult> ParseSharePostAsync(RawPostInfo raw, ShareMode mode)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };
            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            Libary.Instance.LogTech($"[ParseSharePost] ▶ Start | Mode={mode} | PostLink={raw.PostLink}");
            // =====================================================
            // A️⃣ POST SHARE (A)
            // =====================================================
            var infoA = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink,
                PageID = raw.PageID,
                ContainerIdFB = raw.ContainerIdFB
            };

            // ---------- POSTER ----------
            if (raw.Context == CrawlContext.Fanpage)
            {
                infoA.PosterName = raw.PageName;
                infoA.PosterLink = raw.PageLink;
                infoA.PosterIdFB = raw.ContainerIdFB;
                infoA.PosterNote = FBType.Fanpage;
                infoA.ContainerType = FBType.Fanpage;
            }
            else
            {
                infoA.ContainerType = FBType.GroupOn;
                await CrawlBaseDAO.Instance.FillPosterInfoAsync(infoA, page, post, postinfor);
            }
            // ---------- CONTENT SHARE ----------
            // ⭐ tách từ case 2 cũ
            var (contentShare, contentOriginal, postTypeShare, originalPostType) = await ParseShareContentAsync(page, post, postinfor, mode == ShareMode.Reel);
            infoA.Content = contentShare;
            infoA.PostType = postTypeShare;
            Libary.Instance.LogTech($"[Share-A] ContentLen={(infoA.Content?.Length ?? 0)} | Type={infoA.PostType}");
            // ---------- INTERACTION ----------
            (infoA.LikeCount, infoA.CommentCount, infoA.ShareCount) = await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(post);
            Libary.Instance.LogTech($"[Share-A] 👍{infoA.LikeCount} 💬{infoA.CommentCount} 🔁{infoA.ShareCount}");
            // ---------- ADD POST A ----------
            var postA = BuildPostPage(infoA);
            result.Posts.Add(postA);
            // =====================================================
            // B️⃣ POST GỐC (B)
            // =====================================================
            PostPage postB = null;
            var infoB = new PostInfoRawVM
            {
                PostLink = raw.OriginalPostLink,
                PostTime = raw.OriginalPostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.OriginalPostTime),
            };
            if (mode == ShareMode.Reel)
            {
                Libary.Instance.LogTech("[Share-B] Open REEL original (NEW FLOW)");

                await CrawlPostReelDAO.Instance.OpenReelShareAndInitVMAsync(page, infoB);
                postB = BuildPostPage(infoB);
            }
            else if(mode == ShareMode.Normal)
            {
                infoB.Content = contentOriginal;
                infoB.PostType = originalPostType;
                Libary.Instance.LogTech($"[Share-B] Open ORIGINAL post | Link={raw.OriginalPostLink}");

                if (CrawlBaseDAO.Instance.NeedFetchReelDetail(infoB))
                {
                    var postOri = await CrawlBaseDAO.Instance.GetPostOriginalInfoAsync(page,raw.OriginalPostLink);
                    if (postOri != null)
                    {
                        CrawlBaseDAO.Instance.MergeRawInfoIfEmpty(infoB, postOri);
                    }
                }
                else
                {
                    Libary.Instance.LogTech("[Share-B] Skip GetPostOriginal – info already sufficient from FEED");
                }
                postB = BuildPostPage(infoB);
            }

            if (postB != null && !string.IsNullOrWhiteSpace(postB.PostLink))
            {
                result.Posts.Add(postB);

                // =================================================
                // C️⃣ SHARE MAP
                // =================================================
                result.Shares.Add(new ShareItem
                {
                    PageLinkA = raw.PageLink,
                    PostLinkB = postB.PostLink,
                    ShareTimeRaw = infoA.PostTime,
                    ShareTimeReal = infoA.RealPostTime ?? DateTime.MinValue
                });

                Libary.Instance.LogTech($"[Share-MAP] A={raw.PageLink} → B={postB.PostLink}");
            }

            Libary.Instance.LogTech($"[ParseSharePost] ◀ End | Posts={result.Posts.Count}, Shares={result.Shares.Count}");

            return result;
        }
        private async Task<(string contentShare, string contentOriginal, PostType postTypeShare, PostType originalPostType)> ParseShareContentAsync(
         IPage page,
         IElementHandle post,
         IReadOnlyList<IElementHandle> postinfor,
         bool isReel)
        {
            string noidung = null;
            string noidunggoc = null;

            PostType postType = PostType.Share_NoContent;
            PostType originalPostType = PostType.Page_Unknow;

            int c = postinfor?.Count ?? 0;

            Libary.Instance.LogDebug( $"[ParseShareContent] Start | isReel={isReel} | postinfor.Count={c}");
            try
            {
                // =================================================
                // 🟦 SHARE REEL (ĐẶC BIỆT)
                // postinfor.Count == 3
                // content share nằm ở index 2
                // =================================================
                if (isReel && c == 3)
                {
                    noidung = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);

                    if (!string.IsNullOrWhiteSpace(noidung))
                    {
                        postType = PostType.Share_WithContent;
                        originalPostType = PostType.Page_Unknow;

                        Libary.Instance.LogDebug( "[ParseShareContent] SHARE REEL: Có content share (index 2)");
                    }
                    else
                    {
                        postType = PostType.Share_NoContent;

                        Libary.Instance.LogDebug( "[ParseShareContent] SHARE REEL: Không có content share");
                    }
                    return (noidung, noidunggoc, postType, originalPostType);
                }

                // =================================================
                // 🟩 SHARE THƯỜNG (LOGIC CŨ)
                // =================================================
                if (c >= 5)
                {
                    var el2 = postinfor[2];
                    var el4 = postinfor[4];

                    string content2 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el2);
                    string content4 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el4);

                    bool hasContent2 = !string.IsNullOrWhiteSpace(content2) && content2 != "N/A";
                    bool hasContent4 = !string.IsNullOrWhiteSpace(content4) && content4 != "N/A";

                    Libary.Instance.LogDebug($"[ParseShareContent] content2.len={(content2?.Length ?? 0)}, content4.len={(content4?.Length ?? 0)}");

                    // =========================
                    // CASE 6
                    // =========================
                    if (c == 6)
                    {
                        var el5 = postinfor[5];
                        string content5 = await CrawlBaseDAO.Instance.GetContentTextAsync(page, el5);
                        bool hasContent5 = !string.IsNullOrWhiteSpace(content5) && content5 != "N/A";

                        Libary.Instance.LogDebug($"[ParseShareContent] content5.len={(content5?.Length ?? 0)}");

                        if (hasContent2 && hasContent5)
                        {
                            noidung = content2;
                            noidunggoc = content5;

                            postType = PostType.Share_WithContent;
                            originalPostType = PostType.Page_Normal;

                            Libary.Instance.LogDebug("[ParseShareContent] CASE 6: Share + Original đều có content");
                        }
                        else if (!hasContent2 && !hasContent5 && hasContent4)
                        {
                            noidunggoc = content4;

                            postType = PostType.Share_NoContent;
                            originalPostType = PostType.Page_Normal;

                            Libary.Instance.LogDebug( "[ParseShareContent] CASE 6: Chỉ có content gốc (container 4)");
                        }
                        else if (hasContent2)
                        {
                            noidung = content2;

                            postType = PostType.Share_WithContent;
                            originalPostType = PostType.Page_Unknow;

                            Libary.Instance.LogDebug( "[ParseShareContent] CASE 6: Chỉ có content share");
                        }
                        else
                        {
                            Libary.Instance.LogDebug("[ParseShareContent] CASE 6: Không lấy được content");
                        }
                    }
                    // =========================
                    // CASE 5
                    // =========================
                    else
                    {
                        if (hasContent2)
                        {
                            noidung = content2;
                            postType = PostType.Share_WithContent;

                            Libary.Instance.LogDebug("[ParseShareContent] CASE 5: Share có content");
                        }
                        else if (hasContent4)
                        {
                            noidunggoc = content4;
                            postType = PostType.Share_NoContent;
                            originalPostType = PostType.Page_Normal;
                            Libary.Instance.LogDebug("[ParseShareContent] CASE 5: Chỉ có content gốc");
                        }
                        else
                        {
                            Libary.Instance.LogDebug( "[ParseShareContent] CASE 5: Không lấy được content");
                        }
                    }
                }
                // =========================
                // CASE 4
                // =========================
                else if (c == 4)
                {
                    noidung = await CrawlBaseDAO.Instance.BackgroundTextAllAsync(page, post);
                    if (!string.IsNullOrWhiteSpace(noidung))
                    {
                        postType = PostType.Share_NoContent;
                        Libary.Instance.LogDebug( "[ParseShareContent] CASE 4: Background share");
                    }
                    else
                    {
                        Libary.Instance.LogDebug( "[ParseShareContent] CASE 4: Không có content");
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"[ParseShareContent] ❌ Exception: {ex.Message}");
            }
            Libary.Instance.LogDebug( $"[ParseShareContent] End | ShareType={postType}, ShareLen={(noidung?.Length ?? 0)}, OriginalLen={(noidunggoc?.Length ?? 0)}");
            return (noidung, noidunggoc, postType, originalPostType);
        }
        // =====================================================
        // PARSE — REEL ORIGINAL
        // =====================================================
        private async Task<PostResult> ParseReelAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            Libary.Instance.LogTech($"[ParseReelOriginal] ▶ Start | Page={raw.PageName}");
            var reelPost = new PostPage();
            try
            {

                if (raw.Context == CrawlContext.Fanpage)
                {
                    Libary.Instance.LogTech("----FanPage Reel----");
                    // ========================
                    // 1️⃣ OPEN & EXTRACT REEL
                    // ========================
                    reelPost = await CrawlPostReelDAO.Instance.ExtractPostReelAll(raw.Page, raw.PostNode);
                    reelPost.PageName = raw.PageName;
                    reelPost.PageLink = raw.PageLink;
                }
                else if (raw.Context == CrawlContext.Group)
                {
                    reelPost = await CrawlPostReelDAO.Instance.ExtractPostReelAll(raw.Page, raw.PostNode);
                }
                if (reelPost == null || reelPost.PostLink == "N/A")
                {
                    Libary.Instance.LogTech("[Reel] ❌ ExtractPostReelAll trả về NULL / PostLink=N/A");
                    return result;
                }
                // ========================
                // 2️⃣ GÁN CONTEXT PAGE
                // ========================
                Libary.Instance.LogTech(
                    $"[Reel] ✅ Lấy Reel thành công\n" +
                    $"   🔗 Link      : {reelPost.PostLink}\n" +
                    $"   👤 Người đăng: {reelPost.PosterName}\n" +
                    $"   ⏰ Thời gian Raw : {reelPost.PostTime}\n" +
                     $"   ⏰ Thời gian Raw : {TimeHelper.NormalizeTime(reelPost.RealPostTime)}\n" +
                     $"   👍 PosterNote      : {reelPost.PosterNote}\n" +
                    $"   👍 Like      : {reelPost.LikeCount}\n" +
                    $"   💬 Comment   : {reelPost.CommentCount}\n" +
                    $"   🔁 Share     : {reelPost.ShareCount}"
                );

                if (!string.IsNullOrWhiteSpace(reelPost.Content) && reelPost.Content != "N/A")
                {
                    Libary.Instance.LogTech(
                        $"   📝 Caption   : {ProcessingHelper.PreviewText(reelPost.Content)}");
                }
                else
                {
                    Libary.Instance.LogTech(
                        "   📝 Caption   : (không có)");
                }
                // ========================
                // 3️⃣ ADD RESULT
                // ========================
                result.Posts.Add(reelPost);

                Libary.Instance.LogTech("[ParseReelOriginal] ◀ End | Reel OK");

            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"[ParseReelOriginal] ❌ Exception: {ex.Message}");
            }

            return result;
        }
        private async Task<PostResult> ParseReelUnknowAsync(RawPostInfo raw)
        {
            var result = new PostResult
            {
                Posts = new List<PostPage>(),
                Shares = new List<ShareItem>()
            };

            var page = raw.Page;
            var post = raw.PostNode;
            var postinfor = raw.PostInfor;
            Libary.Instance.LogTech($"[ParseReelUnknow] ▶ Start | Group={raw.PageName}");

            // =================================================
            // A️⃣ POST TRONG GROUP (LUÔN CÓ – LUÔN FILL)
            // =================================================
            var infoA = new PostInfoRawVM
            {
                PostLink = raw.PostLink,
                PostTime = raw.PostTime,
                RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTime),
                PageName = raw.PageName,
                PageLink = raw.PageLink,
                PageID = raw.PageID,
               ContainerType = raw.Context == CrawlContext.Fanpage? FBType.Fanpage: FBType.GroupOn,
                ContainerIdFB = raw.ContainerIdFB
            };
            Libary.Instance.LogTech($"INFOA IDFB: {infoA.ContainerIdFB}");
            // 1️⃣ Poster
            await CrawlBaseDAO.Instance.FillPosterInfoAsync(infoA, page, post, postinfor);
            // 2️⃣ Content + Interaction (LUÔN LẤY)
            await CrawlBaseDAO.Instance.FillContentAndInteractionNormalAsync(infoA, page, post, postinfor);

            Libary.Instance.LogTech(
             $"[Reel-A] Poster={infoA.PosterName} | " +
             $"PosterLink={infoA.PosterLink} | " +
             $"IDFB={infoA.PosterIdFB} | " +
             $"ContentLen={(infoA.Content?.Length ?? 0)}"
         );

            try
            {
                Libary.Instance.LogTech("[Reel] 🔗 Open reel to detect REAL vs SHARE");

                // infoReel chỉ là RAW, chưa build
                var infoB = new PostInfoRawVM
                {
                    PostLink = raw.ReelLink,
                    PostTime = raw.PostTimeReel,
                    RealPostTime = TimeHelper.ParseFacebookTime(raw.PostTimeReel),                   
                };
                // =================================================
                // B️⃣ OPEN REEL GỐC
                // =================================================            
               await CrawlPostReelDAO.Instance.OpenReelShareAndInitVMAsync(page, infoB);
              
                Libary.Instance.LogTech($"[Reel-B] Poster={infoB.PosterName} | IDFBPoster = {infoB.PosterIdFB} | Page={infoB.PageName} | ContentLen={(infoB.Content?.Length ?? 0)}");

                // =================================================
                // C️⃣ SO SÁNH NGHIỆP VỤ (KHÔNG SO LINK)
                // =================================================
                bool hasPosterA = ProcessingHelper.IsValidContent(infoA.PosterIdFB);
                bool hasPosterB = ProcessingHelper.IsValidContent(infoB.PosterIdFB);

                bool samePoster =
                    (!hasPosterA || !hasPosterB) // ❗ thiếu dữ liệu → coi như TRUE
                    || infoA.PosterIdFB == infoB.PosterIdFB;

                Libary.Instance.LogTech(
                    $"PosterCheck: A={infoA.PosterIdFB} | B={infoB.PosterIdFB} | " +
                    $"HasA={hasPosterA} | HasB={hasPosterB} | Result={samePoster}"
                );

                bool hasIdA = ProcessingHelper.IsValidContent(infoA.ContainerIdFB);
                bool hasIdB = ProcessingHelper.IsValidContent(infoB.ContainerIdFB);

                bool sameContainer =
                    (!hasIdA || !hasIdB) // ❗ thiếu ID → không fail
                    || infoA.ContainerIdFB == infoB.ContainerIdFB;

                Libary.Instance.LogTech($"Poster: {samePoster} | Container: {sameContainer}");
                Libary.Instance.LogTech($"A.ContainerId: {infoA.ContainerIdFB}");
                Libary.Instance.LogTech($"B.ContainerId: {infoB.ContainerIdFB}");

                bool hasContentA =
                    !string.IsNullOrWhiteSpace(infoA.Content) &&
                    infoA.Content != "N/A";

                bool hasContentB =
                    !string.IsNullOrWhiteSpace(infoB.Content) &&
                    infoB.Content != "N/A";

                double similarity = -1;
                bool isRealReel = false;

                // =================================================
                // 🔑 RULE CHÍNH: ƯU TIÊN ENTITY
                // =================================================
                bool isSameEntity = samePoster && sameContainer;

                if (isSameEntity)
                {
                    if (hasContentA && hasContentB)
                    {
                        similarity = TextSimilarity.Similarity(
                            infoA.Content,
                            infoB.Content);

                        if (similarity >= 0.7)
                        {
                            isRealReel = true;
                        }
                    }
                    else
                    {
                        // ❗ KEY: thiếu content vẫn coi là REAL
                        isRealReel = true;
                    }
                }

                Libary.Instance.LogTech(
                    $"[Reel-Compare] " +
                    $"SamePoster={samePoster} | " +
                    $"SameContainer={sameContainer} | " +
                    $"HasA={hasContentA} | " +
                    $"HasB={hasContentB} | " +
                    $"Sim={(similarity >= 0 ? similarity.ToString("0.00") : "N/A")}"
                );

                //check page person
                bool isPerson =
                    infoB.PosterNote == FBType.Person ||
                    infoB.PosterNote == FBType.PersonKOL ||
                    infoB.PosterNote == FBType.PersonHidden;

                // =================================================
                // D️⃣ KẾT LUẬN
                // =================================================
                if (isRealReel)
                {
                    // 🔄 Dùng luôn hasContentA / hasContentB đã tính ở trên
                    if (!hasContentA && hasContentB)
                    {
                        infoA.Content = infoB.Content;

                        Libary.Instance.LogTech(
                            "[Reel] 🔄 Merge content B → A (A thiếu content)");
                    }

                    var postA = BuildPostPage(infoA);

                    postA.PostType = hasContentA || hasContentB
                        ? PostType.page_Real_Cap
                        : PostType.Page_Reel_NoCap;

                    Libary.Instance.LogTech(
                        "[Reel] ✅ GROUP REEL (REAL – 1 post, NOT share)");

                    result.Posts.Add(postA);
                    return result;
                }

                // =================================================
                // E️⃣ SHARE REEL THỰC SỰ
                // =================================================
                Libary.Instance.LogTech("[Reel] 🔁 SHARE REEL (Group)");
                // ===== SHARE REEL
                var postA2 = BuildPostPage(infoA);
                postA2.PostType = hasContentA
                    ? PostType.Share_Reel_ConTent
                    : PostType.Share_Reel_NoContent;

                var postB = BuildPostPage(infoB);
                postB.PostType =hasContentB
                        ? (isPerson
                            ? PostType.Person_Reel_ConTent
                            : PostType.page_Real_Cap)
                        : (isPerson
                            ? PostType.Person_Reel_NoConent
                            : PostType.Page_Reel_NoCap);
                result.Posts.Add(postA2);
                result.Posts.Add(postB);

                result.Shares.Add(new ShareItem
                {
                    PageLinkA = raw.PageLink,
                    PostLinkB = infoB.PostLink,
                    ShareTimeRaw = infoA.PostTime,
                    ShareTimeReal = infoA.RealPostTime ?? DateTime.MinValue
                });

                Libary.Instance.LogTech( $"[Share-MAP] A={raw.PageLink} → B={infoB.PostLink}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech(
                    $"[ParseReelUnknow] ❌ Exception: {ex.Message}");
            }
            return result;
        }
        private PostPage BuildPostPage(PostInfoRawVM info)
        {
            return new PostPage
            {
                PostLink = info.PostLink,
                PostTime = info.PostTime,
                RealPostTime = info.RealPostTime,

                PosterName = info.PosterName,
                PosterLink = info.PosterLink,
                PosterNote = info.PosterNote,
                PosterIdFB = !string.IsNullOrWhiteSpace(info.PosterIdFB)
                    ? info.PosterIdFB
                    : UrlHelper.ExtractIdFromUrl(info.PosterLink),

                PageName = info.PageName,
                PageLink = info.PageLink,

                PageID = !string.IsNullOrWhiteSpace(info.PageID)
                    ? info.PageID
                    : info.ContainerIdFB,

                ContainerIdFB = !string.IsNullOrWhiteSpace(info.ContainerIdFB)
                    ? info.ContainerIdFB
                    : info.PageID,

                ContainerType = info.ContainerType,

                Content = info.Content,

                LikeCount = info.LikeCount,
                CommentCount = info.CommentCount,
                ShareCount = info.ShareCount,

                Attachment = info.AttachmentJson,
                PostType = info.PostType
            };
        }
        public static string Clean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            s = s.Trim();

            return s.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                ? null
                : s;
        }
    }

}
