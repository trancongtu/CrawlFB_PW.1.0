using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using Microsoft.Playwright;
using DevExpress.DocumentView;
using IPage = Microsoft.Playwright.IPage;
using DocumentFormat.OpenXml.Office2019.Drawing.Animation.Model3D;
using DocumentFormat.OpenXml.Wordprocessing;
using DevExpress.Data.ExpressionEditor;
using System.Reflection;
using DocumentFormat.OpenXml.Vml.Office;
using CrawlFB_PW._1._0;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using DevExpress.Data.Filtering.Helpers;
using FBType = CrawlFB_PW._1._0.Enums.FBType;
using CrawlFB_PW._1._0.Helper;
using PostTypeEnum = CrawlFB_PW._1._0.Enums.PostType;
using CrawlFB_PW._1._0.ViewModels;
namespace CrawlFB_PW._1._0.DAO
{
    internal class PageDAO
    {
        // 🔹 Biến static giữ instance duy nhất
        private static PageDAO _instance;

        // 🔹 Lock để đảm bảo thread-safe nếu có đa luồng
        private static readonly object _lock = new object();
        private static readonly Random _random = new Random();      
        private Dictionary<string, FBType> CheckedTypeCache = new Dictionary<string, FBType>(StringComparer.OrdinalIgnoreCase);

        // 🔹 Constructor private để không ai tạo instance bên ngoài được
        private PageDAO() { }

        // 🔹 Property công khai để lấy instance (lazy load)
        public static PageDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new PageDAO();
                    }
                }
                return _instance;
            }
        }
        /// <summary>
        /// HÀM DÙNG CHUNG
        ///     // hàm xác định feed của groups hay fanpage
        public async Task<IElementHandle> GetFeedContainerAsync(IPage page)
        {
            IElementHandle feed = null;

            try
            {
                // 🧠 Nếu là group
                if (page.Url.Contains("/groups/"))
                {
                    feed = await page.QuerySelectorAsync("div[role='feed']")
                        ?? await page.QuerySelectorAsync("div[data-pagelet*='GroupFeed']");
                }
                else
                {
                    // 🧠 Nếu là fanpage hoặc trang cá nhân
                    feed = await page.QuerySelectorAsync("div[data-pagelet*='ProfileTimeline']")
                        ?? await page.QuerySelectorAsync("div[data-pagelet*='PageFeed']")
                        ?? await page.QuerySelectorAsync("div[role='feed']")
                        ?? await page.QuerySelectorAsync("div[role='main']");
                }

                // 🧩 Fallback thêm nếu vẫn null
                if (feed == null)
                {
                    feed = await page.QuerySelectorAsync("div[data-pagelet*='FeedContainer']")
                        ?? await page.QuerySelectorAsync("div[data-pagelet*='MainFeed']");

                    Console.WriteLine("⚠️ Fallback feed selector được kích hoạt");
                }

                if (feed == null)
                {
                    Console.WriteLine("❌ Không tìm thấy feed chính (DOM có thể đã thay đổi).");
                }
                else
                {
                    Console.WriteLine($"✅ Feed container xác định thành công: {page.Url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi lấy feed: {ex.Message}");
            }

            return feed;
        }
        // LẤY TÊN
        public async Task<string> GetPageNameAsync(IPage page)
        {
            try
            {
                // 🔹 Chờ load DOM ổn định
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await PageDAO.Instance.RandomDelayAsync(page, 400, 800);

                string currentUrl = page.Url?.ToLower() ?? "";
                bool isGroup = currentUrl.Contains("/groups/");
                bool isPage = currentUrl.Contains("/pages/") || !isGroup; // mặc định: nếu không phải group => page/profile

                IElementHandle nameSpan = null;

                if (isGroup)
                {
                    // 🟩 Group name
                    nameSpan = await page.QuerySelectorAsync(
                        "span[class='x193iq5w xeuugli x13faqbe x1vvkbs xlh3980 xvmahel " +
                        "x1n0sxbx x1lliihq x1s928wv xhkezso x1gmr53x x1cpjm7i x1fgarty " +
                        "x1943h6x xtoi2st xw06pyt x1q74xe4 xyesn5m x1xlr1w8 xzsf02u x1yc453h']"
                    );
                }
                else if (isPage)
                {
                    // 🟦 Fanpage hoặc profile public
                    nameSpan = await page.QuerySelectorAsync("div[class = 'x1e56ztr x1xmf6yo']>span");

                    // Fallback 1: fanpage có thể load chậm → retry
                    if (nameSpan == null)
                    {
                        await PageDAO.Instance.RandomDelayAsync(page, 500, 1000);
                        nameSpan = await page.QuerySelectorAsync("h1[class = 'html-h1 xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1vvkbs x1heor9g x1qlqyl8 x1pd3egz x1a2a7pz']");
                    }
                }
                // Không tìm thấy thẻ nào
                if (nameSpan == null)
                    return "N/A";
                string pageName = (await nameSpan.InnerTextAsync())?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(pageName))
                    pageName = "N/A";
                Libary.Instance.LogTech($"{Libary.IconOK}PageName thành công: " + pageName);
                return pageName;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"❌ [GetPageNameAsync] Lỗi lấy PageName: {ex.Message}");
                return "N/A";
            }
        }

        //// lấy like share => HIỆN TẠI ĐANG DÙNG
        public async Task<(int likes, int comments, int shares)> ExtractPostInteractionsAsync(IElementHandle post)
        {
            int likes = 0, comments = 0, shares = 0;
            try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo} -- BẮT ĐẦU LẤY TƯƠNG TÁC------");
                // 💬 SHARE + COMMENT (layout có chữ)
                var textSpans = await post.QuerySelectorAllAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs xkrqix3 x1sur9pj']"
                );
                foreach (var span in textSpans)
                {
                    string text = (await span.InnerTextAsync())?.Trim().ToLower() ?? "";
                    if (string.IsNullOrEmpty(text)) continue;

                    if (text.Contains("bình luận") || text.Contains("comment"))
                        comments = ProcessingHelper.ParseFacebookNumber(text);
                    else if (text.Contains("chia sẻ") || text.Contains("share"))
                        shares = ProcessingHelper.ParseFacebookNumber(text);
                }
                // 👍 LIKE
                var likeSpan = await post.QuerySelectorAsync("span[class='x135b78x']");
                if (likeSpan == null)
                {
                    likeSpan = await post.QuerySelectorAsync("span[class='xt0b8zv x135b78x']");
                }
                if (likeSpan == null)
                {
                    likeSpan = await post.QuerySelectorAsync("span[class*='x135b78x']");
                }
                if (likeSpan != null)
                {
                    string likeText = (await likeSpan.InnerTextAsync())?.Trim() ?? "";
                    likes = ProcessingHelper.ParseFacebookNumber(likeText);
                }
                Libary.Instance.LogDebug($"{Libary.IconOK} ✅ [ExtractPostInteractionsAsync] Like={likes}, Comment={comments}, Share={shares}");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} ❌ [ExtractPostInteractionsAsync] Lỗi: {ex.Message}");
            }

            return (likes, comments, shares);
        }
        // hàm lấy time and link
        public async Task<(List<string> timeList, List<string> linkList)> ExtractTimeAndLinksAsync(IEnumerable<IElementHandle> postinfor)
        {
            var timeList = new List<string>();
            var linkList = new List<string>();
            var addedLinks = new HashSet<string>();

            int index = 0;
            Libary.Instance.LogDebug(Libary.StartPost($" PHÂN TÍCH POST"));
            Libary.Instance.LogDebug("Bắt đầu trích xuất time & link");
            foreach (var info in postinfor)
            {
                index++;
                try
                {
                    string textContent = (await info.InnerTextAsync())?.Trim() ?? "";

                    if (string.IsNullOrEmpty(textContent))
                    {
                        Libary.Instance.LogDebug($"[{index}] Bỏ qua: text rỗng");
                        continue;
                    }

                    // kiểm tra có phải text thời gian không
                    if (Regex.IsMatch(
                        textContent,
                        @"(\d+\s*(giờ|phút|ngày|hôm qua|tháng))",
                        RegexOptions.IgnoreCase))
                    {
                        Libary.Instance.LogDebug($"[{index}] Phát hiện text thời gian");

                        var anchors = await info.QuerySelectorAllAsync(
                            "a[class*='x1i10hfl'], a[href*='posts'], a[href*='videos']"
                        );

                        Libary.Instance.LogDebug($"[{index}] Số link tìm được: {anchors.Count}");

                        foreach (var a in anchors)
                        {
                            string href = await a.GetAttributeAsync("href");

                            if (ProcessingHelper.IsValidPostPath(href) && addedLinks.Add(href))
                            {
                                timeList.Add(textContent);
                                linkList.Add(href);
                            }
                        }
                    }
                    else
                    {
                        Libary.Instance.LogDebug($"[{index}] Không phải text thời gian");
                    }
                }
                catch (Exception ex)
                {
                    // ❗ lỗi DOM cục bộ → log debug, KHÔNG throw
                    Libary.Instance.LogDebug($"{Libary.IconFail} [{index}] Lỗi khi xử lý postinfor: {ex.Message}");
                }
            }

            Libary.Instance.LogDebug($"✔ Hoàn tất extract | " +
                                    $"Time={Libary.CountIcon(timeList.Count)}({timeList.Count}) | " +
                                    $"Link={Libary.CountIcon(linkList.Count)}({linkList.Count})");
            return (timeList, linkList);
        }

        // hàm dưới lấy thông tin người đăng theo timelink, postlink ban đầu
        public async Task<(string postTime, string originalPostTime, string postLink, string originalPostLink)>
        PostTypeDetectorAsync(List<string> timeList, List<string> linkList, IEnumerable<IElementHandle> postinfor = null)
        {
            string postTime = "N/A";
            string originalPostTime = "N/A";
            string postLink = "N/A";
            string originalPostLink = "N/A";
            try
            {
                int timeCount = timeList.Count;
                int linkCount = linkList.Count;

                // ===== CASE 1: BÀI TỰ ĐĂNG =====
                if (timeCount == 1 && linkCount >= 1)
                {
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);

                    Libary.Instance.LogDebug($"{Libary.IconInfo} HÀM LẤY TIME&LINK -> Phát hiện bài tự đăng");
                }
                // ===== CASE 2: BÀI SHARE =====
                else if (timeCount == 2 && linkCount >= 2)
                {
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    originalPostTime = TimeHelper.CleanTimeString(timeList[1]);
                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);
                    originalPostLink = ProcessingHelper.ShortenFacebookPostLink(linkList[1]);

                    Libary.Instance.LogDebug($"{Libary.IconInfo}  HÀM LẤY TIME&LINK -> Phát hiện bài Share");
                }
                // ===== CASE 0: VIDEO / REEL =====
                else if (timeCount == 0 && linkCount == 0)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Không có time/link → nghi bài video");
                    if (postinfor != null)
                    {
                        var (vtime, vlink) = await HandleVideoPostAsync(postinfor);
                        postTime = vtime;
                        postLink = vlink;
                        Libary.Instance.LogDebug(
                            vlink != "N/A"
                                ? $"{Libary.IconOK}  Lấy được Link Video"
                                : $"{Libary.IconFail}  Không Lấy được Link Video"
                        );
                    }
                }
                else
                {

                    Libary.Instance.LogDebug($"{Libary.IconInfo}  Không khớp pattern post nào");
                }
            }
            catch (Exception ex)
            {

                // ❗ lỗi kỹ thuật → debug, KHÔNG throw
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} PostTypeDetector ERROR: {ex.Message}"
                );
            }
            Libary.Instance.LogDebug(
                $"${Libary.IconInfo} [ PostTypeDetectorAsync]Kết quả | " +
                $"PostTime={Libary.BoolIcon(postTime != "N/A")}, " +
                $"PostLink={Libary.BoolIcon(postLink != "N/A")}, " +
                $"OriginalLink={Libary.BoolIcon(originalPostLink != "N/A")}"
            );
            return (postTime, originalPostTime, postLink, originalPostLink);
        }
        //--------------hàm lấy thông tin người đăng theo selector------------------
        public async Task<(string name, string link)> GetPosterInfoBySelectorsAsync(IElementHandle container)
        {
            string name = "N/A";
            string link = "N/A";
            try
            {
                // ===== Selector chính =====
                Libary.Instance.LogDebug($"{Libary.IconInfo} Thử selector chính lấy người đăng");
                var el = await container.QuerySelectorAsync("span[class='xjp7ctv'] > a");
                if (el != null)
                {
                    name = (await el.InnerTextAsync())?.Trim() ?? "N/A";
                    link = await el.GetAttributeAsync("href") ?? "N/A";
                    if (link != "N/A")
                        link = ProcessingDAO.Instance.ShortenPosterLink(link);
                    Libary.Instance.LogDebug($"{Libary.IconOK} Lấy người đăng bằng selector chính Name: {name}");
                    return (name, link);
                }
                // ===== Selector dự phòng =====
                Libary.Instance.LogDebug($"{Libary.IconInfo}Selector chính không có, thử selector dự phòng");
                var el2 = await container.QuerySelectorAsync("span[class='xjp7ctv'] > span > span > a");
                if (el2 != null)
                {
                    name = (await el2.InnerTextAsync())?.Trim() ?? "N/A";
                    link = await el2.GetAttributeAsync("href") ?? "N/A";

                    if (link != "N/A")
                        link = ProcessingDAO.Instance.ShortenPosterLink(link);
                    Libary.Instance.LogDebug($"{Libary.IconOK} Lấy người đăng bằng selector dự phòng Name: {name}");
                    return (name, link);
                }
                // ===== Không tìm được =====
                Libary.Instance.LogDebug($"{Libary.IconFail} Không tìm thấy thẻ người đăng với cả 2 selector");
            }
            catch (Exception ex)
            {
                // ❗ lỗi DOM cục bộ → chỉ debug
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} Lỗi khi lấy thông tin người đăng: {ex.Message}"
                );
            }
            // fallback an toàn
            return (name, link);
        }
        // nếu k lấy được nội dung---thay thế div hàm dưới => HÀM HIỆN TẠI ĐANG DÙNG
        public async Task<string> GetContentTextAsync(IPage page, IElementHandle container)
        {
            var contentEls = await container.QuerySelectorAllAsync(
                "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
            );

            if (contentEls.Count == 0)
            {
                Libary.Instance.LogDebug("⚠️ Không tìm thấy nội dung bài đăng (GetContentTextAsync)");
                return "N/A";   // ⬅️ quan trọng
            }

            Libary.Instance.LogDebug("✅ Tìm thấy thẻ nội dung");

            return await GetFullContentAsync(page, container);
        }

        // 🧩 Hàm mở “Xem thêm” và lấy nội dung bài đăng chính => HÀM HIỆN TẠI ĐANG DÙNG
        private async Task<string> GetFullContentAsync(IPage page, IElementHandle container)
        {
            if (container == null)
                return "N/A";

            var sb = new StringBuilder();

            try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo}[FULL] START GetFullContentAsync");
                // Chỉ tìm Xem thêm TRONG container để tránh click nhầm bài khác
                var seeMoreBtn = await container.QuerySelectorAsync(
                    "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                );

                if (seeMoreBtn != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Tìm thấy 'Xem thêm' tròn bài viết");
                    // Kiểm tra kích thước nút có bị che không
                    var bbox = await seeMoreBtn.BoundingBoxAsync();
                    if (bbox != null && bbox.Width > 5 && bbox.Height > 5)
                    {
                        // Scroll đúng vào node – không dùng scroll random
                        await page.EvaluateAsync(
                            "el => el.scrollIntoView({behavior:'instant', block:'center'})",
                            seeMoreBtn
                        );

                        await page.WaitForTimeoutAsync(120);

                        try
                        {
                            Libary.Instance.LogDebug($"{Libary.IconInfo} Clicking 'Xem thêm' safely");

                            await seeMoreBtn.ClickAsync(new ElementHandleClickOptions
                            {
                                Timeout = 2000,
                                Trial = false
                            });

                            await page.WaitForTimeoutAsync(200);
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.LogDebug($"{Libary.IconInfo} SAFE fallback JS click: " + ex.Message);

                            // fallback JS click (cực kỳ an toàn)
                            await page.EvaluateAsync(
                                @"el => { try { el.click(); } catch (e) {} }",
                                seeMoreBtn
                            );

                            await page.WaitForTimeoutAsync(150);
                        }
                    }
                    else
                    {
                        Libary.Instance.LogDebug($"{Libary.IconInfo} 'Xem thêm' bị che / nhỏ → Bỏ qua click");
                    }
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Không có 'Xem thêm' trong post node");
                }
                // Lấy nội dung sau khi expand
                var spans = await container.QuerySelectorAllAsync("div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']");
                foreach (var span in spans)
                {
                    var text = (await span.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} ERROR GetFullContentAsync: {ex.Message}"
                );
            }

            string result = sb.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(result))
            {
                Libary.Instance.LogDebug($"{Libary.IconOK} Đã lấy toàn bộ nội dung sau 'Xem thêm'"
                );
            }
            else
            {
                Libary.Instance.LogDebug($"{Libary.IconWarn}  Nội dung rỗng sau 'Xem thêm'");
            }
            return string.IsNullOrWhiteSpace(result) ? "N/A" : result;
        }

        // HÀM LẤY BACKGROUND => HIỆN TẠI ĐANG DÙNG
        public async Task<string> BackgroundTextAllAsync(IPage page, IElementHandle post)
        {
            string text = "";
            // 1️⃣ Lấy caption trong div nền màu
            try
            {
                // cơ bản lấy được hết text
                var storyDiv = await post.QuerySelectorAsync("div[data-ad-rendering-role='story_message']");
                if (storyDiv != null)
                {
                    // 🔍 tìm story_message (caption kiểu bài share, bài thường)

                    if (storyDiv == null)
                        return "N/A";

                    // 🔍 Tìm nút 'Xem thêm' trong story_message
                    var seeMore = await storyDiv.QuerySelectorAsync(
                        "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                    );

                    if (seeMore != null)
                    {
                        Libary.Instance.CreateLog($"{Libary.IconInfo} TÌM THẤY 'Xem thêm' → click");

                        // Scroll vào giữa cho an toàn
                        await page.EvaluateAsync("el => el.scrollIntoView({block:'center'})", seeMore);
                        await page.WaitForTimeoutAsync(120);

                        try
                        {
                            await seeMore.ClickAsync(new ElementHandleClickOptions()
                            {
                                Timeout = 1500,
                            });
                        }
                        catch (Exception ex)
                        {
                            // fallback JS click
                            Libary.Instance.CreateLog($"{Libary.IconFail} ⚠ Click thường lỗi → fallback JS click: " + ex.Message);
                            await page.EvaluateAsync("(el)=>{ try{ el.click(); } catch{} }", seeMore);
                        }

                        await page.WaitForTimeoutAsync(200);
                    }

                    // 🔥 Lấy toàn bộ nội dung sau khi mở rộng
                    text = await storyDiv.InnerTextAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text.Trim();

                    return "N/A";

                }
                else
                {

                    var bgBlocks = await post.QuerySelectorAllAsync("div[class='x1yx25j4 x13crsa5 x1rxj1xn x162tt16 x5zjp28'] > div");
                    if (bgBlocks != null)
                    {
                        foreach (var e in bgBlocks)
                        {
                            text = (await e.InnerTextAsync())?.Trim();

                        }
                    }
                    else
                    {
                        var bgImages = await post.QuerySelectorAllAsync("img[alt][class*='x15mokao']");
                        foreach (var img in bgImages)
                        {
                            text = (await img.GetAttributeAsync("alt"))?.Trim();
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(text)) Libary.Instance.LogDebug($"{Libary.IconOK} Lấy được text BackGround ký tự: {text.Length}");
                else Libary.Instance.LogDebug($"{Libary.IconFail} Lỗi text BackGround ký tự: ");
            }
            catch { }
            return text.Trim();
        }
        // HÀM CHECKTYPE
        public async Task<FBType> CheckFBTypeAsync(IPage tab)
        {
            bool isGroup = false;
            bool hasFriends = false;
            bool hasFollow = false;
            bool isKOL = false;
            string url = tab.Url;
            Libary.Instance.LogDebug($"[CheckFBType] ▶ CHECK = {url}");
            var body = await tab.QuerySelectorAsync("body");
            string allText = (await body.InnerTextAsync() ?? "").ToLower();
            string[] hiddenMarks =
            {
            "bạn hiện không xem được nội dung này",
            "nội dung này hiện không khả dụng",
            "this content isn't available",
            "content isn't available",
            "content not found",
            "bài viết này không hiển thị",
            "trang này không khả dụng",
            "không thể xem nội dung",
            "không tồn tại"
            };

            if (hiddenMarks.Any(x => allText.Contains(x)))
            {
                Libary.Instance.LogDebug("[CheckFBType] ❌ PROFILE HIDDEN / CONTENT NOT AVAILABLE");
                return FBType.PersonHidden;
            }
            // ============================================================
            // 1. GROUP DETECT
            // ============================================================
            if (url.Contains("/groups/"))
                isGroup = true;
            var groupDiv = await tab.QuerySelectorAsync("div[class*='x193iq5w']");
            if (isGroup)
            {
                string all = (await body.InnerTextAsync() ?? "").ToLower();

                bool isPrivate = all.Contains("nhóm riêng tư") || all.Contains("riêng tư");
                bool isPublic = all.Contains("nhóm công khai") || all.Contains("công khai");
                if (isPrivate && !isPublic)
                {
                    Libary.Instance.LogDebug("[CheckFBType] 🔒 FINAL = GROUP-OFF");
                    return FBType.GroupOff;
                }
                else
                {
                    Libary.Instance.LogDebug("[CheckFBType] 🔓 FINAL = GROUP-ON");
                    return FBType.GroupOn;
                }
            }
            // ============================================================
            // 2. PERSON – LẤY TABLIST
            // ============================================================
            string[] tabSelectors =
            {
        "div[role='tablist'] a[role='tab']",
        "div[data-pagelet='ProfileTabs'] a",
        "div[role='navigation'] a"
             };
            List<IElementHandle> tabs = new List<IElementHandle>();
            foreach (string sel in tabSelectors)
            {
                var found = await tab.QuerySelectorAllAsync(sel);
                // Libary.Instance.CreateLog($"[DEBUG] Try selector '{sel}' → {found.Count} nodes");

                if (found.Count > 0)
                {
                    tabs = found.ToList();
                    break;
                }
            }
            // ============================================================
            // 3. Đọc TEXT tab bằng BEFORE / AFTER (DOM 2025)
            // ============================================================
            foreach (var el in tabs)
            {
                string text = await el.EvaluateAsync<string>(@"
            el => {
                const before = window.getComputedStyle(el, '::before').getPropertyValue('content');
                const after  = window.getComputedStyle(el, '::after').getPropertyValue('content');
                const own    = el.textContent || '';
                return (before + ' ' + own + ' ' + after)
                    .replace(/'/g, '')     // bỏ ký tự '
                    .replace(/""/g, '')    // bỏ ký tự ""
                    .trim();
            }");
                string href = await el.GetAttributeAsync("href") ?? "";
                Libary.Instance.LogDebug($"TAB: text='{text}' | href='{href}'");
                string lower = text.ToLower();
                if (lower.Contains("bạn bè") || lower.Contains("friends"))
                    hasFriends = true;
                if (href.Contains("/friends"))
                    hasFriends = true;
            }
            // ============================================================
            // 4. FOLLOW detection – FANPAGE / KOL
            var header = await tab.QuerySelectorAsync("div[data-pagelet='ProfileActions']");

            if (header != null)
            {
                var followBtn = await header.QuerySelectorAsync(
                    "div[role='button']:has-text('Theo dõi'), div[role='button']:has-text('Follow')"
                );

                if (followBtn != null)
                {
                    hasFollow = true;
                    Libary.Instance.LogDebug("[CheckFBType] FOLLOW BUTTON FOUND IN HEADER");
                }
            }

            // nhận diện KOL
            var kol = await tab.QuerySelectorAsync("span:has-text('Người sáng tạo nội dung số')");
            if (kol != null)
            {
                isKOL = true;
                Libary.Instance.LogDebug("[CheckFBType] 🟩 KOL DETECTED");
            }

            // ============================================================
            // 5. CHỐT KẾT QUẢ
            // ============================================================
            if (hasFriends && !hasFollow)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟩 FINAL = PERSON");
                return FBType.Person;
            }

            if ((hasFriends && hasFollow) || isKOL)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟧 FINAL = PERSON-KOL");
                return FBType.PersonKOL;
            }

            if (!hasFriends && hasFollow)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟨 FINAL = FANPAGE");
                return FBType.Fanpage;
            }

            Libary.Instance.LogDebug("[CheckFBType] ❓ FINAL = UNKNOWN");
            return FBType.Unknown;
        }

        public async Task<FBType> OpenLinkAndCheckTypeAsync(IPage mainPage, string url)
        {
            IPage newPage = null;

            try
            {
                Libary.Instance.LogDebug($"[OpenCheckType] 🔗 Mở link: {url}");

                // chờ tab popup
                var popupTask = mainPage.Context.WaitForPageAsync();

                // mở tab
                await mainPage.EvaluateAsync($"window.open('{url}', '_blank');");

                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));
                if (finished != popupTask)
                    return FBType.Unknown;

                newPage = await popupTask;

                // chờ load DOM
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(800);

                // scroll giả lập 2025
                await ProcessingDAO.Instance.HumanScrollAsync(newPage);

                await newPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await newPage.WaitForTimeoutAsync(300);

                // GỌI 1 LẦN – đúng theo flow của bạn
                var type = await CheckFBTypeAsync(newPage);
                Libary.Instance.LogDebug($"[OpenCheckType] 🏷 Type = {type}");

                return type;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [OpenCheckType] Lỗi: {ex.Message}");
                return FBType.Unknown;
            }
            finally
            {
                try { if (newPage != null) await newPage.CloseAsync(); } catch { }
            }
        }
        public async Task<FBType> CheckTypeCachedAsync(IPage mainPage, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || url == "N/A")
                return FBType.Unknown;

            if (CheckedTypeCache.TryGetValue(url, out FBType cachedType))
            {
                Libary.Instance.LogDebug($"[CheckTypeCached] 🔁 Cache hit: {url} → {cachedType}");
                return cachedType;
            }

            FBType result = await OpenLinkAndCheckTypeAsync(mainPage, url);

            CheckedTypeCache[url] = result;

            return result;
        }
        /// </summary>
        ///  // HÀM CHÍNH LẤY BÀI GỐC ĐANG DÙNG
        public async Task<PostPage> GetPostOriginal(IPage mainPage, string url, int timeoutSec = 3)
        {
            var post = new PostPage();
            IPage newPage = null;
            post.PostLink = url;
            bool success = false;
            try
            {
                Libary.Instance.LogDebug($"{Libary.IconStart} [ORIGINAL POST] Bắt đầu mở bài gốc");
                // ===== 1️⃣ Mở tab mới =====
                int beforeCount = mainPage.Context.Pages.Count;
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync($"window.open('{url}', '_blank');");
                Libary.Instance.LogDebug($"{Libary.IconInfo} Mở tab bài gốc");
                var finished = await Task.WhenAny(popupTask, Task.Delay(timeoutSec * 1000));
                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} Mở tab bài gốc bị timeout");
                    return null;
                }
                int afterCount = mainPage.Context.Pages.Count;
                if (afterCount <= beforeCount)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} Popup không tăng page count → abort");
                    await SafeClosePageAsync(newPage);
                    return null;
                }
                newPage = await popupTask;
                Libary.Instance.LogDebug($"{Libary.IconOK} Mở tab bài gốc thành công");
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(300);
                // scroll nhẹ
                await ProcessingDAO.Instance.HumanScrollAsync(newPage);
                await newPage.WaitForTimeoutAsync(200);

                // ===== 2️⃣ Tìm div bài post =====
                IElementHandle postDiv = null;

                string selector1 = "div[aria-labelledby='_R_imjbsmj5ilipam_']";
                postDiv = await newPage.QuerySelectorAsync(selector1);

                if (postDiv != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconOK} Tìm postDiv bằng selector1");
                }
                else
                {
                    string selector2 =
                        "div[class='html-div xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x78zum5 xdt5ytf x1iyjqo2 x1n2onr6 xqbnct6 xga75y6']";

                    postDiv = await newPage.QuerySelectorAsync(selector2);

                    if (postDiv != null)
                        Libary.Instance.LogDebug($"{Libary.IconWarn} Tìm postDiv bằng selector 2");
                }
                if (postDiv == null)
                {
                    if (postDiv == null)
                    {
                        Libary.Instance.LogDebug($"{Libary.IconFail} Không tìm thấy postDiv bài gốc");
                        await newPage.CloseAsync();
                        return null;
                    }
                }
                // ===== 3️⃣ Lấy tương tác =====
                (post.LikeCount, post.CommentCount, post.ShareCount) =
                    await ExtractPostInteractionsAsync(postDiv);
                Libary.Instance.LogDebug(
                    $"{Libary.CountIcon((post.LikeCount ?? 0) + (post.CommentCount ?? 0) + (post.ShareCount ?? 0))} " +
                    $"Tương tác 👍={post.LikeCount ?? 0} 💬={post.CommentCount ?? 0} 🔁={post.ShareCount ?? 0}"
                );

                // ===== 4️⃣ Lấy thời gian =====
                var postinfor = await postDiv.QuerySelectorAllAsync("div.xu06os2.x1ok221b");
                string originalTime = "N/A";

                foreach (var info in postinfor)
                {
                    var txt = (await info.InnerTextAsync())?.Trim().ToLower();
                    if (string.IsNullOrWhiteSpace(txt)) continue;

                    if (ProcessingDAO.Instance.IsTime(txt))
                    {
                        originalTime = txt;
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(originalTime))
                {
                    originalTime = TimeHelper.CleanTimeString(originalTime);
                    post.PostTime = originalTime;
                    post.RealPostTime = TimeHelper.ParseFacebookTime(originalTime);
                    Libary.Instance.LogDebug($"{Libary.IconOK} Lấy thời gian bài gốc");
                }
                else
                {
                    post.PostTime = "N/A";
                    Libary.Instance.LogDebug($"{Libary.IconFail} Không lấy được thời gian bài gốc");
                }

                // ===== 5️⃣ Lấy tên page =====
                string pageName = "";
                var pageNameDiv = await postDiv.QuerySelectorAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs']"
                );

                if (pageNameDiv != null)
                {
                    pageName = (await pageNameDiv.InnerTextAsync()) ?? "";
                    post.PageName = pageName;
                    Libary.Instance.LogDebug($"{Libary.IconOK} Lấy tên page bài gốc");
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconWarn} Không tìm thấy tên page bài gốc");
                }

                // ===== 6️⃣ Lấy nội dung =====
                string content = await ExtractPopupContentAsync(newPage, postDiv);

                if (string.IsNullOrWhiteSpace(content) || content == "N/A")
                {
                    string bgText = await BackgroundTextAllAsync(mainPage, postDiv);
                    if (!string.IsNullOrWhiteSpace(bgText))
                    {
                        content = bgText.Trim();
                        post.PostType = PostTypeEnum.Page_BackGround.ToString();
                    }
                }
                else post.PostType = PostTypeEnum.Page_Normal.ToString();
                post.Content = content;
                string preview = content ?? "";
                if (preview.Length > 100)
                    preview = preview.Substring(0, 100) + "...";
                Libary.Instance.LogDebug(
                    string.IsNullOrWhiteSpace(content)
                        ? $"{Libary.IconFail} Nội dung bài gốc rỗng"
                        : $"{Libary.IconOK} Lấy nội dung bài gốc (len={content.Length}) | preview=\"{preview}\""
                );
                success = true;
                return post;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} [ORIGINAL POST] Lỗi khi lấy bài gốc: {ex.Message}");
                return null;
            }
            finally
            {
                await SafeClosePageAsync(newPage);

                if (success)
                    Libary.Instance.LogDebug("✔ [ORIGINAL POST] Hoàn tất xử lý bài gốc");
                else
                    Libary.Instance.LogDebug("❌ [ORIGINAL POST] Kết thúc (FAIL)");

                Libary.Instance.LogDebug(Libary.EndPost());
            }
        }
        /// 
        // hàm tổng lấy bài viết 1 groups, đầu vào từng post, lấy feed ở ngoài      
        public async Task<List<PostPage>> GetPostAutoPageAsyncV3(IPage page, IElementHandle post, string pagename, string url)
        {
            var listPosts = new List<PostPage>();
            PostPage postB = new PostPage();
            PostPage postA = new PostPage();
            string urlreal = url;
            try
            {
                string PosterName = "N/A", PosterLink = "N/A", PosterNote = "N/A";
                string OriginalPosterLink = "N/A";
                string PostTime = "N/A", OriginalPostTime = "N/A";
                string PostLink = "N/A", OriginalPostLink = "N/A";
                string Content = "N/A";
                string PostStatus = "N/A";
                int CommentCount = 0, ShareCount = 0, LikeCount = 0;
                Libary.Instance.CreateLog("   🔹 Bắt đầu phân tích post");            
                // 1️⃣ Lấy các node postinfor (giống Selenium)
                var postinfor = await post.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");
                Libary.Instance.CreateLog($"   🔹 postinfor.Count = {postinfor.Count}");
                var (timeList, linkList) = await ExtractTimeAndLinksAsync(postinfor);
                Libary.Instance.CreateLog($"🔹 TimeList={timeList.Count}, LinkList={linkList.Count}");              
                for (int i = 0; i < Math.Max(timeList.Count, linkList.Count); i++)
                {
                    string time = i < timeList.Count ? timeList[i] : "N/A";
                    string link = i < linkList.Count ? linkList[i] : "N/A";
                }
                if (timeList.Count > 0 || linkList.Count > 0)
                {
                    try
                    {
                        (PostTime, OriginalPostTime, PostLink, OriginalPostLink) = await PostTypeDetectorAsync(timeList, linkList);
                        Libary.Instance.CreateLog("   🔹 lấy time/link bình thường");                     
                        Libary.Instance.CreateLog($"   🔹 PostTime = {PostTime}");
                        Libary.Instance.CreateLog($"   🔹 OriginalPostTime = {OriginalPostTime}");
                        Libary.Instance.CreateLog($"   🔹 PostLink = {PostLink}");
                        Libary.Instance.CreateLog($"   🔹 OriginalPostLink = {OriginalPostLink}");
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.CreateLog($"⚠️ [GetPostPageAsync] Lỗi PostTypeDetectorAsync, có time vẫn lỗi: {ex.Message}");
                        // Giữ giá trị mặc định để tránh null reference                   
                    }
                }
                try
                {
                    switch (timeList.Count)
                    {
                        case 1: // có 1 link 1 time thì là bài đăng
                            Libary.Instance.CreateLog("Vào Case 1");
                            if (postinfor.Count == 3 || postinfor.Count == 2)
                            {
                                var posterContainer = GetSafe(postinfor, 0);
                                try
                                {
                                    (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                    Libary.Instance.CreateLog("   🔹 Người đăng lấy bằng selector postinfor 0 thành công");
                                }
                                catch
                                {
                                    (PosterName, PosterLink) = await GetPosterFromProfileNameAsync(post);
                                    if (posterContainer != null) PosterName = "Người đăng ẩn danh";
                                    Libary.Instance.CreateLog("   🔹 Người đăng lấy bằng profile_name");
                                }

                                if (postinfor.Count == 3)
                                {
                                    Libary.Instance.CreateLog("Vào Postinfor3");
                                    var contentContainer = GetSafe(postinfor, 2);
                                    Libary.Instance.CreateLog("   🔹 Đang lấy nội dung...");
                                    Content = await GetContentTextAsync(page, contentContainer);
                                    Libary.Instance.CreateLog("Kết quả có ký tự:" +Content);
                                    // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                    if (string.IsNullOrWhiteSpace(Content) || Content == "N/A")
                                    {
                                        Libary.Instance.CreateLog("Content trống, thử lấy bằng backgroud:");
                                        Content = await BackgroundTextAllAsync(page, post);
                                        if (!string.IsNullOrWhiteSpace(Content))
                                            PostStatus = "bài đăng kèm ảnh/video";
                                        else
                                            PostStatus = "bài đăng không có nội dung";
                                    }
                                    else PostStatus = "bài đăng bình thường";
                                }//else là 2 nền màu, ảnh
                                else
                                {
                                    Libary.Instance.CreateLog("Vào Postinfor2");
                                    // var listText = new List<string>();
                                    Content = await BackgroundTextAllAsync(page,post);
                                    PostStatus = "bài đăng nền màu/ảnh";
                                }
                            }
                            else if (postinfor.Count == 4)
                            {
                                var posterContainer = GetSafe(postinfor, 0);
                                (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                if (PosterName != "N/A" && PosterName != null) Libary.Instance.CreateLog("Lấy thông tin người đăng bằng GetPosterInfoBySelectorsAsync thành công ");
                                var contentContainer = GetSafe(postinfor, 2);
                                Libary.Instance.CreateLog("   🔹 Đang lấy nội dung...");
                                Content = await GetContentTextAsync(page, contentContainer);
                                // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                if (string.IsNullOrWhiteSpace(Content))
                                {
                                    Content = await BackgroundTextAllAsync(page, post);
                                    if (!string.IsNullOrWhiteSpace(Content))
                                        PostStatus = "bài đăng kèm ảnh/video";
                                    else
                                        PostStatus = "bài đăng không có nội dung";
                                }
                                else
                                {
                                    PostStatus = "bài đăng dẫn link ngoài";
                                }
                            }
                            Libary.Instance.CreateLog("   🔹 Đang lấy tương tác...");
                            (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                            if (!string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A")
                        {
                            var fbType = await CheckTypeCachedAsync(page, PosterLink);
                                PosterNote = fbType.ToString();
                        }
                            if (PostLink != null && PostLink != "N/A")
                            {
                                postA.PostLink = PostLink;
                                // postA.PostTime = PostTime;
                                postA.PostTime = PostTime;
                                postA.PosterName = PosterName;
                                postA.PosterLink = PosterLink;
                                postA.PageName = pagename;
                                postA.PageLink = urlreal;
                                postA.Content = Content;
                                postA.PosterNote = PosterNote;
                                postA.ShareCount = ShareCount;
                                postA.CommentCount = CommentCount;  
                                postA.LikeCount = LikeCount;
                                postA.PostType = "Page Đăng/duyệt: "+PostStatus;
                            };
                            listPosts.Add(postA);
                            break;
                        case 2:
                            {
                             Libary.Instance.CreateLog("   🔹 CASE 2: Bài share → Tách bài gốc qua tab mới");                         
                             var posterContainer = GetSafe(postinfor, 0);
                             (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                Libary.Instance.CreateLog($"   🔹 PosterName = {PosterName}");
                                Libary.Instance.CreateLog($"   🔹 PosterLink = {PosterLink}");
                                Libary.Instance.CreateLog("   🔹 Đang lấy nội dung...");
                            
                                var contentContainer = GetSafe(postinfor, 2);
                                Content = await GetContentTextAsync(page, contentContainer);                         
                                if (Content == "" || Content == "N/A")
                                {
                                    Libary.Instance.CreateLog("   🔹 lấy nội dung bằng Background");
                                    Content = await BackgroundTextAllAsync(page, post);
                                }
                                if(Content != "N/A") Libary.Instance.CreateLog("   🔹 lấy nội dung thành công, kết quả: "+Content);
                                (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                                if (!string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A")
                                {
                                    var fbType = await CheckTypeCachedAsync(page, PosterLink);
                                    PosterNote = fbType.ToString();
                                }
                                if (PostLink != null && PostLink != "N/A")
                                {
                                    postA.PostLink = PostLink;
                                    postA.PostTime = PostTime;
                                    postA.PosterName = PosterName;
                                    postA.PosterLink = PosterLink;
                                    postA.PageName = pagename;
                                    postA.PageLink = urlreal;
                                    postA.Content = Content;
                                    postA.PosterNote = PosterNote;
                                    postA.ShareCount = ShareCount;
                                    postA.CommentCount = CommentCount;
                                    postA.LikeCount = LikeCount;
                                    postA.PostType = "Page Share: " + PostStatus;
                                };
                                listPosts.Add(postA);
                                if (OriginalPostLink != "N/A" && OriginalPosterLink != null) postB = await GetPostOriginal(page, OriginalPostLink);
                                if (postB != null && postB.PostLink != "N/A")
                                {   if (postA.PosterLink == OriginalPosterLink)                                     
                                    listPosts.Add(postB);
                                    //table share
                                }
                                else
                                {
                                    Libary.Instance.CreateLog("[Caller] ❌ GetPostOriginal lỗi" );
                                }
                            }                          
                            break;
                        case 0:
                            {
                                Libary.Instance.CreateLog("🟦 CASE 0 → Xử lý bài REEL (postinfor.Count = 0)");

                                try
                                {
                                    postA = await ExtractPostReelAll(page, post);

                                    if (postA != null && postA.PostLink != "N/A")
                                    {
                                        postA.PageName = pagename;
                                        Libary.Instance.CreateLog($"🟩 Reel OK → Link={postA.PostLink}");                                    
                                        listPosts.Add(postA);
                                    }
                                    else
                                    {
                                        Libary.Instance.CreateLog("🟥 ExtractPostReelAll return NULL → bỏ qua bài reel");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Libary.Instance.CreateLog($"❌ Lỗi CASE 0 (Reel): {ex.Message}");
                                }
                                break;
                            }
                           
                    }
                }
                catch (Exception ex)
                {
                    Libary.Instance.CreateLog($"⚠️ Lỗi trong case lấy nội dung: {ex.Message}");
                }
                try
                {
                   /* if (!string.IsNullOrWhiteSpace(OriginalPosterLink) && OriginalPosterLink != "N/A")
                    {
                        var fbType = await CheckTypeCachedAsync(page, OriginalPosterLink);
                        OriginalPosterNote = fbType.ToString();
                    }*/
                    //string finalPageName = OriginalPosterName;
                    //string finalPageLink = OriginalPosterLink;
                   // if (postB.PosterNote == "Person" || postB.PosterNote == "PersonKOL")
                    //{
                    //    finalPageName = "";
                   //     finalPageLink = "";
                    //}
                    //if (!string.IsNullOrEmpty(OriginalPostLink) && OriginalPostLink != "N/A")
                    //{
                     /*  var (likeB, commentB, shareB) = await OpenLinkInNewTabForInteractionsAsync(page, OriginalPostLink);                      
                        LikeCountOriginal = likeB;
                        CommentCountOriginal= commentB;
                        SharecountOriginal = shareB;
                        Libary.Instance.CreateLog(
                            $"[OriginalPost] 🧩 Like={likeB}, Comment={commentB}, Share={shareB}"
                        );*/
                        
                   //if(postB != null)     listPosts.Add(postB);
                    //}
                }
                catch (Exception ex)
                {
                    Libary.Instance.CreateLog($"⚠️ Lỗi trong lấy tương tác bài gốc B: {ex.Message}");
                }             
                //if (string.IsNullOrEmpty(OriginalPostTime)) PostStatus = "Page Đăng/duyệt bài" + PostStatus;
                //else PostStatus = "Page share lại bài" + PostStatus;
               
                if(listPosts.Count()>0) return listPosts;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GroupsDAO_PW] GetPostAutoPageAsyncV3 lỗi: " + ex.Message);
            }
            return listPosts;
        }       
        // Hàm xử lý Groups
        public async Task<PostResult> GetPostAutoGroupsAsync(IPage page, IElementHandle post, string pagename, string url)
        {
            var listShares = new List<ShareItem>();
            var listPosts = new List<PostPage>();
            PostPage postB = new PostPage();
            PostPage postA = new PostPage();
            string urlreal = url;
            try
            {
                string PosterName = "N/A", PosterLink = "N/A", PosterNote = "N/A";
                string OriginalPosterLink = "N/A";string OriginalContent = "N/A";
                string PostTime = "N/A", OriginalPostTime = "N/A";
                string PostLink = "N/A", OriginalPostLink = "N/A";
                string Content = "N/A", OriginalPostType = "N/A";
                string PostType = "N/A"; string OriginalPosterName = "N/A";
                int CommentCount = 0, ShareCount = 0, LikeCount = 0;
                Libary.Instance.LogTech(Environment.NewLine +
                    "================================= BẮT ĐẦU PHẦN TÍCH POST GROUPS =================================" +Environment.NewLine );
                // 1️⃣ Lấy các node postinfor (giống Selenium)
                bool IsShareRell = false;
                var (hasReel, rawReelLink) = await DetectReelFromPostAsync(post);
                string reelLink = ProcessingHelper.NormalizeReelLink(rawReelLink);
                var postinfor = await post.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");         
                var (timeList, linkList) = await ExtractTimeAndLinksAsync(postinfor);
                Libary.Instance.LogTech($"[Groups] postinfor={postinfor.Count}, time={timeList.Count}, link={linkList.Count}");
                Libary.Instance.LogTech(Environment.NewLine +
                   "================================= lấy thời gian =================================" + Environment.NewLine);
                for (int i = 0; i < Math.Max(timeList.Count, linkList.Count); i++)
                {
                    string time = i < timeList.Count ? timeList[i] : "N/A";
                    string link = i < linkList.Count ? linkList[i] : "N/A";
                }
                if (timeList.Count > 0 || linkList.Count > 0)
                {
                    try
                    {
                        (PostTime, OriginalPostTime, PostLink, OriginalPostLink) = await PostTypeDetectorAsync(timeList, linkList);
                      // log
                        bool hasPostTime = !string.IsNullOrWhiteSpace(PostTime) && PostTime != "N/A";
                        bool hasPostLink = !string.IsNullOrWhiteSpace(PostLink) && PostLink != "N/A";
                        bool hasOriginalLink = !string.IsNullOrWhiteSpace(OriginalPostLink) && OriginalPostLink != "N/A";
                        if (hasPostTime && hasPostLink)
                        {
                            if (hasOriginalLink)
                            {
                                // ✅✅ Bài share
                                Libary.Instance.LogTech("✅✅ Lấy time & link post thành công (Bài share)");
                            }
                            else
                            {
                                // ✅ Bài cơ bản
                                Libary.Instance.LogTech("✅ Lấy time & link post thành công (Bài cơ bản)");
                            }
                        }
                        else
                        {
                            // ❌ Thiếu dữ liệu
                            Libary.Instance.LogTech( "❌ Không lấy đủ time/link post");
                        }
                        Libary.Instance.LogTech("   🔹 lấy time/link bình thường");
                        Libary.Instance.LogTech($"   🔹 PostTime = {PostTime}"); 
                        Libary.Instance.LogTech($"   🔹 RealPostTime = {TimeHelper.ParseFacebookTime(PostTime)}");
                        Libary.Instance.LogTech($"   🔹 OriginalPostTime = {OriginalPostTime}");
                        Libary.Instance.LogTech($"   🔹 RealOriginalPostTime = {TimeHelper.ParseFacebookTime(OriginalPostTime)}");
                        Libary.Instance.LogTech($"   🔹 PostLink = {PostLink}");
                        Libary.Instance.LogTech($"   🔹 OriginalPostLink = {OriginalPostLink}");
                        //====== hết log=========
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogTech($"⚠️ [GetPostAutoGroups] Lỗi PostTypeDetectorAsync, có time vẫn lỗi: {ex.Message}");
                        // Giữ giá trị mặc định để tránh null reference                   
                    }
                }
                // ===== Detect SHARE REEL (PAGE) =====
                if (hasReel && PostLink != "N/A")
                {
                    // 👉 Reel gốc (tự đăng) → BỎ QUA
                    if (PostLink == reelLink)
                    {
                        // ❌ KHÔNG làm gì
                        // 👉 thoát if, chạy tiếp flow cũ
                    }
                    else
                    {
                        // 🔁 SHARE REEL
                        var reelOriginal = await ExtractPostReelAll(page, post);

                        if (reelOriginal != null && reelOriginal.PostLink != "N/A")
                        {
                            OriginalPostLink = reelOriginal.PostLink;
                            OriginalPostTime = reelOriginal.PostTime;
                            OriginalPosterName = reelOriginal.PosterName;
                            OriginalPosterLink = reelOriginal.PosterLink;
                            OriginalContent = reelOriginal.Content;

                            PostType = PostTypeEnum.Share_NoContent.ToString();

                            Libary.Instance.LogTech("🟦 [PAGE] Detect SHARE REEL");

                            // 🚨 CHẶN FLOW CŨ
                            IsShareRell = true;
                        }
                    }
                }

                // ===== FLOW CŨ =====
                if (!IsShareRell)
                {
                    try
                    {
                        Libary.Instance.LogTech(Environment.NewLine +
                            "-----------------------LẤY CHI TIẾT -----------------------------" + Environment.NewLine);
                        switch (timeList.Count)
                        {
                            case 1: // có 1 link 1 time thì là bài đăng hoặc share Reel                
                                if (postinfor.Count == 3 || postinfor.Count == 2)
                                {
                                    var posterContainer = GetSafe(postinfor, 0);
                                    try
                                    {
                                        (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                        Libary.Instance.LogTech("   🔹 Người đăng lấy bằng selector postinfor 0 thành công", AppConfig.ENABLE_TECH_LOG);
                                    }
                                    catch
                                    {
                                        (PosterName, PosterLink) = await GetPosterFromProfileNameAsync(post);
                                        if (posterContainer != null) PosterName = "Người đăng ẩn danh";
                                        Libary.Instance.LogTech($"{Libary.IconWarn} FALLBACK lấy PosterName bằng profile_name",AppConfig.ENABLE_TECH_LOG);
                                    }
                                    //Log
                                    bool hasPostername = !string.IsNullOrWhiteSpace(PosterName) && PosterName != "N/A";
                                    bool hasPosterLink = !string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A";
                                    if (hasPostername && hasPosterLink) Libary.Instance.LogTech($"{Libary.IconOK} Lấy Người đăng thành công");
                                    else
                                    {
                                        if (!hasPosterLink && !hasPostername) Libary.Instance.LogTech($"{Libary.IconFail} không lấy được thông tin người đăng");
                                        else if (!hasPosterLink) Libary.Instance.LogTech($"{Libary.IconFail} không lấy được link người đăng");
                                        else Libary.Instance.LogTech($"{Libary.IconFail} không lấy được Tên người đăng");
                                    }
                                    // == hết log
                                    if (postinfor.Count == 3)
                                    {
                                        Libary.Instance.LogTech("Vào Postinfor3", AppConfig.ENABLE_TECH_LOG);
                                        var contentContainer = GetSafe(postinfor, 2);
                                        Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                        string ContentNomal = await GetContentTextAsync(page, contentContainer);
                                        //===log
                                        if (!string.IsNullOrWhiteSpace(ContentNomal) && ContentNomal != "N/A")
                                        {
                                            Content = ContentNomal;
                                            Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content Nomal", AppConfig.ENABLE_TECH_LOG);
                                            PostType = PostTypeEnum.Page_Normal.ToString();
                                        }
                                        else
                                        {
                                            Libary.Instance.LogTech("Content trống, thử lấy bằng backgroud:", AppConfig.ENABLE_TECH_LOG);
                                            string ContentBackGround = await BackgroundTextAllAsync(page, post);
                                            if (!string.IsNullOrWhiteSpace(ContentBackGround) && ContentBackGround != "N/A")
                                            {
                                                Content = ContentBackGround;
                                                Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content BackGround", AppConfig.ENABLE_TECH_LOG);
                                                PostType = PostTypeEnum.Page_BackGround.ToString();
                                            }
                                            else
                                            {
                                                PostType = PostTypeEnum.Page_Unknow.ToString();
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                        {
                                            Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công, số ký tự: " + Content.Length);
                                            if (!string.IsNullOrWhiteSpace(Content))
                                            {
                                                Libary.Instance.LogTech("preview: " + ProcessingHelper.PreviewText(Content));
                                            }
                                        }
                                        else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được nội dung bài viết");
                                        Libary.Instance.LogTech("chốt kiểu bài viết: " + PostType);
                                    }//else là 2 nền màu, ảnh
                                    else
                                    {
                                        Libary.Instance.LogTech("Vào Postinfor2", AppConfig.ENABLE_TECH_LOG);
                                        Content = await BackgroundTextAllAsync(page, post);
                                        if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                        {
                                            PostType = PostTypeEnum.Page_BackGround.ToString();
                                            Libary.Instance.LogTech("chốt kiểu bài viết: " + PostType);
                                            Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                        }
                                        else
                                        {
                                            PostType = PostTypeEnum.Page_NoConent.ToString();
                                            Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được nội dung bài viết Case 2");
                                        }
                                    }
                                }
                                else if (postinfor.Count == 4)
                                {
                                    Libary.Instance.LogTech("Vào Postinfor 4", AppConfig.ENABLE_TECH_LOG);
                                    var posterContainer = GetSafe(postinfor, 0);
                                    (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                    if (PosterName != "N/A" && PosterName != null)
                                        Libary.Instance.LogTech($"{Libary.IconOK} [ GetPosterInfoBySelectorsAsync]Lấy thông tin người đăng thành công ");
                                    var contentContainer = GetSafe(postinfor, 2);
                                    Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                    Content = await GetContentTextAsync(page, contentContainer);
                                    // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                    if (string.IsNullOrWhiteSpace(Content) || Content == "N/A")
                                    {
                                        Content = await BackgroundTextAllAsync(page, post);
                                        if (!string.IsNullOrWhiteSpace(Content))
                                        {
                                            Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                            PostType = PostTypeEnum.Page_BackGround.ToString();
                                            Libary.Instance.LogTech("preview: " + ProcessingHelper.PreviewText(Content));
                                        }
                                        else
                                        {
                                            Libary.Instance.LogTech($"{Libary.IconFail} Background k thấy nội dung");
                                            PostType = PostTypeEnum.Page_Unknow.ToString();
                                        }
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail} Bài viết gắn link ngoài k có content");
                                        PostType = PostTypeEnum.Page_LinkWeb.ToString();
                                    }
                                    Libary.Instance.LogTech("chốt kiểu bài viết: " + PostType);
                                }
                                Libary.Instance.LogTech("----  CASE 1 🔹 Đang lấy tương tác...---");
                                (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                                if (LikeCount != 0 || CommentCount != 0 || ShareCount != 0) Libary.Instance.LogTech($"{Libary.IconOK}Lấy tương tác thành công: Like " + LikeCount + " Share: " + ShareCount);
                                else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được tương tác bài viết");
                                if (!string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A")
                                {
                                    var fbType = await CheckTypeCachedAsync(page, PosterLink);
                                    PosterNote = fbType.ToString();
                                    if (!string.IsNullOrEmpty(PosterNote) && PosterNote != "N/A")
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconOK}[CheckType] thành công: " + PosterNote);
                                    }
                                    else Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy Kiểu người đăng");
                                }
                                if (PostLink != null && PostLink != "N/A")
                                {
                                    postA.PostLink = PostLink;
                                    postA.RealPostTime = TimeHelper.ParseFacebookTime(PostTime);
                                    postA.PostTime = PostTime;
                                    postA.PosterName = PosterName;
                                    postA.PosterLink = PosterLink;
                                    postA.PageName = pagename;
                                    postA.PageLink = urlreal;
                                    postA.Content = Content;
                                    postA.PosterNote = PosterNote;
                                    postA.ShareCount = ShareCount;
                                    postA.CommentCount = CommentCount;
                                    postA.LikeCount = LikeCount;
                                    postA.PostType = PostType;
                                };
                                listPosts.Add(postA);
                                break;
                            case 2:
                                {
                                    Libary.Instance.LogTech($" {Libary.IconInfo}  🔹 CASE 2: Bài share");
                                    var posterContainer = GetSafe(postinfor, 0);
                                    (PosterName, PosterLink) = await GetPosterInfoBySelectorsAsync(posterContainer);
                                    //Log người đăng
                                    bool hasPostername = !string.IsNullOrWhiteSpace(PosterName) && PosterName != "N/A";
                                    bool hasPosterLink = !string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A";
                                    if (hasPostername && hasPosterLink) Libary.Instance.LogTech($"{Libary.IconOK} Lấy Người đăng thành công");
                                    else
                                    {
                                        if (!hasPosterLink && !hasPostername) Libary.Instance.LogTech($"{Libary.IconFail} không lấy được thông tin người đăng");
                                        else if (!hasPosterLink) Libary.Instance.LogTech($"{Libary.IconFail} không lấy được link người đăng");
                                        else Libary.Instance.LogTech($"{Libary.IconFail} không lấy được Tên người đăng");
                                    }
                                    // == hết log người đăng
                                    Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                    // =====LẤY CONTENT NORMAL =====
                                    string noidung = null, noidunggoc = null, noidunganh = null;
                                    if (postinfor.Count >= 5)
                                    {
                                        var contentContainer2 = GetSafe(postinfor, 2);

                                        var contentContainer4 = GetSafe(postinfor, 4);
                                        string content2 = await GetContentTextAsync(page, contentContainer2);
                                        string content4 = await GetContentTextAsync(page, contentContainer4);

                                        bool hasContent2 = !string.IsNullOrWhiteSpace(content2) && content2 != "N/A";
                                        bool hasContent4 = !string.IsNullOrWhiteSpace(content4) && content4 != "N/A";

                                        if (postinfor.Count == 6)
                                        {
                                            var contentContainer5 = GetSafe(postinfor, 5);
                                            string content5 = await GetContentTextAsync(page, contentContainer5);
                                            bool hasContent5 = !string.IsNullOrWhiteSpace(content5) && content5 != "N/A";
                                            if (hasContent2 && hasContent5)
                                            {
                                                noidung = content2;
                                                noidunggoc = content5;
                                                PostType = PostTypeEnum.Share_WithContent.ToString();
                                                Libary.Instance.LogTech("🟩 Lấy content share + content gốc");
                                                OriginalPostType = PostTypeEnum.Page_Normal.ToString();
                                            }
                                            else if (!hasContent2 && !hasContent5 && hasContent4)
                                            {
                                                noidunggoc = content4;
                                                PostType = PostTypeEnum.Share_NoContent.ToString();
                                                Libary.Instance.LogTech("🟨 Lấy content gốc từ container 4");
                                                OriginalPostType = PostTypeEnum.Page_Normal.ToString();
                                            }
                                            else if (hasContent2)
                                            {
                                                noidung = content2;
                                                PostType = PostTypeEnum.Share_WithContent.ToString();
                                                OriginalPostType = PostTypeEnum.Page_Unknow.ToString();
                                                Libary.Instance.LogTech("🟦 Chỉ có content share");
                                            }
                                            else
                                            {
                                                Libary.Instance.LogTech("🟥 Không lấy được content text");
                                            }
                                        }
                                    }
                                    else if (postinfor.Count == 4)
                                    {
                                        Libary.Instance.LogTech("   🔹 Content normal rỗng, thử Background", AppConfig.ENABLE_TECH_LOG);
                                        noidunganh = await BackgroundTextAllAsync(page, post);
                                    }
                                    if (!string.IsNullOrWhiteSpace(noidunggoc)) OriginalContent = noidunggoc;
                                    if (!string.IsNullOrEmpty(noidung)) Content = noidung;
                                    if (!string.IsNullOrEmpty(noidunganh)) Content = noidunganh;
                                    // ===== 4️⃣ LOG TỔNG KẾT =====
                                    if (Content != "N/A")
                                    {
                                        Libary.Instance.LogTech(
                                            $"{Libary.IconOK} Lấy nội dung bài viết thành công " +
                                            $"(len={Content.Length})"
                                        );
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail} Không lấy được nội dung bài viết");
                                    }
                                    // ===== LẤY TƯƠNG TÁC =====
                                    (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                                    int totalInteraction = LikeCount + CommentCount + ShareCount;
                                    Libary.Instance.LogTech(
                                        $"{Libary.CountIcon(totalInteraction)} " +
                                        $"Tương tác 👍={LikeCount} 💬={CommentCount} 🔁={ShareCount}"
                                    );
                                    // ===== CHECK TYPE NGƯỜI ĐĂNG =====
                                    if (!string.IsNullOrWhiteSpace(PosterLink) && PosterLink != "N/A")
                                    {
                                        var fbType = await CheckTypeCachedAsync(page, PosterLink);
                                        PosterNote = fbType.ToString();

                                        Libary.Instance.LogTech($"{Libary.IconOK} Xác định loại người đăng: {PosterNote}"
                                        );
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail} Không xác định được loại người đăng (PosterLink rỗng)");
                                    }
                                    if (PostLink != null && PostLink != "N/A")
                                    {
                                        postA.PostLink = PostLink;
                                        postA.RealPostTime = TimeHelper.ParseFacebookTime(PostTime);
                                        postA.PostTime = PostTime;
                                        postA.PosterName = PosterName;
                                        postA.PosterLink = PosterLink;
                                        postA.PageName = pagename;
                                        postA.PageLink = urlreal;
                                        postA.Content = Content;
                                        postA.PosterNote = PosterNote;
                                        postA.ShareCount = ShareCount;
                                        postA.CommentCount = CommentCount;
                                        postA.LikeCount = LikeCount;
                                        postA.PostType = PostType;
                                        listPosts.Add(postA);
                                        if (OriginalPostLink != "N/A" && OriginalPosterLink != null)
                                        {
                                            Libary.Instance.LogTech(Environment.NewLine +
                               "-----------------------LẤY CHI TIẾT BÀI GỐC -----------------------------" + Environment.NewLine);
                                            postB = await GetPostOriginal(page, OriginalPostLink);
                                            Libary.Instance.LogTech(
                                               postB == null
                                                   ? "Không lấy được bài gốc"
                                                   : SummarizeOriginalPost(postB)
                                           );
                                        }
                                        if (postB != null && postB.PostLink != "N/A")
                                        {
                                            //if (postA.PosterLink == OriginalPosterLink)
                                            listPosts.Add(postB);
                                            //table share
                                            listShares.Add(new ShareItem
                                            {
                                                PageLinkA = urlreal,
                                                PostLinkB = postB.PostLink,
                                                ShareTimeRaw = postA.PostTime,
                                                ShareTimeReal = TimeHelper.ParseFacebookTime(postA.PostTime)//để ý
                                            });
                                            Libary.Instance.LogTech($"[SHARE ADD] Raw='{postA.PostTime}' Parsed='{TimeHelper.ParseFacebookTime(postA.PostTime)}'");
                                        }
                                        else
                                        {
                                            Libary.Instance.LogTech($"{Libary.IconFail}[Caller] ❌ LẤY BÀI GỐC BẰNG GetPostOriginal lỗi");
                                        }
                                    }
                                }
                                break;
                            case 0:
                                {
                                    Libary.Instance.LogTech($"{Libary.IconInfo}🟦 CASE 0 → Xử lý bài REEL (postinfor.Count = 0)");
                                    try
                                    {
                                        postA = await ExtractPostReelAll(page, post);

                                        if (postA != null && postA.PostLink != "N/A")
                                        {
                                            postA.PageName = pagename;
                                            Libary.Instance.LogTech($"{Libary.IconOK}🟩 Reel OK");
                                            listPosts.Add(postA);
                                        }
                                        else
                                        {
                                            Libary.Instance.LogTech($"{Libary.IconFail}🟥 ExtractPostReelAll return NULL → bỏ qua bài reel");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail}❌ Lỗi CASE 0 (Reel): {ex.Message}");
                                    }
                                    break;
                                }
                        }
                    }

                    catch (Exception ex)
                    {
                        Libary.Instance.LogTech($"⚠️ Lỗi trong lấy chi tiết bài viết: {ex.Message}");
                    }
                }
                //if (string.IsNullOrEmpty(OriginalPostTime)) PostStatus = "Page Đăng/duyệt bài" + PostStatus;
                //else PostStatus = "Page share lại bài" + PostStatus;
                if (listPosts.Count() > 0) return new PostResult { Posts = listPosts, Shares = listShares }; ;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("[PageDAO] GetPostAutoGroups lỗi: " + ex.Message);
            }
            finally
            {
                Libary.Instance.LogTech(
                    $"{Libary.IconInfo}[GetPostAutoGROUPS] Kết thúc xử lý post | Posts={listPosts.Count}, Shares={listShares.Count}"
                );
            }
            return new PostResult { Posts = listPosts, Shares = listShares }; 
        }

        //------------------------hàm full lấy reel => HIỆN TẠI ĐANG DÙNG
        public async Task<PostPage> ExtractPostReelAll(IPage mainPage, IElementHandle post)
        {
            var reel = new PostPage()
            {

                PostType = PostTypeEnum.page_Real_Cap.ToString()
            };
            try
            {
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
                    if (ProcessingDAO.Instance.IsTime(time))
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
                if (posterLink != "N/A") reel.PosterLink = ProcessingDAO.Instance.ShortenPosterLinkReel(posterLink);
                if (reel.PosterLink != "N/A")
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} dùng CheckType lấy type người đăng Reel");
                    var fbTypeReel = await CheckTypeCachedAsync(mainPage, reel.PosterLink);
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
        public async Task<(int like, int comment, int share)> ExtractReelInteractionsAsync(IPage page)
        {
            int likes = 0, comments = 0, shares = 0;
            try
            {
                Libary.Instance.LogDebug("[Reel] 🎬 Bắt đầu đọc tương tác bài Reel...");

                // 1️⃣ Lấy danh sách thẻ reel
                var interactZone = await page.QuerySelectorAllAsync("div[class='xuk3077x78zum5']");
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
        //---------------------------------------------
        //----------- hàm xử lý fanpage riêng-------------------
        public async Task<PostResult> GetPostAutoFanpageAsync(IPage page, IElementHandle post, string pagename, string url)
        {
            var listShares = new List<ShareItem>();
            var listPosts = new List<PostPage>();
            PostPage postB = new PostPage();
            PostPage postA = new PostPage();
            string urlreal = url;
            try
            {
                string PosterName = "N/A", PosterLink = "N/A", PosterNote = "N/A";
                string OriginalPosterLink = "N/A";
                string PostTime = "N/A", OriginalPostTime = "N/A";
                string PostLink = "N/A", OriginalPostLink = "N/A";
                string Content = "N/A";
                string PostType = "N/A"; //OriginalPostType = "N/A";
                int CommentCount = 0, ShareCount = 0, LikeCount = 0;
                Libary.Instance.LogTech(Environment.NewLine +
                   "================================= BẮT ĐẦU PHẦN TÍCH POST GROUPS =================================" + Environment.NewLine
                  );
                Libary.Instance.LogTech($"[Fanpage] Bắt đầu phân tích post | Page={pagename}");
                // 1️⃣ Lấy các node postinfor (giống Selenium)
                var postinfor = await post.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");
                var (timeList, linkList) = await ExtractTimeAndLinksAsync(postinfor);
                // ===== LOGTECH: TÓM TẮT =====
                Libary.Instance.LogTech($"[Fanpage] postinfor={postinfor.Count}, time={timeList.Count}, link={linkList.Count}");
                Libary.Instance.LogTech(Environment.NewLine +
           "================================= lấy thời gian =================================" + Environment.NewLine);
                for (int i = 0; i < Math.Max(timeList.Count, linkList.Count); i++)
                {
                    string time = i < timeList.Count ? timeList[i] : "N/A";
                    string link = i < linkList.Count ? linkList[i] : "N/A";
                }
                if (timeList.Count > 0 || linkList.Count > 0)
                {
                    try
                    {
                        (PostTime, OriginalPostTime, PostLink, OriginalPostLink) = await PostTypeDetectorAsync(timeList, linkList);
                        // log
                        bool hasPostTime = !string.IsNullOrWhiteSpace(PostTime) && PostTime != "N/A";
                        bool hasPostLink = !string.IsNullOrWhiteSpace(PostLink) && PostLink != "N/A";
                        bool hasOriginalLink = !string.IsNullOrWhiteSpace(OriginalPostLink) && OriginalPostLink != "N/A";
                        if (hasPostTime && hasPostLink)
                        {
                            if (hasOriginalLink)
                            {
                                // ✅✅ Bài share
                                Libary.Instance.LogTech($"{Libary.IconOK}✅✅ Lấy time & link post thành công (Bài share)"
                                );
                            }
                            else
                            {
                                // ✅ Bài cơ bản
                                Libary.Instance.LogTech($"{Libary.IconOK}✅ Lấy time & link post thành công (Bài cơ bản)"
                                );
                            }
                        }
                        else
                        {
                            // ❌ Thiếu dữ liệu
                            Libary.Instance.LogTech($"{Libary.IconFail}❌ Không lấy đủ time/link post"
                            );
                        }
                        Libary.Instance.LogTech($" {Libary.IconOK}  🔹 lấy time/link bình thường");
                        Libary.Instance.LogTech($"   🔹 PostTime = {PostTime}", AppConfig.ENABLE_TECH_LOG);
                        Libary.Instance.LogTech($"   🔹 OriginalPostTime = {OriginalPostTime}", AppConfig.ENABLE_TECH_LOG);
                        Libary.Instance.LogTech($"   🔹 PostLink = {PostLink}", AppConfig.ENABLE_TECH_LOG);
                        Libary.Instance.LogTech($"   🔹 OriginalPostLink = {OriginalPostLink}", AppConfig.ENABLE_TECH_LOG);
                        //====== hết log=========
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogTech($"{Libary.IconFail}⚠️ [GetPostPageAsync] Lỗi PostTypeDetectorAsync, có time vẫn lỗi: {ex.Message}");
                        // Giữ giá trị mặc định để tránh null reference                   
                    }
                }
                try
                {
                    Libary.Instance.LogTech(Environment.NewLine +
                "-----------------------LẤY CHI TIẾT -----------------------------" + Environment.NewLine);
                    switch (timeList.Count)
                    {
                        case 1: // có 1 link 1 time thì là bài đăng
                            PosterName = pagename;
                            PosterLink = urlreal;
                            PosterNote = "FanPage";
                            if (postinfor.Count == 3 || postinfor.Count == 2)
                            {
                                if (postinfor.Count == 3)
                                {
                                    Libary.Instance.LogTech("Vào Postinfor3", AppConfig.ENABLE_TECH_LOG);
                                    var contentContainer = GetSafe(postinfor, 2);
                                    Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                    string ContentNomal = await GetContentTextAsync(page, contentContainer);
                                    if (!string.IsNullOrWhiteSpace(ContentNomal) && ContentNomal != "N/A")
                                    {
                                        Content = ContentNomal;
                                        Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content Nomal");
                                        PostType = PostTypeEnum.Page_Normal.ToString();
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech("Content trống, thử lấy bằng backgroud:", AppConfig.ENABLE_TECH_LOG);
                                        string ContentBackGround = await BackgroundTextAllAsync(page, post);
                                        if (!string.IsNullOrWhiteSpace(ContentBackGround) && ContentBackGround != "N/A")
                                        {
                                            Content = ContentBackGround;
                                            Libary.Instance.LogTech($"{Libary.IconOK} lấy bằng content BackGround");
                                            PostType = PostTypeEnum.Page_Photo_Cap.ToString();
                                        }
                                        else
                                        {
                                            PostType = PostTypeEnum.Page_Unknow.ToString();
                                        }
                                    }
                                }//else là 2 nền màu, ảnh
                                else
                                {
                                    Content = await BackgroundTextAllAsync(page, post);
                                    if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                    {
                                        PostType = PostTypeEnum.Page_BackGround.ToString();
                                        Libary.Instance.LogTech("chốt kiểu bài viết: " + PostType);
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                    }
                                    else
                                    {
                                        PostType = PostTypeEnum.Page_Unknow.ToString();
                                        Libary.Instance.LogTech($"{Libary.IconFail} Lỗi không lấy được nội dung bài viết background");
                                    }
                                }
                            }
                            else if (postinfor.Count == 4)
                            {
                                Libary.Instance.LogTech("Vào Postinfor 4", AppConfig.ENABLE_TECH_LOG);
                                var contentContainer = GetSafe(postinfor, 2);
                                Libary.Instance.LogTech("   🔹 Đang lấy nội dung...", AppConfig.ENABLE_TECH_LOG);
                                Content = await GetContentTextAsync(page, contentContainer);
                                // 🔁 Nếu không có nội dung thì fallback sang BackgroundTextAllAsync
                                if (string.IsNullOrWhiteSpace(Content) || Content == "N/A")
                                {
                                    Content = await BackgroundTextAllAsync(page, post);
                                    if (!string.IsNullOrWhiteSpace(Content) && Content != "N/A")
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy bài viết thành công background, số ký tự: " + Content.Length);
                                        PostType = PostTypeEnum.Page_Photo_Cap.ToString();
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail} Background k thấy nội dung");
                                        PostType = PostTypeEnum.Page_Unknow.ToString();
                                    }
                                }
                                else
                                {
                                    Libary.Instance.LogTech($"{Libary.IconFail} Bài viết gắn link ngoài k có content");
                                    PostType = PostTypeEnum.Page_Unknow.ToString();
                                }
                                Libary.Instance.LogTech("chốt kiểu bài viết: " + PostType);
                            }
                            (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                            if (PostLink != null && PostLink != "N/A")
                            {
                                postA.PostLink = PostLink;
                                postA.RealPostTime = TimeHelper.ParseFacebookTime(PostTime);
                                postA.PostTime = PostTime;
                                postA.PosterName = PosterName;
                                postA.PosterLink = PosterLink;
                                postA.PageName = pagename;
                                postA.PageLink = urlreal;
                                postA.Content = Content;
                                postA.PosterNote = PosterNote;
                                postA.ShareCount = ShareCount;
                                postA.CommentCount = CommentCount;
                                postA.LikeCount = LikeCount;
                                postA.PostType = PostType;
                            };
                            listPosts.Add(postA);
                            // ===== LOGTECH: KẾT QUẢ FANPAGE CASE 1 =====
                            bool hasContentCase = !string.IsNullOrWhiteSpace(Content) && Content != "N/A";
                            if (hasContentCase) Libary.Instance.LogTech($"{Libary.IconOK} Lấy content ok ký tự: {Content.Length}");
                            else Libary.Instance.LogTech($"{Libary.IconFail} Lấy content lỗi");
                            bool hasInteractCase = LikeCount + CommentCount + ShareCount > 0;
                            if (hasInteractCase) Libary.Instance.LogTech($"{Libary.IconOK} Lấy OK tương tác: 👍={LikeCount} 💬={CommentCount} 🔁={ShareCount}");
                            else Libary.Instance.LogTech($"{Libary.IconFail} Lấy tương tác lỗi");
                            Libary.Instance.LogTech($"{Libary.IconInfo} PostStatus: {PostType}");
                            break;
                        case 2:
                            {
                                PosterName = pagename;
                                PosterLink = urlreal;
                                PosterNote = "FanPage";
                                var contentContainer = GetSafe(postinfor, 2);
                                string ContentNormal = await GetContentTextAsync(page, contentContainer);
                                if (ContentNormal == "" || ContentNormal == "N/A")
                                {
                                    string ContentBackGroud = await BackgroundTextAllAsync(page, post);
                                    if (!string.IsNullOrEmpty(ContentBackGroud) || ContentBackGroud != "N/A")
                                    {
                                        Content = ContentBackGroud;
                                        PostType = PostTypeEnum.Page_BackGround.ToString();
                                        Libary.Instance.LogTech($"{Libary.IconOK} Lấy content fanpage BackGroud");
                                    }
                                }
                                else
                                {
                                    Libary.Instance.LogTech($"{Libary.IconOK} Lấy content fanpage nomarl");
                                    Content = ContentNormal;
                                    PostType = PostTypeEnum.Page_Normal.ToString();
                                }
                                (LikeCount, CommentCount, ShareCount) = await ExtractPostInteractionsAsync(post);
                                if (PostLink != null && PostLink != "N/A")
                                {
                                    postA.PostLink = PostLink;
                                    postA.RealPostTime = TimeHelper.ParseFacebookTime(PostTime);
                                    postA.PostTime = PostTime;
                                    postA.PosterName = PosterName;
                                    postA.PosterLink = PosterLink;
                                    postA.PageName = pagename;
                                    postA.PageLink = urlreal;
                                    postA.Content = Content;
                                    postA.PosterNote = PosterNote;
                                    postA.ShareCount = ShareCount;
                                    postA.CommentCount = CommentCount;
                                    postA.LikeCount = LikeCount;
                                    postA.PostType = PostType;
                                };
                                // ===== LOGTECH: KẾT QUẢ FANPAGE CASE 2 =====
                                bool hasContentCase2 = !string.IsNullOrWhiteSpace(Content) && Content != "N/A";
                                if (hasContentCase2) Libary.Instance.LogTech($"{Libary.IconOK} Lấy content ok ký tự: {Content.Length}");
                                else Libary.Instance.LogTech($"{Libary.IconFail} Lấy content lỗi");
                                bool hasInteractCase2 = LikeCount + CommentCount + ShareCount > 0;
                                if (hasInteractCase2) Libary.Instance.LogTech($"{Libary.IconOK} Lấy OK tương tác: 👍={LikeCount} 💬={CommentCount} 🔁={ShareCount}");
                                else Libary.Instance.LogTech($"{Libary.IconFail} Lấy tương tác lỗi");
                                Libary.Instance.LogTech($"{Libary.IconInfo} PostStatus: {PostType}");
                                listPosts.Add(postA);
                                if (OriginalPostLink != "N/A" && OriginalPosterLink != null)
                                {
                                    Libary.Instance.LogTech(Environment.NewLine +
              "-----------------------LẤY CHI TIẾT BÀI GỐC -----------------------------" + Environment.NewLine);
                                    postB = await GetPostOriginal(page, OriginalPostLink);
                                    Libary.Instance.LogTech(
                                        postB == null
                                ? $"❌ Bài gốc FAIL | Link={OriginalPostLink}"
                                 : $"✅ Bài gốc OK | Link={postB.PostLink}");
                                }

                                if (postB != null && postB.PostLink != "N/A")
                                {
                                    if (postA.PosterLink == OriginalPosterLink)
                                        postB.PostTime = postB.PostTime;
                                    listPosts.Add(postB);
                                    //table share
                                    listShares.Add(new ShareItem
                                    {
                                        PageLinkA = urlreal,
                                        PostLinkB = postB.PostLink,
                                        ShareTimeRaw = postA.PostTime,
                                        ShareTimeReal = TimeHelper.ParseFacebookTime(postA.PostTime)
                                    });
                                }
                                else
                                {
                                    Libary.Instance.LogTech($"{Libary.IconFail}[Caller] ❌ LẤY BÀI GỐC BẰNG GetPostOriginal lỗi");
                                }
                            }
                            break;
                        case 0:
                            {
                                Libary.Instance.LogTech($"{Libary.IconInfo}🟦 CASE 0 → Xử lý bài REEL (postinfor.Count = 0)");
                                try
                                {
                                    postA = await ExtractPostReelAll(page, post);
                                    if (postA != null && postA.PostLink != "N/A")
                                    {
                                        postA.PageName = pagename;
                                        Libary.Instance.LogTech($"{Libary.IconOK}🟩 Reel OK");
                                        listPosts.Add(postA);
                                    }
                                    else
                                    {
                                        Libary.Instance.LogTech($"{Libary.IconFail}🟥 ExtractPostReelAll return NULL → bỏ qua bài reel");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Libary.Instance.LogTech($"{Libary.IconFail}❌ Lỗi CASE 0 (Reel): {ex.Message}");
                                }
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogTech($"⚠️ Lỗi trong lấy chi tiết bài viết: {ex.Message}");
                }
                //if (string.IsNullOrEmpty(OriginalPostTime)) PostStatus = "Page Đăng/duyệt bài" + PostStatus;
                //else PostStatus = "Page share lại bài" + PostStatus;
                if (listPosts.Count() > 0) return new PostResult { Posts = listPosts, Shares = listShares }; ;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("[PAgeDAO] GetPostAutofanpage lỗi: " + ex.Message);
            }
            finally
            {
                Libary.Instance.LogTech(
                    $"[GetPostAutoFanpage] Kết thúc xử lý post | Posts={listPosts.Count}, Shares={listShares.Count}"
                );
            }
            return new PostResult { Posts = listPosts, Shares = listShares };
        }
     
        // kiểu thông tin người đăng khác, Đây là fallback dành riêng cho bài “video / reel / media post / ad unit”.
        public async Task<(string name, string link)> GetPosterFromProfileNameAsync(IElementHandle post)
        {
            try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo}Thử lấy poster bằng profilename");
                var profileDiv = await post.QuerySelectorAsync("div[data-ad-rendering-role='profile_name']");
                if (profileDiv == null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail}⚠️ Không tìm thấy profile_name trong post.");
                    return ("N/A", "N/A");
                }

                var linkEl = await profileDiv.QuerySelectorAsync("a[href]");
                if (linkEl == null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail}⚠️ Không tìm thấy thẻ a trong profile_name.");
                    return ("N/A", "N/A");
                }
                string name = (await linkEl.InnerTextAsync())?.Trim() ?? "N/A";
                string href = await linkEl.GetAttributeAsync("href") ?? "N/A";
                if (href != "N/A")
                    href = ProcessingDAO.Instance.ShortenPosterLink(href);
                else
                    href = "N/A";
                Libary.Instance.LogDebug($"{Libary.IconOK} 👤 Lấy từ profile_name: {name} | {href}");
                return (name, href);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [GetPosterFromProfileNameAsync] Lỗi: {ex.Message}");
                return ("N/A", "N/A");
            }
        }
        // hàm lấy link và người đăng video
        public async Task<(string postTime, string postLink)> HandleVideoPostAsync(IEnumerable<IElementHandle> postinfor)
        {
            string postTime = "N/A";
            string postLink = "N/A";

            try
            {
                foreach (var post in postinfor)
                {
                    // Lấy link video
                    var videoAnchors = await post.QuerySelectorAllAsync("a[href*='video']");
                    if (videoAnchors.Count > 0)
                    {
                        string href = await videoAnchors.First().GetAttributeAsync("href");
                        if (!string.IsNullOrEmpty(href))
                            postLink = href;
                    }

                    // Lấy thời gian đăng gần vùng video
                    string txt = (await post.InnerTextAsync())?.Trim() ?? "";
                    if (Regex.IsMatch(txt, @"(\d+\s*(giờ|phút|ngày|hôm qua|tháng))", RegexOptions.IgnoreCase))
                    {
                        postTime = TimeHelper.CleanTimeString(txt);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi trong HandleVideoPostAsync: " + ex.Message);
            }

            return (postTime, postLink);
        }//hàm lấy link và người đăng video
        // hàm chọn postinfor an toàn, tránh lỗi
        public IElementHandle GetSafe(IReadOnlyList<IElementHandle> list, int index)
        {
            if (index < list.Count)
                return list[index];

            throw new ArgumentOutOfRangeException($"postinfor không có phần tử thứ {index}.");
        }
       
        // xem thêm bài hiện popup
  
        // các hàm lấy content ảnh và caption reel
        public async Task<string> GetBackgroundTextAsync(IElementHandle post)
        {
            try
            {
                // 🎯 Tìm vùng có nền màu hoặc ảnh nền
                var bgContainer = await post.QuerySelectorAsync("div[style*='background-color'], div[style*='background-image']");
                if (bgContainer == null)
                    return "N/A";

                var spans = await bgContainer.QuerySelectorAllAsync("span, div[dir='auto']");
                var sb = new StringBuilder();

                foreach (var span in spans)
                {
                    string text = (await span.InnerTextAsync())?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(text))
                        sb.Append(text + " ");
                }

                string result = sb.ToString().Trim();

                // Nếu vẫn trống, thử lấy textContent fallback
                if (string.IsNullOrWhiteSpace(result))
                {
                    var raw = (await bgContainer.GetPropertyAsync("textContent"))?.ToString();
                    result = raw?.Trim('"') ?? "";
                }

                if (string.IsNullOrWhiteSpace(result))
                    return "N/A";

                Libary.Instance.LogDebug(
     $"{Libary.IconOK} [GetBackgroundTextAsync] Background text length={result?.Length ?? 0}");

                return result;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ Lỗi GetBackgroundTextAsync: {ex.Message}");
                return "N/A";
            }
        }
        // hÀM CON CỦA GETRELLaLL -> ĐANG DÙNG
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
        // ----------các hàm fallback lấy content/// dùng ở get cũ k auto
        // Helper: wrapper safe lấy poster info
        private async Task<(string name, string link)> SafeGetPosterInfoAsync(IElementHandle container)
        {
            if (container == null) return ("N/A", "N/A");
            try
            {
                var res = await GetPosterInfoBySelectorsAsync(container); // (name, link)
                if (string.IsNullOrWhiteSpace(res.name)) return ("N/A", "N/A");
                return res;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ SafeGetPosterInfoAsync lỗi: {ex.Message}");
                return ("N/A", "N/A");
            }
        }

        // Helper: wrapper safe lấy content từ container
     
        /*
        * GetPosterFullFallback:
        * - thử index 3, nếu không thì 2 (dùng GetSafe + GetPosterInfoBySelectorsAsync)
        * - nếu vẫn không có tên, fallback đọc span[class='xjp7ctv'] trong post (nếu có 2 giá trị thì lấy phần 1)
         */
        // lấy người đăng bài share
        public async Task<(string PosterName, string PosterLink)> GetPosterFullFallbackAsync(IPage page, IElementHandle post, IReadOnlyList<IElementHandle> postinfor)
        {
            try
            {
                // Thử index 3 rồi 2
                int[] tryIdx = new int[] { 3, 2 };
                foreach (var idx in tryIdx)
                {
                    var candidate = GetSafe(postinfor, idx);
                    var (name, link) = await SafeGetPosterInfoAsync(candidate);
                    if (name != "N/A")
                    {
                        Libary.Instance.CreateLog($" GetPosterFullFallbackAsync: lấy theo trường hợp lỗi");
                        return (name, ProcessingDAO.Instance.ShortenPosterLink(link));
                    }
                }

                // Fallback: đọc span.xjp7ctv từ post
                var spanEls = await post.QuerySelectorAllAsync("span[class='xjp7ctv']");
                var texts = new List<string>();
                foreach (var s in spanEls)
                {
                    var t = (await s.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrEmpty(t)) texts.Add(t);
                }

                if (texts.Count >= 1)
                {
                    // nếu có 2 giá trị thì theo bạn: giá trị 1 là người đăng
                    string posterName = texts[0];
                    Libary.Instance.CreateLog($"🔹 Poster fallback from span.xjp7ctv: {posterName}");
                    // link không rõ từ span => giữ "N/A"
                    return (posterName, "N/A");
                }

                Libary.Instance.CreateLog("🔹 GetPosterFullFallback: không tìm được poster");
                return ("N/A", "N/A");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ GetPosterFullFallback lỗi: {ex.Message}");
                return ("N/A", "N/A");
            }
        }

        /*
         * GetContentFallback:
         * - Quét post để lấy span có class dài (selector bạn cung cấp)
         * - Nếu có 2 giá trị -> return (value1 -> Content, value2 -> OriginalContent)
         * - Nếu có 1 giá trị -> return (value1, empty)
         * - Trả về tuple strings (content, originalContent)
         */
        public async Task<(string Content, string OriginalContent)> GetContentFallbackAsync(IPage page, IElementHandle post)
        {
            try
            {
                var selector = "span[class='x193iq5w xeuugli x13faqbe x1vvkbs xlh3980 xvmahel x1n0sxbx x1lliihq x1s928wv xhkezso x1gmr53x x1cpjm7i x1fgarty x1943h6x x4zkp8e x3x7a5m x6prxxf xvq8zen xo1l8bm xzsf02u x1yc453h']";
                var spans = await post.QuerySelectorAllAsync(selector);
                var list = new List<string>();
                foreach (var s in spans)
                {
                    var t = (await s.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrEmpty(t)) list.Add(t);
                }

                if (list.Count >= 2)
                {
                    // theo bạn: 1 là bài đăng, 2 là gốc
                    string c = list[0];
                    string oc = list[1];
                    // nếu có "Xem thêm" cần gọi GetFullContentAsync trên post để expand + read again
                    if (!string.IsNullOrEmpty(c) && c.IndexOf("Xem thêm", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            // gọi GetFullContentAsync để mở xem thêm; GetFullContentAsync nên trả toàn bộ text khi truyền post
                            var full = await GetFullContentAsync(page, post);
                            if (!string.IsNullOrWhiteSpace(full))
                            {
                                return (full, oc);
                            }
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.CreateLog($"⚠️ GetContentFallback -> GetFullContentAsync lỗi: {ex.Message}");
                        }
                    }
                    return (c, oc);
                }
                else if (list.Count == 1)
                {
                    var only = list[0];
                    if (!string.IsNullOrEmpty(only) && only.IndexOf("Xem thêm", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            var full = await GetFullContentAsync(page, post);
                            return (string.IsNullOrWhiteSpace(full) ? only : full, string.Empty);
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.CreateLog($"⚠️ GetContentFallback (1) -> GetFullContentAsync lỗi: {ex.Message}");
                            return (only, string.Empty);
                        }
                    }
                    return (only, string.Empty);
                }

                // Không tìm được span phù hợp => trả rỗng để caller tiếp fallback
                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ GetContentFallback lỗi: {ex.Message}");
                return (string.Empty, string.Empty);
            }
        }
        /*
 * GetContentFullFallback:
 * - logic:
 *    * thử content ở index 2 (maybeContent)
 *    * nếu có -> Content = maybeContent, cố gắng lấy OriginalContent ở index 4 (theo cấu trúc 5 node)
 *    * nếu không -> thử lấy OriginalContent ở index 4 (theo code cũ index 5/4 tùy)
 *    * nếu vẫn không -> gọi GetContentFallbackAsync(post)
 *    * nếu vẫn không -> BackgroundTextAllAsync(page, post)
 * - trả về (Content, OriginalContent, PostStatus)
 */
        public async Task<(string Content, string OriginalContent, string PostStatus)> GetContentFullFallbackAsync(IPage page, IElementHandle post, IReadOnlyList<IElementHandle> postinfor)
        {
            string Content = string.Empty;
            string OriginalContent = string.Empty;
            string PostStatus = "N/A";

            try
            {
                // 1) thử lấy content người chia sẻ ở index 2
                var maybeContentContainer = GetSafe(postinfor, 2);
                string possibleContent = await GetContentTextAsync(page, maybeContentContainer);
                bool hasUserContent = !string.IsNullOrWhiteSpace(possibleContent);

                if (hasUserContent)
                {
                    Content = possibleContent;
                    PostStatus = "bài share (người chia sẻ có viết)";
                    // thử lấy original ở index 4 (thường cấu trúc 5 node)
                    var maybeOriginal = GetSafe(postinfor, 4);
                    if (maybeOriginal != null)
                    {
                        OriginalContent = await GetContentTextAsync(page, maybeOriginal);
                        if (!string.IsNullOrWhiteSpace(OriginalContent))
                            Libary.Instance.CreateLog("🔹 Lấy được OriginalContent ở index 4");
                    }
                    // nếu original vẫn trống, fallback bằng GetContentFallback
                    if (string.IsNullOrWhiteSpace(OriginalContent))
                    {
                        // 1️⃣ Thử lấy nền màu trước
                        string bg = await BackgroundTextAllAsync(page, post);

                        if (!string.IsNullOrWhiteSpace(bg))
                        {
                            OriginalContent = bg;
                            Libary.Instance.CreateLog("🔹 OriginalContent lấy từ nền màu (BackgroundTextAllAsync)");
                        }
                        else
                        {
                            // 2️⃣ Nếu nền màu rỗng → fallback cuối
                            var (c, oc) = await GetContentFallbackAsync(page, post);

                            if (!string.IsNullOrWhiteSpace(oc))
                            {
                                OriginalContent = oc;
                                Libary.Instance.CreateLog("🔹 Fallback: lấy OriginalContent từ GetContentFallback");
                            }
                        }
                    }
                }
                else
                {
                    // 2) nếu người chia sẻ không viết => bài gốc có nội dung ở index 4 (theo ý bạn)
                    var maybeOriginalContentContainer = GetSafe(postinfor, 4);
                    OriginalContent = await GetContentTextAsync(page, maybeOriginalContentContainer);
                    if (!string.IsNullOrWhiteSpace(OriginalContent))
                    {
                        Libary.Instance.CreateLog("bài share (gốc có content, người chia sẻ không viết)");
                    }
                    else
                    {
                        // 3) fallback: thử index 5 (nếu bạn trước dùng 5)
                        var maybeOriginalIndex5 = GetSafe(postinfor, 5);
                        OriginalContent = await GetContentTextAsync(page, maybeOriginalIndex5);
                        if (!string.IsNullOrWhiteSpace(OriginalContent))
                        {
                            Libary.Instance.CreateLog("bài share (gốc có content ở index 5)");
                        }
                    }
                    // 4) nếu vẫn rỗng cả -> gọi GetContentFallback (quét toàn post)
                    if (string.IsNullOrWhiteSpace(OriginalContent))
                    {
                        var (c, oc) = await GetContentFallbackAsync(page, post);
                        if (!string.IsNullOrWhiteSpace(c) || !string.IsNullOrWhiteSpace(oc))
                        {
                            Content = string.IsNullOrWhiteSpace(c) ? Content : c;
                            OriginalContent = string.IsNullOrWhiteSpace(oc) ? OriginalContent : oc;                          
                            Libary.Instance.CreateLog("🔹 Fallback tổng thể: lấy content từ GetContentFallback");
                        }
                    }
                }

                // 5) Nếu vẫn không có gì -> BackgroundTextAllAsync (cuối cùng)
                if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(OriginalContent))
                {
                    var bg = await BackgroundTextAllAsync(page, post);
                    if (!string.IsNullOrWhiteSpace(bg))
                    {
                        Content = bg;
                        Libary.Instance.CreateLog("fallback nền màu / background");
                        Libary.Instance.CreateLog("🔹 BackgroundTextAllAsync thành công");
                    }
                    else
                    {
                       
                        Libary.Instance.CreateLog("🔹 Không lấy được content bằng bất kỳ phương pháp nào");
                    }
                }              
                return (Content.Trim(), OriginalContent.Trim(), PostStatus);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"⚠️ GetContentFullFallback lỗi: {ex.Message}");
                return (string.Empty, string.Empty, "lỗi");
            }
        }

        //-------------------------------------------//////////////
        //-------GetPostTextAsyn: LẤy nhanh Text, fallback
        public async Task<string> GetPostTextAsync(IElementHandle postinfor)
        {
            try
            {
                // 🎯 Thử tìm các div chứa nội dung caption gốc
                var textDivs = await postinfor.QuerySelectorAllAsync(
                    "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
                );

                var sb = new StringBuilder();

                // ✅ Trường hợp tìm được div nội dung
                if (textDivs != null && textDivs.Count > 0)
                {
                    foreach (var div in textDivs)
                    {
                        string inner = (await div.InnerTextAsync())?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(inner))
                        {
                            sb.AppendLine(inner);
                        }
                    }
                }

                string result = sb.ToString().Trim();

                // ⚠️ Nếu không có text hoặc rỗng, thử fallback qua span[dir=auto]
                if (string.IsNullOrWhiteSpace(result))
                {
                    Libary.Instance.CreateLog("⚠️ Không có nội dung từ class xdj266r..., thử fallback span[dir=auto].");

                    var spanElements = await postinfor.QuerySelectorAllAsync("span[dir='auto']");
                    var spanText = new StringBuilder();

                    foreach (var span in spanElements)
                    {
                        string inner = (await span.InnerTextAsync())?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(inner))
                        {
                            // Loại bỏ các ký tự lặp không cần thiết
                            if (!spanText.ToString().Contains(inner))
                                spanText.AppendLine(inner);
                        }
                    }

                    result = spanText.ToString().Trim();
                }

                // ✅ Kiểm tra lần cuối
                if (string.IsNullOrWhiteSpace(result))
                {
                    return "N/A";
                }
                return result;
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetPostTextAsync: {ex.Message}");
                return "N/A";
            }
        }
     
        // hàm lấy bài ẩn hrel, click vào===========REEL---------------
        public async Task<(string postTime, string postLink,string originalTime, string originalLink,string posterName,string posterLink,string posterNote)>ExtractFallback(IPage page, IElementHandle post)
        {
            string postTime = "N/A";
            string postLink = "N/A";
            string originalTime = "N/A";
            string originalLink = "N/A";
            string posterName = "N/A";
            string posterLink = "N/A";
            string posterNote = "N/A";

            try
            {
                Libary.Instance.LogDebug("[ExtractFallback] ");
                var aTags = await post.QuerySelectorAllAsync("a[href]");
                var timeLinks = new List<(string time, string href)>();

                foreach (var a in aTags)
                {
                    string href = await a.GetAttributeAsync("href") ?? "";
                    string text = (await a.InnerTextAsync() ?? "").Trim();
                    if (string.IsNullOrEmpty(href) || string.IsNullOrEmpty(text))
                        continue;
                    // =============== DETECT POSTER ONLY (CHỈ /user/) ===============
                    if (href.Contains("/user/") && posterLink == "N/A")
                    {
                        posterName = text;
                        posterLink = ProcessingDAO.Instance.ShortenPosterLink(href);
                    }
                    // 1) Bỏ qua các link nhiễu
                    if (href.IndexOf("comment_id=", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        href.IndexOf("/story/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        href.IndexOf("story_fbid", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Libary.Instance.LogDebug("[Fallback] SKIP noise link: " + href);
                        continue;
                    }
                    // 2) Kiểm tra link bài thật
                    bool isRealPost =
                        Regex.IsMatch(href, @"facebook\.com/.+/posts/\d+([^0-9]|$)", RegexOptions.IgnoreCase) ||
                        href.IndexOf("/reel/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        href.IndexOf("/permalink/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        href.IndexOf("photo.php?fbid=", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!isRealPost)
                        continue;                  
                    // detect time text
                    string lower = text.ToLower();

                    bool isTime =
                        lower.Contains("phút") ||
                        lower.Contains("giờ") ||
                        lower.Contains("hôm nay") ||
                        lower.Contains("hôm qua") ||
                        lower.Contains("ngày") ||
                        Regex.IsMatch(lower, @"\d+\s*(phút|giờ|ngày)", RegexOptions.IgnoreCase);

                    if (isTime)
                    {
                        string cleanTime = TimeHelper.CleanTimeString(text);
                        string shortLink = ProcessingHelper.ShortenFacebookPostLink(href);
                        timeLinks.Add((cleanTime, shortLink));
                    }
                }

                // ========== GÁN TIME/LINK ==========
                if (timeLinks.Count >= 1)
                {
                    postTime = timeLinks[0].time;
                    postLink = timeLinks[0].href;
                }

                if (timeLinks.Count >= 2)
                {
                    originalTime = timeLinks[1].time;
                    originalLink = timeLinks[1].href;
                }

                // fallback click nếu không có time
                if (timeLinks.Count == 0)
                {
                    Libary.Instance.LogDebug($"[TRACE-FALLBACK] Click timestamp to open link ");
                    (postTime, postLink) = await ExtractPostLinkByClickAsync(page, post);
                }

                // ========== CHECK TYPE ngay tại fallback ==========
                if (posterLink != "N/A")
                {
                    var fbType = await CheckTypeCachedAsync(page, posterLink);
                    posterNote = fbType.ToString();
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ ExtractFallbackV2 lỗi: {ex.Message}");
            }

            return (postTime, postLink, originalTime, originalLink, posterName, posterLink, posterNote);
        }
        public async Task<(string postTime, string postLink)> ExtractPostLinkByClickAsync(IPage mainPage, IElementHandle post)
        {
            string postTime = "N/A";
            string postLink = "N/A";

            try
            {
                // 🔹 Tìm tất cả span có khả năng chứa thời gian
                var timeSpans = await post.QuerySelectorAllAsync(
                    "span.xdj266r.x14z9mp.xat24cr.x1lziwak span, " +
                    "span.xdj266r.x14z9mp.xat24cr.x1lziwak"
                );

                IElementHandle timeSpan = null;
                foreach (var span in timeSpans)
                {
                    string text = (await span.InnerTextAsync() ?? "").Trim();
                    if (Regex.IsMatch(text, @"(\d+)\s*(phút|giờ|ngày|tháng)|lúc\s*\d", RegexOptions.IgnoreCase))
                    {
                        timeSpan = span;
                        postTime = text;
                        break;
                    }
                }

                if (timeSpan == null)
                {
                    Libary.Instance.CreateLog("⚠️ Không tìm thấy span chứa thời gian để click fallback.");
                    return (postTime, postLink);
                }

                Libary.Instance.CreateLog($"🕒 Fallback click span thời gian '{postTime}'.");

                // Cuộn vào vùng nhìn thấy trước khi click
                await timeSpan.ScrollIntoViewIfNeededAsync();
                await PageDAO.Instance.RandomDelayAsync(mainPage, 300, 500);
                // Tìm phần tử cha có role='button' hoặc 'link' (thường là clickable container)
                var clickable = await timeSpan.QuerySelectorAsync("xpath=ancestor::*[@role='button' or @role='link'][1]");
                if (clickable == null) clickable = timeSpan;
                // ================================================
                // 🧩 LỚP 1: Ctrl + Click (mở tab mới nếu được)
                // ================================================
                var popupTask = mainPage.Context.WaitForPageAsync();
                await mainPage.EvaluateAsync(@"(el) => {
            const ev = new MouseEvent('click', {
                bubbles: true,
                cancelable: true,
                ctrlKey: true
            });
            el.dispatchEvent(ev);
        }", clickable);

                // Chờ tối đa 8 giây cho tab mới bật ra
                Task<IPage> newPageTask = popupTask;
                Task finished = await Task.WhenAny(new Task[] { newPageTask, Task.Delay(8000) });

                if (object.ReferenceEquals(finished, newPageTask))
                {
                    IPage newPage = await newPageTask;
                    await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                    postLink = newPage.Url;
                    await newPage.CloseAsync();

                    Libary.Instance.CreateLog($"✅ Mở tab mới (Ctrl+Click) thành công: {postLink}");
                    return (postTime, postLink);
                }

                // ================================================
                // 🧩 LỚP 2: Nếu Ctrl+Click bị chặn → dùng window.open()
                // ================================================
                Libary.Instance.CreateLog("⚠️ Ctrl+Click bị chặn → fallback window.open()");

                string urlFromScript = await mainPage.EvaluateAsync<string>(@"(el) => {
            // Ưu tiên href thật (nếu tồn tại)
            const link = el.closest('a')?.href;
            if (link) {
                window.open(link, '_blank');
                return link;
            }

            // Nếu không có href, tìm link ẩn trong node con
            let found = null;
            const walk = (node) => {
                if (!node) return;
                if (node.nodeType === 1 && node.hasAttribute('href')) found = node.getAttribute('href');
                if (!found && node.childNodes) Array.from(node.childNodes).forEach(walk);
            };
            walk(el);

            if (found) {
                window.open(found, '_blank');
                return found;
            }

            return 'N/A';
        }", clickable);

                if (!string.IsNullOrEmpty(urlFromScript) && urlFromScript != "N/A")
                {
                    var newPage2Task = mainPage.Context.WaitForPageAsync();
                    Task finished2 = await Task.WhenAny(new Task[] { newPage2Task, Task.Delay(8000) });
                    await PageDAO.Instance.RandomDelayAsync(mainPage, 500, 1000);
                    if (object.ReferenceEquals(finished2, newPage2Task))
                    {
                        IPage newPage2 = await newPage2Task;
                        await newPage2.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        postLink = newPage2.Url;
                        await newPage2.CloseAsync();

                        Libary.Instance.CreateLog($"✅ Fallback window.open() lấy link: {postLink}");
                        return (postTime, postLink);
                    }

                    Libary.Instance.CreateLog("⚠️ Timeout 8s chờ tab mới (window.open không bật tab).");
                }

                Libary.Instance.CreateLog("⚠️ Không lấy được link bằng cả Ctrl+Click lẫn window.open.");
                return (postTime, postLink);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi ExtractPostLinkFromTimeSpanAsync: {ex.Message}");
                return (postTime, postLink);
            }
        }
        // hàm xử lý mở bài share lấy tương tác
        // tab phụ đang lưu ở đây xem thế nào
        //== HÀM CON CỦA EXTRALLFULLREEL --> ĐANG DÙNG
       
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
        public async Task<(int like, int comment, int share)> ExtractWatchVideoInteractionsAsync(IPage page)
        {
            int likes = 0, comments = 0, shares = 0;

            try
            {
                Libary.Instance.CreateLog("[Watch] 🎥 Bắt đầu lấy tương tác video...");

                // like / comment / share có chữ → dùng ParseFacebookNumber
                var buttons = await page.QuerySelectorAllAsync("div[aria-label]");
                foreach (var b in buttons)
                {
                    string label = (await b.GetAttributeAsync("aria-label"))?.ToLower() ?? "";
                    string txt = (await b.InnerTextAsync() ?? "").Trim().ToLower();

                    if (label.Contains("thích") || label.Contains("like"))
                        likes = ProcessingHelper.ParseFacebookNumber(txt);

                    if (label.Contains("bình luận") || label.Contains("comment"))
                        comments = ProcessingHelper.ParseFacebookNumber(txt);

                    if (label.Contains("chia sẻ") || label.Contains("share"))
                        shares = ProcessingHelper.ParseFacebookNumber(txt);
                }

                Libary.Instance.CreateLog($"[Watch] ✅ Video: Like={likes}, Comment={comments}, Share={shares}");
                return (likes, comments, shares);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ [Watch] Lỗi đọc video: {ex.Message}");
                return (likes, comments, shares);
            }
        }
        public async Task<(int like, int comment, int share)>OpenLinkInNewTabForInteractionsAsync(IPage mainPage, string url)
        {
            IPage newPage = null;

            int like = 0, comment = 0, share = 0;

            try
            {
                Libary.Instance.LogDebug($"[OpenLinkInNewTabForInteractionsAsync] 🔗 Mở: {url}");

                var popupTask = mainPage.Context.WaitForPageAsync();

                // mở tab thật giống người dùng
                await mainPage.EvaluateAsync($"window.open('{url}', '_blank');");

                var finished = await Task.WhenAny(popupTask, Task.Delay(6000));

                if (finished != popupTask)
                {
                    Libary.Instance.LogDebug("[OpenLinkInNewTabForInteractionsAsync] ⚠️ Không mở tab → có thể là popup.");
                    await ClosePostPopupAsync(mainPage);
                    return (0, 0, 0);
                }

                newPage = await popupTask;
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(1200);
                await newPage.WaitForTimeoutAsync(500);
                string u = newPage.Url.ToLower();

                if (u.Contains("/reel/"))
                {
                    Libary.Instance.LogDebug("[OpenLinkInNewTabForInteractionsAsync] 🎬 Reel");
                    (like, comment, share) = await ExtractReelInteractionsAsync(newPage);
                }
                else if (u.Contains("/watch") || u.Contains("?v="))
                {
                    Libary.Instance.LogDebug("[OpenLinkInNewTabForInteractionsAsync] 🎥 Watch");
                    (like, comment, share) = await ExtractWatchVideoInteractionsAsync(newPage);
                }
                else
                {
                    Libary.Instance.LogDebug("[OpenLinkInNewTabForInteractionsAsync] 📝 Post thường");
                    var post = await newPage.QuerySelectorAsync("div[role='article']");
                    (like, comment, share) = await ExtractPostInteractionsAsync(post);
                }

                Libary.Instance.LogDebug($"[OpenLinkInNewTabForInteractionsAsync] ✅ Like={like}, Cmt={comment}, Share={share}");
                return (like, comment, share);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [OpenLinkInNewTabForInteractionsAsync] Lỗi: {ex.Message}");
                return (0, 0, 0);
            }
            finally
            {
                // đóng popup tab phụ nếu có
                if (newPage != null)
                {
                    try
                    {
                        await newPage.CloseAsync();
                        Libary.Instance.LogDebug("[OpenTab] 🧹 Tab phụ đã đóng.");
                    }
                    catch { }
                }
            }
        }
     
        public async Task<FBType> CheckFBTypeAsyncOld(IPage tab)
        {
            bool isGroup = false;
            bool hasFriends = false;
            bool hasFollow = false;

            string url = tab.Url;
            Libary.Instance.LogDebug($"[CheckFBType] ▶ CHECK = {url}");

            // ==========================================
            // 1. GROUP (ưu tiên cao nhất)
            // ==========================================
            if (url.IndexOf("/groups/", StringComparison.OrdinalIgnoreCase) >= 0)
                isGroup = true;

            var groupDiv = await tab.QuerySelectorAsync("div[class*='x193iq5w']");
            if (groupDiv != null)
            {
                string txt = (await groupDiv.InnerTextAsync() ?? "").ToLower();
                if (txt.Contains("nhóm"))
                    isGroup = true;
            }
            if (isGroup)
            {
                // Lấy toàn bộ text trong body
                var body = await tab.QuerySelectorAsync("body");
                string allText = (await body.InnerTextAsync() ?? "").ToLower();

                bool isPrivate =
                    allText.Contains("nhóm riêng tư") ||
                    allText.Contains("riêng tư");
                bool isPublic =
                    allText.Contains("nhóm công khai") ||
                    allText.Contains("công khai");
                if (isPrivate && !isPublic)
                {
                    Libary.Instance.LogDebug("[CheckFBType] 🔒 FINAL = GROUP-OFF (Riêng tư)");
                    return FBType.GroupOff;
                }
                else
                {
                    Libary.Instance.LogDebug("[CheckFBType] 🔓 FINAL = GROUP-ON (Công khai)");
                    return FBType.GroupOn;
                }
            }
            // ==========================================
            // 2. PERSON (người bạn)
            // ==========================================
            var personDiv = await tab.QuerySelectorAsync("div[aria-orientation='horizontal']");
            if (personDiv != null)
            {
                var innerText = await personDiv.InnerTextAsync();
                hasFriends = innerText != null && innerText.Contains("Bạn bè");
            }         
            if (hasFriends)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟩 PERSON: 'người bạn'");
            }        
            // ==========================================
            // 3. PERSON fallback — có link /friends/
            // ==========================================
            var friendA = await tab.QuerySelectorAsync("a[href*='/friends']");
            if (friendA != null)
            {
                hasFriends = true;
                Libary.Instance.LogDebug("[CheckFBType] 🟩 PERSON: link /friends/");
            }

            // ==========================================
            // 4. FOLLOW — detect Fanpage hoặc KOL
            // ==========================================
            var followBtn = await tab.QuerySelectorAsync(
                "div[aria-label*='Theo dõi'], button[aria-label*='Theo dõi'], div[role='button']:has-text('Theo dõi')");
            if (followBtn != null)
            {
                hasFollow = true;
                Libary.Instance.LogDebug("[CheckFBType] 🟨 FOLLOW: found button");
            }

            // text fallback
            var allBtn = await tab.QuerySelectorAllAsync("button, div[role='button'], span");
            foreach (var b in allBtn)
            {
                string t = (await b.InnerTextAsync())?.Trim().ToLower();
                if (!string.IsNullOrEmpty(t) &&
                    (t.Contains("theo dõi") || t.Contains("follow")))
                {
                    hasFollow = true;
                    Libary.Instance.LogDebug($"[CheckFBType] 🟨 FOLLOW TEXT: '{t}'");
                }
            }
            var span = await tab.QuerySelectorAsync("span[class='x193iq5w xeuugli x13faqbe x1vvkbs x1xmvt09 x6prxxf xvq8zen xo1l8bm xzsf02u']");
            bool IsKOL = false;
            if (span != null)
            {
                var text = (await span.InnerTextAsync() ?? "").Trim();

                if (text.Contains("Người sáng tạo nội dung số"))
                {
                    IsKOL = true;
                    Libary.Instance.LogDebug("[CheckFBType] 🟩 KOL DETECTED - Người sáng tạo nội dung số");
                }
            }
            // ==========================================
            // 6. CHỐT KẾT QUẢ
            // ==========================================
            // Person thường
            if (hasFriends && !hasFollow)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟩 FINAL = PERSON");
                return FBType.Person;
            }

            // Person KOL (người FOLLOW ONLY, trang cá nhân)
            if ((hasFriends && hasFollow) || IsKOL)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟧 FINAL = PERSON-KOL");

                return FBType.PersonKOL;
            }

            // Fanpage
            if (!hasFriends && hasFollow )
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟨 FINAL = FANPAGE");
                return FBType.Fanpage;
            }

            // Person-KOL fallback (không có “người bạn”, không có “friends”, nhưng có follow)
            if (hasFriends && hasFollow)
            {
                Libary.Instance.LogDebug("[CheckFBType] 🟧 FINAL = PERSON-KOL (fallback)");
                return FBType.PersonKOL;
            }

            Libary.Instance.LogDebug("[CheckFBType] ❓ FINAL = UNKNOWN");
            return FBType.Unknown;
        }
     
        //---------------------bài share ở dưới mở popup ấy
        private async Task<string> ExtractPopupContentAsync(IPage page, IElementHandle postDiv)
        {
            if (postDiv == null)
                return "N/A";
            string content = "";
            // 1) Lấy children
            try
            {
                var children = await postDiv.QuerySelectorAllAsync(":scope > div");
                IElementHandle contentContainer = null;
                // 2) Nếu có ≥ 3 phần tử → thẻ cuối là caption    
                if (children != null && children.Count == 3)
                {
                    contentContainer = children[children.Count - 1];
                    Libary.Instance.CreateLog("[CONTENT] Dùng children[last] làm content.");
                }            
                // 3) Nếu có contentContainer
                if (contentContainer != null)
                {
                    // try click xem thêm
                    var seeMore = await contentContainer.QuerySelectorAsync(
                        "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                    );

                    if (seeMore != null)
                    {
                        Libary.Instance.CreateLog("[CONTENT] Click 'Xem thêm'");
                        try { await seeMore.ClickAsync(); } catch { }
                        await page.WaitForTimeoutAsync(200);
                    }

                    // lấy full text
                    content = await contentContainer.InnerTextAsync() ?? "";
                    content = content.Trim();
                }
                if (content == "")
                {
                    // 4) FALLBACK — div caption chuẩn
                    var fbCaptionDivs = await postDiv.QuerySelectorAllAsync(
                        "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
                    );

                    if (fbCaptionDivs != null && fbCaptionDivs.Count > 0)
                    {
                        var sb = new StringBuilder();
                        foreach (var d in fbCaptionDivs)
                        {
                            var t = await d.InnerTextAsync();
                            if (!string.IsNullOrWhiteSpace(t))
                                sb.AppendLine(t.Trim());
                        }
                        string result = sb.ToString().Trim();
                        if (result != "") content = result;                    
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [ExtractPopupContentAsync] Không tìm thấy content: {ex.Message}"
                );
                return "N/A"; // ⭐ BẮT BUỘC
            }
            if (!string.IsNullOrWhiteSpace(content))
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconOK} [ExtractPopupContentAsync] Lấy thành công content bài share (length={content.Length})"
                );
                return content; // ⭐ BẮT BUỘC
            }
            return "N/A"; // ⭐ fallback cuối
        }
       
        // HÀM BỔ TRỢ LẤY GỐC, ĐÓNG TAB POPUP AN TOÀN
        private async Task SafeClosePageAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return;

            try
            {
                await page.CloseAsync(new PageCloseOptions
                {
                    RunBeforeUnload = true
                });

                // chờ context cập nhật lại page list
                await Task.Delay(100);
            }
            catch
            {
                // không throw – tránh ảnh hưởng flow crawl
            }
        }     
        public async Task CloseAllPopupsSafeAsync(IPage mainPage)
        {
            var context = mainPage.Context;

            foreach (var page in context.Pages.ToList())
            {
                if (page == mainPage)
                    continue;

                try
                {
                    Libary.Instance.LogDebug($"{Libary.IconWarn} Đóng popup còn sót");

                    await page.CloseAsync(new PageCloseOptions
                    {
                        RunBeforeUnload = true
                    });

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogDebug(
                        $"{Libary.IconWarn} Popup không đóng được: {ex.Message}"
                    );
                }
            }

            Libary.Instance.LogDebug(
                $"{Libary.IconOK} Popup cleanup xong | Pages={context.Pages.Count}"
            );
        }

        //CÁC HÀM PHỤ
        //hàm delay chờ
        public async Task RandomDelayAsync(IPage page, int min, int max)
        {
            try
            {
                if (page == null) return;

                if (min < 0) min = 0;
                if (max <= min) max = min + 1;

                int delay = _random.Next(min, max);

                // Ghi log theo module "Delay"
                Libary.Instance.CreateLog("Service", $"⏱ Random delay: {delay} ms (min={min}, max={max})");

                await page.WaitForTimeoutAsync(delay);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("Delay", $"❌ RandomDelayAsync lỗi: {ex.Message}");
            }
        }
        //----HÀM BỔ TRỢ VIẾT LOG---------
        public string SummarizeOriginalPost(PostPage p)
        {
            if (p == null)
                return $"{Libary.IconFail} Không lấy được bài gốc";

            bool hasTime =
                !string.IsNullOrWhiteSpace(p.PostTime) && p.PostTime != "N/A";

            bool hasContent =
                !string.IsNullOrWhiteSpace(p.Content) && p.Content != "N/A";

            bool hasInteract =
                p.LikeCount > 0 || p.CommentCount > 0 || p.ShareCount > 0;

            bool hasPageName =
                !string.IsNullOrWhiteSpace(p.PageName);

            return
                $"Bài gốc: " +
                $"TimeRaw={Libary.BoolIcon(hasTime)}: {p.PostTime} , " +
                $"Content={Libary.BoolIcon(hasContent)}, " +
                $"Interact={Libary.BoolIcon(hasInteract)}, " +
                $"PageName={Libary.BoolIcon(hasPageName)}: {p.PageName}";
        }

        public async Task<bool> ClosePostPopupAsync(IPage page)
        {
            try
            {
                // 1️⃣ Kiểm tra có popup không
                var dialog = await page.QuerySelectorAsync("div[role='dialog']");
                if (dialog == null)
                    return false;

                Libary.Instance.LogDebug($"{Libary.IconWarn} Popup phát hiện, đang đóng");

                // 2️⃣ ESC trước (ổn định nhất)
                await page.Keyboard.PressAsync("Escape");
                await page.WaitForTimeoutAsync(300);

                // 3️⃣ Nếu còn → click nút X
                dialog = await page.QuerySelectorAsync("div[role='dialog']");
                if (dialog != null)
                {
                    var closeBtn = await dialog.QuerySelectorAsync(
                        "div[aria-label='Đóng'], div[aria-label='Close']"
                    );

                    if (closeBtn != null)
                    {
                        await closeBtn.ClickAsync();
                        await page.WaitForTimeoutAsync(300);
                    }
                }

                // 4️⃣ Kiểm tra lại
                dialog = await page.QuerySelectorAsync("div[role='dialog']");
                if (dialog == null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconOK} Popup đã đóng hoàn toàn");
                    return true;
                }

                Libary.Instance.LogDebug($"{Libary.IconFail} Popup vẫn còn sau khi đóng");
                return false;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} Lỗi đóng popup: {ex.Message}");
                return false;
            }
        }
        public bool IsFacebookGroup(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // chuẩn hóa
            url = url.ToLower().Trim();

            // Nếu chứa /groups/ → chắc chắn là group
            return url.Contains("/groups/");
        }
        //==========CÁC HÀM REEL THỪA HƯỞNG TỪ PERSON POST
        public async Task<(bool hasReel, string reelLink)> DetectReelFromPostAsync(IElementHandle post)
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
                        Libary.Instance.LogDebug($"[REEL] ✅ Phát hiện Reel link: {href}");
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
    }
}
