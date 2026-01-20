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
        public async Task<(string OriginalPageName, string OriginalPageLink)>ExtractPageGroupsReel(IPage page,IElementHandle postNode)
        {
            string pageName = "N/A";
            string pageLink = "N/A";

            Libary.Instance.LogDebug(" ▶ Start Reel");
            try
            {
                // =================================================
                // 1️⃣ ƯU TIÊN: a[aria-label='Xem Nhóm']
                // =================================================
                var aGroup = await postNode.QuerySelectorAsync("a[aria-label='Xem Nhóm'][href]");
                if (aGroup != null)
                {
                    pageName = (await aGroup.InnerTextAsync())?.Trim();
                    string href = await aGroup.GetAttributeAsync("href");

                    if (!string.IsNullOrWhiteSpace(href)) pageLink = ProcessingHelper.ShortLinkPage(href);
                    Libary.Instance.LogDebug( $"[ExtractPageContainerReel] ✅ Found by aria-label | Name='{pageName}', Link='{pageLink}'");
                    return (pageName ?? "N/A", pageLink ?? "N/A");
                }
                Libary.Instance.LogDebug("[ExtractPageContainerReel] aria-label='Xem Nhóm' not found → fallback");

                // =================================================
                // 2️⃣ FALLBACK: span[class='x1lliihq x6ikm8r x10wlt62 x1n2onr6']
                // =================================================
                var span = await postNode.QuerySelectorAsync( "span[class='x1lliihq x6ikm8r x10wlt62 x1n2onr6']");

                if (span == null)
                {
                    Libary.Instance.LogDebug("[ExtractPageContainerReel] ❌ Fallback span not found");
                    return (pageName, pageLink);
                }
                // Page name
                pageName = (await span.InnerTextAsync())?.Trim();

                // Page link
                var a = await span.QuerySelectorAsync("a[href]");
                if (a != null)
                {
                    string href = await a.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(href)) pageLink = ProcessingHelper.ShortLinkPage(href);
                }
                Libary.Instance.LogDebug( $" ✅ Found by span | Name='{pageName}', Link='{pageLink}'");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug( $" ❌ Exception: {ex.Message}");
            }

            return (pageName ?? "N/A", pageLink ?? "N/A");
        }
        // 1 lấy postername, posterlink áp dụng groups
        public async Task<(string PosterName, string PosterLink)> ExtractPosterGroupsReel(IElementHandle postNode)
        {
            string posterName = "N/A";
            string posterLink = "N/A";

            Libary.Instance.LogDebug("[ExtractPosterFromHtmlSpan] ▶ Start");

            try
            {
                var span = await postNode.QuerySelectorAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x65f84u']");

                if (span == null)
                {
                    if (span == null)
                    {
                        Libary.Instance.LogDebug(
                            "[ExtractPosterFromHtmlSpan] ❌ Span not found → fallback a[aria-label]");

                        try
                        {
                            var aa = await postNode.QuerySelectorAsync(
                                "a[aria-label='Xem trang cá nhân của chủ sở hữu']");

                            if (aa == null)
                            {
                                Libary.Instance.LogDebug(
                                    "[ExtractPosterFromHtmlSpan] ❌ Fallback <a> not found");
                                return (posterName, posterLink);
                            }

                            // ========= NAME =========
                            posterName = (await aa.InnerTextAsync())?.Trim();

                            // ========= LINK =========
                            var rawHref = (await aa.GetAttributeAsync("href"))?.Trim();
                            posterLink = ProcessingHelper.NormalizeReelPosterProfile(rawHref);

                            Libary.Instance.LogTech(
                                $"[ExtractPosterFromHtmlSpan] ✅ Fallback OK | Name='{posterName}' | Link={posterLink}");

                            return (posterName, posterLink);
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.LogDebug(
                                $"[ExtractPosterFromHtmlSpan] ❌ Fallback exception: {ex.Message}");
                            return (posterName, posterLink);
                        }
                    }

                }

                // ========================
                // POSTER NAME
                // ========================
                posterName = (await span.InnerTextAsync())?.Trim();

                // ========================
                // POSTER LINK
                // ========================
                var a = await span.QuerySelectorAsync("a[href]");
                if (a != null)
                {
                    string href = await a.GetAttributeAsync("href");
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        posterLink = ProcessingHelper.ShortenPosterLink(href);
                    }
                }

                Libary.Instance.LogDebug(
                    $"[ExtractPosterFromHtmlSpan] ✅ Name='{posterName}', Link='{posterLink}'");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"[ExtractPosterFromHtmlSpan] ❌ Exception: {ex.Message}");
            }

            return (posterName ?? "N/A", posterLink ?? "N/A");
        }
        //HÀM MỞ LINK ĐỂ LẤY CONTENT VÀ TƯƠNG TÁC
        public async Task<(string Content,int Like,int Comment, int Share)> OpenReelTabAndExtractAsync( IPage mainPage,string reelLink)
        {
            string content = "N/A";    int like = 0, comment = 0, share = 0;
            Libary.Instance.LogDebug($"{Libary.IconInfo} 🌐 [ReelTab] Open reel tab: {reelLink}");
            IPage newPage = null;
            try
            {
                // ========================
                // 1️⃣ OPEN TAB
                // ========================
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync($"window.open('{reelLink}', '_blank');");

                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));
                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug( $"{Libary.IconFail} ❌ [ReelTab] Timeout mở tab reel");
                    return (content, like, comment, share);
                }
                newPage = await popupTask;
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(500);
                // ========================
                // 2️⃣ GET CONTENT
                // ========================
                Libary.Instance.LogDebug($"{Libary.IconInfo} ✍️ [ReelTab] Lấy caption reel");
                content = await GetReelTextAsync( newPage,await newPage.QuerySelectorAsync("body"));
                // ========================
                // 3 GET INTERACTIONS
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
                    try
                    {
                        await newPage.CloseAsync();
                    }
                    catch { }
                }
            }
            return (content ?? "N/A", like, comment, share);
        }
        // HÀM BỔ TRỢ
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

                PostType = PostType.page_Real_Cap.ToString()
            };
            // Lấy PageName (Page Chứa nếu có)
            var (pagename, pagelink) = await ExtractPageGroupsReel(mainPage, post);
            if (!string.IsNullOrEmpty(pagename))
            {
                reel.PageName = pagename;
                reel.PageLink = pagelink;
            }
            try
            {
                var posterReel = await post.QuerySelectorAsync("a[class*='x1i10hfl xjbqb8w x1ejq31n x18oe1m7 x1sy0etr xstzfhl x972fbf x10w94by x1qhh985 x14e42zd x9f619 x1ypdohk xt0psk2 x3ct3a4 xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x16tdsg8 x1hl2dhg xggy1nq x1a2a7pz x1heor9g xkrqix3 x1sur9pj x1s688f']");
                if (posterReel != null)
                {
                    // Lấy link người đăng
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
                if (!string.IsNullOrWhiteSpace(reel.PosterLink) && reel.PosterLink != "N/A")
                    Libary.Instance.LogDebug($"{Libary.IconOK} PosterRell OK: {reel.PosterLink}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} PosterRell k lấy được link: {ex.Message}");
            }
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
                    reel.PostLink = ProcessingHelper.NormalizeReelLink(reelLink);
                }
                IElementHandle TimeTag = await post.QuerySelectorAsync("span[class= 'html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x4k7w5x x1h91t0o x1h9r5lt x1jfb8zj xv2umb2 x1beo9mf xaigb6o x12ejxvf x3igimt xarpa2k xedcshv x1lytzrv x1t2pt76 x7ja8zs x1qrby5j']");
                if (TimeTag != null)
                {
                    string time = (await TimeTag.InnerTextAsync() ?? "").Trim();
                    if (ProcessingDAO.Instance.IsTime(time))
                    {
                        postTime = TimeHelper.CleanTimeString(time);
                        Libary.Instance.LogDebug($"{Libary.IconOK} 🕒 Found PostTime reel: {postTime}");
                        reel.PostTime = postTime;
                        reel.RealPostTime = TimeHelper.ParseFacebookTime(reel.PostTime);
                    }
                }
                if (reelLink == "N/A")
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail}❌ [ReelExtract] Không có /reel/ để mở tab → return basic info");
                    return reel;
                }
                if (posterLink != "N/A") reel.PosterLink = ProcessingDAO.Instance.ShortenPosterLinkReel(posterLink);
                if (reel.PosterLink != "N/A")
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} dùng CheckType lấy type người đăng Reel");
                    var fbTypeReel = await CrawlBaseDAO.Instance.CheckTypeCachedAsync(mainPage, reel.PosterLink);
                    reel.PosterNote = fbTypeReel.ToString();
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
                var Divpostername = await newPage.QuerySelectorAsync("span[class='xjp7ctv']>a");
                if (Divpostername != null)
                {
                    posterName = (await Divpostername.InnerTextAsync()).Trim();
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} Không lấy được PosterName");
                }
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
            if (info == null || string.IsNullOrWhiteSpace(info.PostLink))
                return;

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
                    // BẮT BUỘC lấy ở đây
                    await FillPosterInfoForReelGroupFromFeedAsync(feed, info);
                }
                else
                {
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
                        info.PageName = "N/A";
                        info.PageLink = "N/A";
                        info.PosterNote = type;
                        info.PosterIdFB = idfb;
                    }
                    else
                    {
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
