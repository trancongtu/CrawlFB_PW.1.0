using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DAO.Page;
using CrawlFB_PW._1._0.DAO.Post;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraEditors.Controls;
using Microsoft.Playwright;
using ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using Ipage = Microsoft.Playwright.IPage;
namespace CrawlFB_PW._1._0.DAO
{
    public class CrawlPostReelDAO
    {
        public CrawlPostReelDAO() { }
        private static CrawlPostReelDAO _instance;
        public static CrawlPostReelDAO Instance
        {
            get
            {
                if (_instance == null) _instance = new CrawlPostReelDAO();
                return _instance;
            }
        }
        // mở link mới lấy thông tin
        //1  lấy tên pagename link page name 
        public async Task<(string OriginalPageName, string OriginalPageLink)>ExtractPageGroupsReel(IPage page, IElementHandle postNode)
        {
            string pageName = null;
            string pageLink = null;

            Libary.Instance.LogDebug(" ▶ Start Reel");

            try
            {
                // 1️⃣ ƯU TIÊN
                var aGroup = await postNode.QuerySelectorAsync("a[aria-label='Xem Nhóm'][href]");
                if (aGroup != null)
                {
                    pageName = Clean(await aGroup.InnerTextAsync());
                    var href = await aGroup.GetAttributeAsync("href");

                    pageLink = Clean(href);

                    if (pageLink != null)
                        pageLink = ProcessingHelper.ShortLinkPage(pageLink);

                    return (pageName, pageLink);
                }

                // 2️⃣ FALLBACK
                var span = await postNode.QuerySelectorAsync(
                    "span[class='x1lliihq x6ikm8r x10wlt62 x1n2onr6']");

                if (span == null)
                    return (null, null);

                pageName = Clean(await span.InnerTextAsync());

                var a = await span.QuerySelectorAsync("a[href]");
                if (a != null)
                {
                    var href = await a.GetAttributeAsync("href");
                    pageLink = Clean(href);

                    if (pageLink != null)
                        pageLink = ProcessingHelper.ShortLinkPage(pageLink);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($" ❌ Exception: {ex.Message}");
            }

            return (pageName, pageLink);
        }
        // 1 lấy postername, posterlink áp dụng groups
        public async Task<(string PosterName, string PosterLink)>ExtractPosterGroupsReel(IElementHandle postNode)
        {
            string posterName = null;
            string posterLink = null;

            Libary.Instance.LogDebug("[ExtractPosterFromHtmlSpan] ▶ Start");

            try
            {
                var span = await postNode.QuerySelectorAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x65f84u']");

                // ======================
                // 1️⃣ FALLBACK nếu không có span
                // ======================
                if (span == null)
                {
                    Libary.Instance.LogDebug(
                        "[ExtractPosterFromHtmlSpan] ❌ Span not found → fallback a[aria-label]");

                    var aa = await postNode.QuerySelectorAsync(
                        "a[aria-label='Xem trang cá nhân của chủ sở hữu']");

                    if (aa == null)
                        return (null, null);

                    posterName = Clean(await aa.InnerTextAsync());

                    var rawHref = await aa.GetAttributeAsync("href");
                    posterLink = Clean(rawHref);

                    if (posterLink != null)
                        posterLink = ProcessingHelper.NormalizeReelPosterProfile(posterLink);

                    return (posterName, posterLink);
                }

                // ======================
                // 2️⃣ NORMAL FLOW
                // ======================
                posterName = Clean(await span.InnerTextAsync());

                var a = await span.QuerySelectorAsync("a[href]");
                if (a != null)
                {
                    var href = await a.GetAttributeAsync("href");
                    posterLink = Clean(href);

                    if (posterLink != null)
                        posterLink = ProcessingHelper.ShortenPosterLink(posterLink);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"[ExtractPosterFromHtmlSpan] ❌ Exception: {ex.Message}");
            }

            return (posterName, posterLink);
        }
        //HÀM MỞ LINK ĐỂ LẤY CONTENT VÀ TƯƠNG TÁC
        public async Task<(string Content, int Like, int Comment, int Share)>OpenReelTabAndExtractAsync(IPage mainPage, string reelLink)
        {
            string content = null;   // 🔥 chỉ đổi N/A → null
            int like = 0, comment = 0, share = 0;

            Libary.Instance.LogDebug($"{Libary.IconInfo} 🌐 [ReelTab] Open reel tab: {reelLink}");

            IPage newPage = null;

            try
            {
                // ========================
                // 1️⃣ OPEN TAB (GIỮ NGUYÊN)
                // ========================
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync($"window.open('{reelLink}', '_blank');");

                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));
                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} ❌ [ReelTab] Timeout mở tab reel");
                    return (content, like, comment, share); // 🔥 null thay vì N/A
                }

                newPage = await popupTask;
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(500);

                // ========================
                // 2️⃣ GET CONTENT (GIỮ NGUYÊN FLOW)
                // ========================
                Libary.Instance.LogDebug($"{Libary.IconInfo} ✍️ [ReelTab] Lấy caption reel");

                var body = await newPage.QuerySelectorAsync("body");

                // 🔥 chỉ thêm Clean, không đổi thuật toán
                content = Clean(await GetReelTextAsync(newPage, body));

                // ========================
                // 3️⃣ GET INTERACTIONS (GIỮ NGUYÊN)
                // ========================
                Libary.Instance.LogDebug($"{Libary.IconInfo} 📊 [ReelTab] Lấy tương tác reel");

                (like, comment, share) = await ExtractReelInteractionsAsync(newPage);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [ReelTab] Exception: {ex.Message}");
            }
            finally
            {
                if (newPage != null)
                {
                    try { await newPage.CloseAsync(); } catch { }
                }
            }

            // 🔥 bỏ "?? N/A"
            return (content, like, comment, share);
        }
        // HÀM BỔ TRỢ
        public async Task<string> GetReelTextAsync(IPage page, IElementHandle post)
        {
            try
            {
                var captionDiv = await post.QuerySelectorAsync("div[class = 'xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']");

                if (captionDiv == null)
                {
                    Libary.Instance.LogDebug("⚠️ Không tìm thấy vùng caption reel.");
                    return null; // 🔥 FIX
                }

                var seeMoreBtn = await captionDiv.QuerySelectorAsync(
                    "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                );

                if (seeMoreBtn != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} [Reel] 🔽 Tìm thấy 'Xem thêm' → click");

                    try
                    {
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

                allLines = allLines
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .Distinct()
                    .ToList();

                string content = string.Join(" ", allLines).Trim();

                if (string.IsNullOrWhiteSpace(content))
                {
                    var raw = await captionDiv.GetPropertyAsync("textContent");
                    content = (raw?.ToString() ?? "").Trim('"');
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} ⚠️ Không lấy được caption reel.");
                    return null; // 🔥 FIX
                }

                Libary.Instance.LogDebug($"🎉 [Reel] Caption: {content.Length} ký tự");
                return content;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug("❌ Lỗi GetReelTextAsync: " + ex.Message);
                return null; // 🔥 FIX
            }
        }
        public async Task<(int like, int comment, int share)> ExtractReelInteractionsAsync(IPage page)
        {
            int likes = 0, comments = 0, shares = 0;
            try
            {
                Libary.Instance.LogDebug("[Reel] 🎬 Bắt đầu đọc tương tác bài Reel...");

                // 1️⃣ Lấy danh sách thẻ reel
                var interactZone = await page.QuerySelectorAllAsync("div[class='xuk3077 x78zum5']");
                if (interactZone == null)
                {
                    Libary.Instance.LogDebug("[Reel] ⚠️ Không thấy vùng tương tác, thử fallback.");
                }
                else
                {
                    var firstReel = interactZone.First();
                    var likeEl = await firstReel.QuerySelectorAsync("div[aria-label='Thích']");
                    if (likeEl != null)
                    {
                        string txt = (await likeEl.InnerTextAsync() ?? "").Trim();
                        likes = ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] 👍 Like = {likes}");
                    }

                    // 3️⃣ Comment
                    var cmtEl = await firstReel.QuerySelectorAsync("div[aria-label='Bình luận']");
                    if (cmtEl != null)
                    {
                        string txt = (await cmtEl.InnerTextAsync() ?? "").Trim();
                        comments = ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] 💬 Comment = {comments}");
                    }

                    // 4️⃣ Share
                    var shareEl = await firstReel.QuerySelectorAsync("div[aria-label='Chia sẻ']");
                    if (shareEl != null)
                    {
                        string txt = (await shareEl.InnerTextAsync() ?? "").Trim();
                        shares = ParseReelNumber(txt);
                        Libary.Instance.LogDebug($"[Reel] ↪️ Share = {shares}");
                    }
                }
                Libary.Instance.LogDebug($"[Reel] ✅ Kết quả cuối: Like={likes}, Comment={comments}, Share={shares}");
                return (likes, comments, shares);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [Reel] Lỗi khi đọc tương tác: {ex.Message}");
                return (likes, comments, shares);
            }
        }
        public int ParseReelNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            text = text.Trim().ToLower();

            try
            {
                // 1️⃣ Nếu có "k"
                if (text.Contains("k"))
                {
                    // Reel thường hiển thị "1,2K" hoặc "1.2K"
                    text = text.Replace("k", "").Replace(",", ".").Trim();
                    if (double.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double v))
                        return (int)(v * 1000);
                }

                // 2️⃣ Nếu có "m"
                if (text.Contains("m"))
                {
                    text = text.Replace("m", "").Replace(",", ".").Trim();
                    if (double.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double v))
                        return (int)(v * 1000000);
                }

                // 3️⃣ Nếu chỉ là số (0–9)
                var digits = new string(text.Where(char.IsDigit).ToArray());
                if (int.TryParse(digits, out int r))
                    return r;
            }
            catch { }

            return 0;
        }
        // HÀM TỔNG LẤY TẤT ÁP DỤNG BÀI REEL ĐƠN THUẦN
        public async Task<PostPage> ExtractPostReelAll(IPage mainPage, IElementHandle post)
        {
            Libary.Instance.LogDebug($"{Libary.IconInfo} Start [ExtractPostReelAll");

            var reel = new PostPage()
            {
                PostType = PostType.page_Real_Cap
            };

            var (pagename, pagelink) = await ExtractPageGroupsReel(mainPage, post);

            if (!string.IsNullOrWhiteSpace(pagename))
            {
                reel.PageName = pagename;
                reel.PageLink = pagelink;
            }

            try
            {
                var posterReel = await post.QuerySelectorAsync("a[class*='x1i10hfl xjbqb8w x1ejq31n x18oe1m7 x1sy0etr xstzfhl x972fbf x10w94by x1qhh985 x14e42zd x9f619 x1ypdohk xt0psk2 x3ct3a4 xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x16tdsg8 x1hl2dhg xggy1nq x1a2a7pz x1heor9g xkrqix3 x1sur9pj x1s688f']");

                if (posterReel != null)
                {
                    string rawHref = await posterReel.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(rawHref))
                    {
                        reel.PosterLink = ProcessingHelper.ShortenPosterLinkReel(rawHref);
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
                            reel.PosterLink = ProcessingHelper.ShortenPosterLinkReel(posterlink);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(reel.PosterLink))
                    Libary.Instance.LogDebug($"{Libary.IconOK} PosterRell OK: {reel.PosterLink}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} PosterRell k lấy được link: {ex.Message}");
            }

            try
            {
                var aTags = await post.QuerySelectorAllAsync("a[href]");

                string reelLink = null;
                string posterLink = null;
                string postTime = null;
                string posterName = null;

                foreach (var a in aTags)
                {
                    string href = await a.GetAttributeAsync("href") ?? "";
                    string text = (await a.InnerTextAsync() ?? "").Trim();

                    if (string.IsNullOrEmpty(href))
                        continue;

                    if (href.Contains("/reel/") && string.IsNullOrWhiteSpace(reelLink))
                    {
                        reelLink = ProcessingHelper.ShortenFacebookPostLink(href);
                        Libary.Instance.LogDebug($"{Libary.IconOK} 🎞️ Found ReelLink = {reelLink}");
                    }

                    reel.PostLink = ProcessingHelper.NormalizeReelLink(reelLink);
                }

                var TimeTag = await post.QuerySelectorAsync("span[class= 'html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x4k7w5x x1h91t0o x1h9r5lt x1jfb8zj xv2umb2 x1beo9mf xaigb6o x12ejxvf x3igimt xarpa2k xedcshv x1lytzrv x1t2pt76 x7ja8zs x1qrby5j']");

                if (TimeTag != null)
                {
                    string time = (await TimeTag.InnerTextAsync() ?? "").Trim();
                    if (ProcessingDAO.Instance.IsTime(time))
                    {
                        postTime = TimeHelper.CleanTimeString(time);
                        reel.PostTime = postTime;
                        reel.RealPostTime = TimeHelper.ParseFacebookTime(postTime);
                    }
                }

                // 🔥 FIX: N/A → null
                if (string.IsNullOrWhiteSpace(reelLink))
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail}❌ [ReelExtract] Không có /reel/");
                    return reel;
                }

                if (!string.IsNullOrWhiteSpace(posterLink))
                    reel.PosterLink = ProcessingDAO.Instance.ShortenPosterLinkReel(posterLink);

                if (!string.IsNullOrWhiteSpace(reel.PosterLink))
                {
                    var (type, idfb) = await CrawlBaseDAO.Instance.CheckTypeCachedAsync(mainPage, reel.PosterLink);
                    reel.PosterNote = type;
                    reel.PosterIdFB = idfb;
                }

                // OPEN TAB (GIỮ NGUYÊN)
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync($"window.open('{reelLink}', '_blank');");

                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));
                if (finished != popupTask)
                {
                    return reel;
                }

                var newPage = await popupTask;
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(500);

                reel.Content = await GetReelTextAsync(newPage, await newPage.QuerySelectorAsync("body"));

                var Divpostername = await newPage.QuerySelectorAsync("span[class='xjp7ctv']>a");
                if (Divpostername != null)
                {
                    posterName = (await Divpostername.InnerTextAsync()).Trim();
                }

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
        public static string Clean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            s = s.Trim();

            return s.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                ? null
                : s;
        }
        //=========II. REEL SHARE - MỞ MỚI LẤY TOÀN BỘ TRỪ TIME, LINKPOST
        //-1. lấy feed node
        public async Task<IElementHandle> GetFirstReelShareFeedAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return null;

            try
            {
                // Đợi feed render
                await page.WaitForSelectorAsync(
                    "div[class='x6s0dn4 x78zum5 x1n2onr6']",
                    new PageWaitForSelectorOptions
                    {
                        Timeout = 8000
                    });

                var feeds = await page.QuerySelectorAllAsync(
                    "div[class='x6s0dn4 x78zum5 x1n2onr6']");

                if (feeds == null || feeds.Count == 0)
                {
                    Libary.Instance.LogDebug(
                        "❌ [ReelShare] Không tìm thấy feed container");
                    return null;
                }

                // 🔥 FIRST = chắc chắn bài đang focus
                var first = feeds.First();

                Libary.Instance.LogDebug( "✅ [ReelShare] Locked FIRST feed container");

                return first;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"❌ [ReelShare] GetFirstReelShareFeedAsync error: {ex.Message}");
                return null;
            }
        }
        //2. lấy page/person
        public async Task<bool> GetContainerReelShareAsync( IPage reelPage,PostInfoRawVM info)
        {
            if (reelPage == null || info == null)
                return false;

            try
            {
                var aGroup = await reelPage.QuerySelectorAsync(
                    "a[aria-label='Xem Nhóm'][href]");

                if (aGroup != null)
                {
                    info.PageName = (await aGroup.InnerTextAsync())?.Trim() ?? info.PageName;
                    info.PageLink = ProcessingHelper.ShortLinkPage(
                        await aGroup.GetAttributeAsync("href"));
                    info.PosterNote = FBType.GroupOn;
                    return true;
                }

                var aOwner = await reelPage.QuerySelectorAsync(
                    "a[aria-label='Xem trang cá nhân của chủ sở hữu'][href]");

                if (aOwner != null)
                {
                    info.PageName = (await aOwner.InnerTextAsync())?.Trim() ?? info.PageName;
                    info.PageLink = ProcessingHelper.ShortenPosterLink(
                        await aOwner.GetAttributeAsync("href"));
                    info.PosterNote = FBType.Unknown;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"[GetContainerReelShare PAGE] ❌ {ex.Message}");
            }
            return false;
        }
        //2.1 lấy page/person dùng feed
        public async Task<bool> GetContainerReelShareAsync(IElementHandle feed, PostInfoRawVM info)
        {
            if (feed == null || info == null)
                return false;

            try
            {
                // ===== GROUP =====
                var groups = await feed.QuerySelectorAllAsync("a[aria-label='Xem Nhóm'][href]");
                if(groups.Count == 0) { Libary.Instance.LogDebug($"[GetContainerReelShare] Không có thẻ Groups"); }
                else
                {
                    foreach (var a in groups)
                    {
                        var name = (await a.InnerTextAsync())?.Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            info.PageName = name;
                            info.PageLink = UrlHelper.ShortenPagePersonLink(await a.GetAttributeAsync("href"));
                            info.ContainerType = FBType.GroupOn;
                            Libary.Instance.LogDebug($"[GetContainerReelShare] GROUP OK | Name='{info.PageName}' | Link='{info.PageLink}'");
                            return true;
                        }              
                    }
                    if (ProcessingHelper.IsValidContent(info.PageName)) Libary.Instance.LogDebug("[GetContainerReelShare] GROUP links found but no valid text");
                }    
               
                var owners = await feed.QuerySelectorAllAsync("a[aria-label='Xem trang cá nhân của chủ sở hữu'][href]");
                if (owners.Count == 0) { Libary.Instance.LogDebug($"[GetContainerReelShare] Không có thẻ Fanpage"); }
                else
                {
                    foreach (var a in owners)
                    {
                        var name = (await a.InnerTextAsync())?.Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            info.PageName = name;
                            info.PageLink = UrlHelper.ShortenPagePersonLink(await a.GetAttributeAsync("href"));
                            info.ContainerType = FBType.Unknown;
                            Libary.Instance.LogDebug($"[GetContainerReelShare] OWNER OK | Name='{info.PageName}' | Link='{info.PageLink}'");
                            return true;
                        }                     
                    }
                    if(ProcessingHelper.IsValidContent(info.PageName)) Libary.Instance.LogDebug("[GetContainerReelShare] Fanpage links found but no valid text");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"[GetContainerReelShare FEED] ❌ {ex.Message}");
            }

            return false;
        }
        //3. lấy poster đối với groups
        public async Task FillPosterInfoForReelGroupFromFeedAsync( IElementHandle feed,PostInfoRawVM info)
        {
            if (feed == null || info == null)
                return;

            try
            {
                // ==================================================
                // 1️⃣ SELECTOR CHÍNH
                // ==================================================
                Libary.Instance.LogDebug(
                    $"{Libary.IconInfo} [ReelGroup-Feed] Try primary poster selector");

                var el = await feed.QuerySelectorAsync("span[class='xjp7ctv'] > a");

                if (el != null)
                {
                    info.PosterName = (await el.InnerTextAsync())?.Trim() ?? info.PosterName;

                    var href = await el.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(href))
                        info.PosterLink = ProcessingDAO.Instance.ShortenPosterLink(href);

                    Libary.Instance.LogDebug(
                        $"{Libary.IconOK} [ReelGroup-Feed] Poster(primary): {info.PosterName}");

                    return;
                }

                // ==================================================
                // 2️⃣ FALLBACK: anchor profile reel
                // ==================================================
                Libary.Instance.LogDebug(
                    $"{Libary.IconInfo} [ReelGroup-Feed] Primary not found, try anchor fallback");

                var aFallback = await feed.QuerySelectorAsync(
                    "a[class='x1i10hfl xjbqb8w x1ejq31n x18oe1m7 x1sy0etr xstzfhl x972fbf " +
                    "x10w94by x1qhh985 x14e42zd x9f619 x1ypdohk xt0psk2 x3ct3a4 " +
                    "xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 " +
                    "x1c1uobl x16tdsg8 x1hl2dhg xggy1nq x1a2a7pz x1heor9g " +
                    "xkrqix3 x1sur9pj x1s688f']");

                if (aFallback != null)
                {
                    info.PosterName = (await aFallback.InnerTextAsync())?.Trim() ?? info.PosterName;

                    var href = await aFallback.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(href)) info.PosterLink = ProcessingDAO.Instance.ShortenPosterLink(href);

                    Libary.Instance.LogDebug($"{Libary.IconOK} [ReelGroup-Feed] Poster(fallback-a): {info.PosterName}");

                    return;
                }
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [ReelGroup-Feed] Poster not found in feed");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [ReelGroup-Feed] Exception: {ex.Message}");
            }
        }
        //-- hàm tổng share reel
        public async Task OpenReelShareAndInitVMAsync(IPage mainPage,PostInfoRawVM info)
        {
            if (info == null || !ProcessingHelper.IsValidContent(info.PostLink))
            {
                Libary.Instance.LogDebug($"❌ [REEL] Skip open invalid link: {info?.PostLink}");
                return;
            }
            IPage reelPage = null;
            IElementHandle feed = null;
            bool IsGroups = false;
            try
            {
                Libary.Instance.LogDebug( $"🌐 [OpenReelShare] Open reel tab: {info.PostLink}");

                // ==================================================
                // 1️⃣ MỞ POPUP REEL
                // ==================================================
                reelPage = await ads.Instance.OpenNewTabSimpleAsync(mainPage, info.PostLink);

                if (reelPage == null)
                {
                    Libary.Instance.LogDebug("[OpenReelShare] STEP 1 Error – không mở được Page");
                    return;
                }
                else Libary.Instance.LogDebug("[OpenReelShare] STEP 1 OK – Mở Page mới thành công");
                // ==================================================
                // 2️⃣ LẤY FEED (ƯU TIÊN)
                // ==================================================
                feed = await GetFirstReelShareFeedAsync(reelPage);
                await CrawlBaseDAO.Instance.BypassSensitiveReelAsync(reelPage, feed);
                if (feed == null)
                {
                    Libary.Instance.LogDebug("[OpenReelShare] STEP 2 Error – Lấy Feed thất bại");
                }
                else
                {
                    Libary.Instance.LogDebug("[OpenReelShare] STEP 2 OK – Lấy Feed thành công");                                  
                }
                bool ok = false;

                if (feed != null)
                {
                    ok = await GetContainerReelShareAsync(feed, info);
                    Libary.Instance.LogDebug("[OpenReelShare] STEP 3 - Lấy Page chứa bằng Feed");
                }
                // fallback: lấy trực tiếp từ page
                if (!ok)
                {
                    await GetContainerReelShareAsync(reelPage, info);
                    Libary.Instance.LogDebug("[OpenReelShare] STEP 3 - Lấy Page chứa bằng Page");
                }
                Libary.Instance.LogDebug(
         $"[OpenReelShare] STEP 3 DONE | " +       
         $"PageName={info.PageName} | " +
         $"PageLink={info.PageLink}");

                // ==================================================
                // 3️⃣ LẤY CONTENT REEL
                // ==================================================
                var body = await reelPage.QuerySelectorAsync("body");
                if (body != null)
                {
                    info.Content = await GetReelTextAsync(reelPage, body);
                    if(ProcessingHelper.IsValidContent(info.Content)) 
                        Libary.Instance.LogDebug($"{Libary.IconOK} Step 4 - lấy nội dung thành công {info.Content.Length.ToString()}");
                }
                // ==================================================
                // 4️⃣ LẤY INTERACTIONS
                // ==================================================               
                (info.LikeCount, info.CommentCount, info.ShareCount)
                    = await ExtractReelInteractionsAsync(reelPage);
                Libary.Instance.LogDebug("[OpenReelShare] STEP 5 DONE – Interactions extracted");
                // ==================================================
                // 5️⃣ LẤY POSTER (CHỈ NAME + LINK, CHƯA CHECK TYPE)
                // ==================================================
                IsGroups =ProcessingHelper.IsValidContent(info.PageLink) && info.PageLink.Contains("/groups/");
                if (IsGroups && feed != null)
                {
                    info.ContainerType = FBType.GroupOn;
                    // BẮT BUỘC lấy ở đây
                    await FillPosterInfoForReelGroupFromFeedAsync(feed, info);
                }
                else
                {
                    info.ContainerType = FBType.Fanpage;
                    info.PosterName = info.PageName;
                    info.PosterLink = info.PageLink;
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [OpenReelShare] Exception: {ex.Message}");
            }
            finally
            {
                // ==================================================
                // 6️⃣ ĐÓNG POPUP TRƯỚC
                // ==================================================
                if (reelPage != null)
                {
                    try { await reelPage.CloseAsync(); } catch { }
                }
            }
            Libary.Instance.LogDebug(
            $"[OpenReelShare] BEFORE CHECKTYPE | " +
            $"mainPageNull={(mainPage == null)} | " +
            $"mainPageClosed={(mainPage?.IsClosed ?? true)}");

            // ==================================================
            // 7️⃣ CHECK TYPE – SAU KHI POPUP ĐÃ ĐÓNG
            // ==================================================
            if (mainPage == null || mainPage.IsClosed)
            {
                Libary.Instance.LogTech("[OpenReelShare] Skip CheckType – mainPage null/closed");
                return;
            }
            if (IsGroups)
            {
                info.ContainerType = FBType.GroupOn;
                // =========================
                // 1️⃣ CHECK CONTAINER (GROUP)
                // =========================
                if (ProcessingHelper.IsValidContent(info.PageLink))
                {
                    var (containerType, containerId) =await CrawlBaseDAO.Instance.CheckTypeCachedAsync(mainPage, info.PageLink);
                    info.ContainerType = containerType;
                    info.ContainerIdFB = containerId;
                    Libary.Instance.LogTech($"[OpenReelShare] GROUP Container | Type={containerType} | IDFB={containerId}");
                }
                // =========================
                // 2️⃣ CHECK POSTER
                // =========================
                if (ProcessingHelper.IsValidContent(info.PosterLink))
                {
                    var (posterType, posterId) =await CrawlBaseDAO.Instance.CheckTypeCachedAsync(mainPage, info.PosterLink);
                    info.PosterNote = posterType;
                    info.PosterIdFB = posterId;
                    Libary.Instance.LogTech($"[OpenReelShare] GROUP Poster | Type={posterType} | IDFB={posterId}");
                }
                // đảm bảo container type đúng là group
                if (info.ContainerType == FBType.Unknown) info.ContainerType = FBType.GroupOn;
            }
            else
            {
               
                if (ProcessingHelper.IsValidContent(info.PageLink))
                {
                    var (type, idfb) =await CrawlBaseDAO.Instance.CheckTypeCachedAsync(mainPage, info.PageLink);
                    Libary.Instance.LogTech($"[OpenReelShare] NON-GROUP | Type={type} | IDFB={idfb}");
                    // nếu là PERSON → không có page
                    if (IsPersonType(type))
                    {
                        info.PageName = null;     // 🔥 FIX
                        info.PageLink = null;     // 🔥 FIX
                        info.PosterNote = type;
                        info.PosterIdFB = idfb;
                        info.ContainerType = type;
                    }
                    else
                    {
                        info.ContainerType = FBType.Fanpage;
                        // container == poster
                        info.PosterNote = info.ContainerType = type;
                        info.PosterIdFB = idfb = info.ContainerIdFB = idfb;
                    }                      
                }
            }
        }
        static bool IsPersonType(FBType t) =>
    t == FBType.Person ||
    t == FBType.PersonKOL ||
    t == FBType.PersonHidden;
    }

}
