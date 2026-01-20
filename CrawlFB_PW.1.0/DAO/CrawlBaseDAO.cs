using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.ViewModels;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
namespace CrawlFB_PW._1._0.DAO
{
    public class CrawlBaseDAO
    {
        public CrawlBaseDAO() { }
        private static CrawlBaseDAO _instance;
        public static CrawlBaseDAO Instance
        {
            get
            {
                if (_instance == null) _instance = new CrawlBaseDAO();
                return _instance;
            }
        }
        private readonly Dictionary<string, (FBType Type, string IdFB)> CheckedTypeCache = new Dictionary<string, (FBType, string)>();


        //1.*==THỜI GIAN VÀ LINK THEO POSTINFOR
        //1.1 lẤY LIST THỜI GIAN VÀ LINK
        public async Task<(List<string> timeList, List<string> linkList)> ExtractTimeAndLinksAsync(IEnumerable<IElementHandle> postinfor)
        {
            var timeList = new List<string>();
            var linkList = new List<string>();
            var addedLinks = new HashSet<string>();

            int index = 0;
            Libary.Instance.LogDebug(Libary.StartPost($" PHÂN TÍCH CHI TIẾT POST"));
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

        //1.2 TỪ LIST LẤY TIME VÀ LINK
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

        //2*. NGƯỜI ĐĂNG 
        //2.1 LẤY THEO PROFILENAME - ĐẦU VÀO POSTNODE
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
        //2.2 --------------hàm lấy thông tin người đăng theo selector------------------đầu vào posinfor
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

        //3*** LẤY CONTENT
        //3.1 NORLMAL nếu k lấy được nội dung---thay thế div hàm dưới => HÀM HIỆN TẠI ĐANG DÙNG
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
                var seeMoreBtn = await container.QuerySelectorAsync("div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                );

                if (seeMoreBtn != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Tìm thấy 'Xem thêm' tròn bài viết");
                    // Kiểm tra kích thước nút có bị che không
                    var bbox = await seeMoreBtn.BoundingBoxAsync();
                    if (bbox != null && bbox.Width > 5 && bbox.Height > 5)
                    {
                        // Scroll đúng vào node – không dùng scroll random
                        await page.EvaluateAsync("el => el.scrollIntoView({behavior:'instant', block:'center'})",
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

        //3.2  HÀM LẤY BACKGROUND => HIỆN TẠI ĐANG DÙNG
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
        //4. HÀM CHECKTYPE
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
        public async Task<(FBType Type, string IdFB)> CheckTypeCachedAsync(
    IPage mainPage,
    string url,
    string knownIdFB = null)
        {
            // =========================
            // 0️⃣ ƯU TIÊN IDFB (DB)
            // =========================
            if (!string.IsNullOrWhiteSpace(knownIdFB))
            {
                // thử PAGE theo IDFB
                var pageType = SQLDAO.Instance.GetPageTypeByID(knownIdFB);
                if (pageType != FBType.Unknown)
                    return (pageType, knownIdFB);

                // thử PERSON theo IDFB
                var personType = SQLDAO.Instance.GetPersonNoteByID(knownIdFB);
                if (personType != FBType.Unknown)
                    return (personType, knownIdFB);
            }
            var pageByLink = SQLDAO.Instance.GetPageTypeIdFbByLink(url);
            if (pageByLink.HasValue)
            {
                return pageByLink.Value; // (FBType, IdFB)
            }
            var personByLink = SQLDAO.Instance.GetPersonNoteIdFbByLink(url);
            if (personByLink.HasValue)
            {
                return personByLink.Value; // (FBType, IdFB)
            }
            // =========================
            // 2️⃣ RAM CACHE
            // =========================
            if (CheckedTypeCache.TryGetValue(url, out var cached))
                return cached;

            // =========================
            // 3️⃣ PLAYWRIGHT (CUỐI CÙNG)
            // =========================
            if (mainPage == null || mainPage.IsClosed)
                return (FBType.Unknown, "");

            IPage newPage = null;
            try
            {
                newPage = await ads.Instance.OpenNewTabSimpleAsync(mainPage, url);
                if (newPage == null)
                    return (FBType.Unknown, "");

                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(300);

                FBType type = await CheckFBTypeAsync(newPage);

                string html = await newPage.ContentAsync();
                string idfb = ProcessingHelper.ExtractFacebookObjectIdFromHtml(html, url);

                var result = (type, idfb);

                CheckedTypeCache[url] = result;
                return result;
            }
            finally
            {
                if (newPage != null)
                    try { await newPage.CloseAsync(); } catch { }
            }
        }

        public async Task<FBType> TryCheckTypeAsync(IPage page, string link)
        {
            if (page == null || page.IsClosed ||
                string.IsNullOrWhiteSpace(link) ||
                link == "N/A")
                return FBType.Unknown;

            var (type, _) = await CheckTypeCachedAsync(page, link);
            return type;
        }

        //5*---LẤY TƯƠNG TÁC    
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
        //====II. PHẦN BÀI GỐC 
        ///  // HÀM CHÍNH LẤY BÀI GỐC ĐANG DÙNG
        /*  public async Task<PostPage> GetPostOriginal(IPage mainPage, string url, int timeoutSec = 3)
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
                      string selector2 = "div[class='html-div xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x78zum5 xdt5ytf x1iyjqo2 x1n2onr6 xqbnct6 xga75y6']";

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
                  (post.LikeCount, post.CommentCount, post.ShareCount) =await ExtractPostInteractionsAsync(postDiv);
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
                  var pageNameDiv = await postDiv.QuerySelectorAsync("span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs']");
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
                          post.PostType = PostType.Page_BackGround.ToString();
                      }
                  }
                  else post.PostType = PostType.Page_Normal.ToString();
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
          */
        //
        public async Task<PostInfoRawVM> GetPostOriginalInfoAsync(IPage mainPage,string url,int timeoutSec = 3)
        {
            var info = new PostInfoRawVM
            {
                PostLink = url
            };

            IPage newPage = null;
            bool success = false;

            try
            {
                Libary.Instance.LogDebug($"{Libary.IconStart} [ORIGINAL POST] Open original post");

                // ===============================
                // 1️⃣ MỞ TAB MỚI
                // ===============================
                newPage = await AdsPowerPlaywrightManager.Instance.OpenNewTabSimpleAsync(mainPage, url);
                await ProcessingDAO.Instance.HumanScrollAsync(newPage);
                await newPage.WaitForTimeoutAsync(200);
                if(newPage != null) Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Mở tab mới OK");
                // ===============================
                // 2️⃣ TÌM POST DIV
                // ===============================
                var postDiv = await GetFeedPostOriginalNormalAsync(newPage);
                if (postDiv == null)
                    return null;
                else Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Lấy Feed Post OK");
                // ===============================
                // 3️⃣ INTERACTIONS
                // ===============================
                (info.LikeCount, info.CommentCount, info.ShareCount) = await ExtractPostInteractionsAsync(postDiv);
                // ===============================
                // 4️⃣ THỜI GIAN
                // ===============================
                var postinfor = await postDiv.QuerySelectorAllAsync("div.xu06os2.x1ok221b");
                foreach (var el in postinfor)
                {
                    var txt = (await el.InnerTextAsync())?.Trim().ToLower();
                    if (ProcessingDAO.Instance.IsTime(txt))
                    {
                        txt = TimeHelper.CleanTimeString(txt);
                        info.PostTime = txt;
                        info.RealPostTime = TimeHelper.ParseFacebookTime(txt);
                        Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] thời gian lấy trong hàm ok {info.RealPostTime.ToString()}");
                        break;
                    }
                }
                // ===============================
                // 5️⃣ PAGE NAME + LINK
                // ===============================
                var (pageName, pageLink) = await GetPageContainerFromFeedAsync(postDiv);
                if (pageLink != "N/A")
                {
                    info.PageName = pageName;
                    info.PageLink = pageLink;
                    Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Lấy pageContainer thành công {info.PageName} link: {info.PageLink}");
                } 
                 var (posterName, posterLink) =await GetGroupPosterFromFeedAsync(postDiv);
                    if (posterLink != "N/A")
                    {
                        info.PosterName = posterName;
                        info.PosterLink = posterLink;
                        info.PosterNote = FBType.GroupOn;
                    }
                else
                {
                    info.PosterName = info.PageName;
                    info.PosterLink = info.PageLink;
                }
                // ===============================
                // 6️⃣ CONTENT
                // ===============================
                string content = await ExtractPopupContentAsync(newPage, postDiv);
                if (string.IsNullOrWhiteSpace(content) || content == "N/A")
                {
                    string bg = await BackgroundTextAllAsync(newPage, postDiv);
                    if (!string.IsNullOrWhiteSpace(bg))
                    {
                        content = bg.Trim();
                        info.PostType = PostType.Page_BackGround;                
                    }
                }
                else
                {
                    info.PostType = PostType.Page_Normal;
                }
                info.Content = content;
                // 6️⃣.1 ATTACHMENT (VIDEO / PHOTO)
                // ===============================
                var (hasVideo, rawVideoLink, videoTime) =
                    await CrawlBaseDAO.Instance.DetectVideoFromPostAsync(postDiv);

                List<(string Src, string Alt)> photos =
                    await CrawlBaseDAO.Instance.DetectPhotosFromPostAsync(postDiv);
                if (photos != null && photos.Count > 0)
                {
                    for (int i = 0; i < photos.Count; i++)
                    {
                        string alt = photos[i].Alt;
                        if (!string.IsNullOrWhiteSpace(alt))
                        {
                            info.Content += "\n" + alt;
                        }
                    }
                }
                info.AttachmentJson = AttachmentHelper.BuildAttachmentJson(
                hasVideo,
                ProcessingHelper.NormalizeReelLink(rawVideoLink),
                videoTime,
                photos);
                bool hasContent = ProcessingHelper.IsValidContent(info.Content);

                if (photos != null && photos.Count > 0)
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

                // xong xuôi mới chechk type
                // ===============================
                // CHECK TYPE SAU KHI ĐÃ LẤY XONG DATA
                // ===============================
                info.ContainerType =await CrawlBaseDAO.Instance.TryCheckTypeAsync(mainPage, info.PageLink);

                info.PosterNote =await CrawlBaseDAO.Instance.TryCheckTypeAsync(mainPage, info.PosterLink);

                if (ProcessingHelper.IsValidContent(info.Content)) 
                { Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Lấy Content thành công {info.Content}"); }
                success = true;
                return info;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} [ORIGINAL POST] Exception: {ex.Message}");
                return null;
            }
            finally
            {
                await SafeClosePageAsync(newPage);
                Libary.Instance.LogDebug(
                    success
                        ? "✔ [ORIGINAL POST] Done"
                        : "❌ [ORIGINAL POST] Failed");
                Libary.Instance.LogDebug(Libary.EndPost());
            }
        }
        public async Task<IElementHandle> GetFeedPostOriginalNormalAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return null;

            try
            {
                // ===============================
                // 1️⃣ Selector chính (aria-labelledby)
                // ===============================
                var postDiv = await page.QuerySelectorAsync("div[aria-labelledby='_R_imjbsmj5ilipam_']");
                if (postDiv != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconOK} [GetFeedPostOriginalNormal] Found postDiv by selector1");
                    return postDiv;
                }
                // ===============================
                // 2️⃣ Selector fallback (class dài)
                // ===============================
                postDiv = await page.QuerySelectorAsync(
                    "div[class='html-div xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b " +
                    "x18d9i69 x1c1uobl x78zum5 xdt5ytf x1iyjqo2 x1n2onr6 xqbnct6 xga75y6']");

                if (postDiv != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconWarn} [GetFeedPostOriginalNormal] Found postDiv by selector2");
                    return postDiv;
                }

                Libary.Instance.LogDebug($"{Libary.IconFail} [GetFeedPostOriginalNormal] postDiv not found");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug( $"{Libary.IconFail} [GetFeedPostOriginalNormal] Exception: {ex.Message}");
            }
            return null;
        }
        // lấy pagecontainer
        public async Task<(string PageName, string PageLink)>GetPageContainerFromFeedAsync(IElementHandle feed)
        {
            if (feed == null)
                return ("N/A", "N/A");
            try
            {
                // ==================================================
                // 1️⃣ ƯU TIÊN: span.html-span ... x1vvkbs
                // ==================================================
                var spanPrimary = await feed.QuerySelectorAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu " +
                    "xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs']");

                if (spanPrimary != null)
                {
                    var a = await spanPrimary.QuerySelectorAsync("a[href]");
                    if (a != null)
                    {
                        var name = (await a.InnerTextAsync())?.Trim() ?? "N/A";
                        var link = await a.GetAttributeAsync("href") ?? "N/A";

                        if (link != "N/A")
                            link = ProcessingHelper.ShortLinkPage(link);

                        Libary.Instance.LogDebug(
                            $"{Libary.IconOK} [PageContainer-Feed] Primary span | {name}");

                        return (name, link);
                    }
                }

                // ==================================================
                // 2️⃣ FALLBACK: a.x1i10hfl.xjbqb8w....
                // ==================================================
                var aFallback = await feed.QuerySelectorAsync(
                    "a[class='x1i10hfl xjbqb8w x1ejq31n x18oe1m7 x1sy0etr xstzfhl x972fbf " +
                    "x10w94by x1qhh985 x14e42zd x9f619 x1ypdohk xt0psk2 x3ct3a4 " +
                    "xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 " +
                    "x1c1uobl x16tdsg8 x1hl2dhg xggy1nq x1a2a7pz xkrqix3 " +
                    "x1sur9pj xzsf02u x1s688f'][href]");

                if (aFallback != null)
                {
                    var name = (await aFallback.InnerTextAsync())?.Trim() ?? "N/A";
                    var link = await aFallback.GetAttributeAsync("href") ?? "N/A";

                    if (link != "N/A") link = ProcessingHelper.ShortLinkPage(link);

                    Libary.Instance.LogDebug($"{Libary.IconWarn} [PageContainer-Feed] Fallback anchor | {name}");

                    return (name, link);
                }

                // ==================================================
                // 3️⃣ RỘNG NHẤT: div[data-ad-rendering-role='profile_name']
                // ==================================================
                var divWide = await feed.QuerySelectorAsync(
                    "div[data-ad-rendering-role='profile_name'] a[href]");

                if (divWide != null)
                {
                    var name = (await divWide.InnerTextAsync())?.Trim() ?? "N/A";
                    var link = await divWide.GetAttributeAsync("href") ?? "N/A";

                    if (link != "N/A")
                        link = ProcessingHelper.ShortLinkPage(link);

                    Libary.Instance.LogDebug(
                        $"{Libary.IconWarn} [PageContainer-Feed] Wide profile_name | {name}");

                    return (name, link);
                }

                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [PageContainer-Feed] Not found");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [PageContainer-Feed] Exception: {ex.Message}");
            }

            return ("N/A", "N/A");
        }
        public async Task<(string PosterName, string PosterLink)> GetGroupPosterFromFeedAsync(IElementHandle feed)
        {
            if (feed == null)
                return ("N/A", "N/A");

            try
            {
                var posterDiv = await feed.QuerySelectorAsync( "div[id='_R_2l5dimjbsmj5ilipamH1_']");

                if (posterDiv == null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconFail} [GroupPoster-Feed] Poster div not found");
                    return ("N/A", "N/A");
                }

                // ưu tiên lấy link
                var a = await posterDiv.QuerySelectorAsync("a[href]");
                if (a == null)
                {
                    Libary.Instance.LogDebug(
                        $"{Libary.IconFail} [GroupPoster-Feed] Anchor not found");
                    return ("N/A", "N/A");
                }

                var name = (await a.InnerTextAsync())?.Trim() ?? "N/A";
                var link = await a.GetAttributeAsync("href") ?? "N/A";

                if (link != "N/A") link = ProcessingHelper.ShortenPosterLink(link);

                Libary.Instance.LogDebug(
                    $"{Libary.IconOK} [GroupPoster-Feed] Poster={name}");

                return (name, link);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [GroupPoster-Feed] Exception: {ex.Message}");
                return ("N/A", "N/A");
            }
        }
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
                    Libary.Instance.LogDebug("[CONTENT] Dùng children[last] làm content.");
                }
                // 3) Nếu có contentContainer
                if (contentContainer != null)
                {
                    // try click xem thêm
                    var seeMore = await contentContainer.QuerySelectorAsync("div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
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
                    var fbCaptionDivs = await postDiv.QuerySelectorAllAsync("div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
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
                Libary.Instance.LogDebug($"{Libary.IconFail} [ExtractPopupContentAsync] Không tìm thấy content: {ex.Message}"
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
        //-- hàm  phụ lấy thông tin ng đăng
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
         //== Hàm xem post có Reel
        public async Task<(bool hasReel, string reelLink, string timeRaw)>DetectReelFromPostAsync(IElementHandle post)
        {
            string timeRaw = null;
            try
            {
                // ==================================================
                // 1️⃣ THỬ LẤY TIME RAW (NẾU CÓ)
                // ==================================================
                try
                {
                    var timeSpans = await post.QuerySelectorAllAsync( "span[class='x1lliihq x6ikm8r x10wlt62 x1n2onr6 xlyipyv xuxw1ft']");

                    foreach (var span in timeSpans)
                    {
                        string rawText = (await span.InnerTextAsync())?.Trim();
                        if (string.IsNullOrWhiteSpace(rawText))
                            continue;
                        // 👉 chỉ kiểm tra có phải text thời gian không
                        if (TimeHelper.IsTime(rawText))
                        {
                            timeRaw = TimeHelper.CleanTimeString(rawText);
                            Libary.Instance.LogDebug($"[ReelDetect] 🕒 Found timeRaw='{timeRaw}'");
                            break; // lấy cái đầu tiên
                        }
                    }
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogDebug(
                        $"[ReelDetect] ⚠️ TryGetTimeRaw failed: {ex.Message}");
                }

                // ==================================================
                // 2️⃣ DETECT REEL LINK
                // ==================================================
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

                    if (href.Contains("/reel/"))
                    {
                        Libary.Instance.LogDebug($"[REEL] ✅ Found Reel link: {href} | timeRaw={(timeRaw ?? "null")}");
                        return (true, href, timeRaw);
                    }
                }
                // ❌ không có reel
                Libary.Instance.LogDebug("[REEL] ❌ Post không chứa Reel");
                return (false, null, timeRaw);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    "[REEL] ❌ DetectReelFromPostAsync error: " + ex.Message);
                return (false, null, null);
            }
        }
        // ==hàm check media
        public async Task<(bool hasVideo, string videoLink, string timeRaw)> DetectVideoFromPostAsync(IElementHandle post)
        {
            try
            {
                string videoLink = null;
                string timeRaw = null;

                var links = await post.QuerySelectorAllAsync("a[href]");

                foreach (var a in links)
                {
                    string href = await a.GetAttributeAsync("href");
                    if (string.IsNullOrWhiteSpace(href))
                        continue;

                    // ❌ bỏ hashtag
                    if (href.IndexOf("/hashtag/", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;

                    bool isVideo =
                        href.IndexOf("/videos/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        href.IndexOf("/watch", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!isVideo)
                        continue;

                    // =========================
                    // 1️⃣ LINK VIDEO
                    // =========================
                    videoLink = UrlHelper.ShortenPostVideoLink(href);

                    // =========================
                    // 2️⃣ TIME NẰM TRONG <a>
                    // =========================
                    string aText = (await a.InnerTextAsync())?.Trim();

                    if (!string.IsNullOrWhiteSpace(aText) && TimeHelper.IsTime(aText))
                    {
                        timeRaw = TimeHelper.CleanTimeString(aText);
                    }
                    else
                    {
                        // fallback: span con trong a
                        var span = await a.QuerySelectorAsync("span");
                        if (span != null)
                        {
                            string spanText = (await span.InnerTextAsync())?.Trim();
                            if (TimeHelper.IsTime(spanText))
                                timeRaw = TimeHelper.CleanTimeString(spanText);
                        }
                    }

                    Libary.Instance.LogDebug(
                        $"[VideoDetect] 🎥 video={videoLink} | time='{timeRaw}'");

                    break;
                }

                if (string.IsNullOrWhiteSpace(videoLink))
                    return (false, null, null);

                return (true, videoLink, timeRaw);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    "[VideoDetect] ❌ DetectVideoFromPostAsync error: " + ex.Message);
                return (false, null, null);
            }
        }
        // check photo
        public async Task<List<(string Src, string Alt)>>DetectPhotosFromPostAsync(IElementHandle post)
        {
            List<(string Src, string Alt)> result =
                new List<(string Src, string Alt)>();

            try
            {
                var imgs = await post.QuerySelectorAllAsync(
                    "img[data-imgperflogname='feedPostPhoto']");

                for (int i = 0; i < imgs.Count; i++)
                {
                    string src = (await imgs[i].GetAttributeAsync("src"))?.Trim();
                    string alt = (await imgs[i].GetAttributeAsync("alt"))?.Trim();

                    if (string.IsNullOrWhiteSpace(src))
                        continue;

                    result.Add((src, alt ?? ""));
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    "[PhotoDetect] ❌ DetectPhotosFromPostAsync error: " + ex.Message);
            }

            return result;
        }

        //== hàm bổ trợ xem info thiếu gì
        public bool NeedFetchReelDetail(PostInfoRawVM info)
        {
            if (string.IsNullOrWhiteSpace(info.Content) || info.Content == "N/A")
                return true;

            if (info.LikeCount == 0 && info.CommentCount == 0 && info.ShareCount == 0)
                return true;

            if (string.IsNullOrWhiteSpace(info.PostTime) || info.PostTime == "N/A")
                return true;

            if (!info.RealPostTime.HasValue)
                return true;

            return false;
        }
        //== hàm bổ trợ merger dữ liệu k đè
        public void MergeReelInfoIfEmpty(PostInfoRawVM info, PostPage reel)
        {
            if ((string.IsNullOrWhiteSpace(info.Content) || info.Content == "N/A")
                && !string.IsNullOrWhiteSpace(reel.Content))
            {
                info.Content = reel.Content;
            }

            if (info.LikeCount == 0 && reel.LikeCount > 0)
                info.LikeCount = reel.LikeCount??0;

            if (info.CommentCount == 0 && reel.CommentCount > 0)
                info.CommentCount = reel.CommentCount??0;

            if (info.ShareCount == 0 && reel.ShareCount > 0)
                info.ShareCount = reel.ShareCount??0;

            if ((string.IsNullOrWhiteSpace(info.PostTime) || info.PostTime == "N/A")
                && !string.IsNullOrWhiteSpace(reel.PostTime))
            {
                info.PostTime = reel.PostTime;
                info.RealPostTime = reel.RealPostTime;
            }
        }
        // merg norlmal đều raw cả
        public void MergeRawInfoIfEmpty(PostInfoRawVM target,PostInfoRawVM source)
        {
            if (target == null || source == null)
                return;

            if (string.IsNullOrWhiteSpace(target.Content) || target.Content == "N/A")
                target.Content = source.Content;

            if (target.PostType == PostType.Page_Unknow)
                target.PostType = source.PostType;

            if (string.IsNullOrWhiteSpace(target.PageName))
                target.PageName = source.PageName;

            if (string.IsNullOrWhiteSpace(target.PageLink))
                target.PageLink = source.PageLink;

            if (string.IsNullOrWhiteSpace(target.PosterName))
                target.PosterName = source.PosterName;

            if (string.IsNullOrWhiteSpace(target.PosterLink))
                target.PosterLink = source.PosterLink;

            if (!target.RealPostTime.HasValue)
            {
                target.PostTime = source.PostTime;
                target.RealPostTime = source.RealPostTime;
            }

            if (target.LikeCount == 0)
                target.LikeCount = source.LikeCount;

            if (target.CommentCount == 0)
                target.CommentCount = source.CommentCount;

            if (target.ShareCount == 0)
                target.ShareCount = source.ShareCount;
        }
        // py pass nội dung bị che
        public  async Task BypassSensitiveReelAsync(IPage page, IElementHandle feed)
        
            {
            if (feed == null)
                return;

            try
            {
                string feedText = (await feed.InnerTextAsync()) ?? "";
                Console.WriteLine(feedText);
                bool isSensitive =
                           feedText.Contains("Nội dung nhạy cảm") ||
                           feedText.Contains("video này bị che đi") ||
                           feedText.Contains("sensitive content");

                if (!isSensitive)
                    return;

                Libary.Instance.LogDebug("[ReelShare] ⚠️ Sensitive content detected");
                // 1️⃣ Click "Tìm hiểu thêm"
                var btnLearnMore = await feed.QuerySelectorAsync(
                    "div[role='button'][aria-label='Tìm hiểu thêm'], " +
                    "div[role='button'][aria-label='Learn more']"
                );

                if (btnLearnMore != null)
                {
                    await btnLearnMore.ClickAsync();
                    Libary.Instance.LogDebug("[ReelShare] Clicked 'Tìm hiểu thêm'");
                    await page.WaitForTimeoutAsync(500);
                }
               
                // 2️⃣ Click "Xem video"
                var btnWatch = await page.QuerySelectorAsync(
                    "div[role='button'][aria-label='Xem video'], " +
                    "div[role='button'][aria-label='Watch video']"
                );

                if (btnWatch != null)
                {
                    await btnWatch.ClickAsync();
                    Libary.Instance.LogDebug("[ReelShare] Clicked 'Xem video'");               
                    await page.WaitForTimeoutAsync(800);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"[ReelShare] BypassSensitiveReelAsync error: {ex.Message}");             
            }
        }
        //hàm gộp cho ngắn thôi
        public async Task FillPosterInfoAsync(PostInfoRawVM info, IPage page, IElementHandle post, IReadOnlyList<IElementHandle> postinfor)
        {
            var posterContainer = postinfor != null && postinfor.Count > 0
                ? postinfor[0]
                : null;

            try
            {
                if (posterContainer != null)
                    (info.PosterName, info.PosterLink) =
                        await CrawlBaseDAO.Instance
                            .GetPosterInfoBySelectorsAsync(posterContainer);
            }
            catch
            {
                (info.PosterName, info.PosterLink) =
                    await CrawlBaseDAO.Instance
                        .GetPosterFromProfileNameAsync(post);
            }

            if (!string.IsNullOrWhiteSpace(info.PosterLink) &&
                info.PosterLink != "N/A")
            {
                (info.PosterNote, info.PosterIdFB) =await CrawlBaseDAO.Instance.CheckTypeCachedAsync(page, info.PosterLink);
            }
        }
        public async Task FillContentAndInteractionNormalAsync(
        PostInfoRawVM info,
        IPage page,
        IElementHandle post,
        IReadOnlyList<IElementHandle> postinfor)
        {
            int c = postinfor?.Count ?? 0;

            if (c >= 3)
            {
                info.Content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);

                info.PostType = !ProcessingHelper.IsValidContent(info.Content)
                        ? PostType.Share_WithContent
                        : PostType.Share_NoContent;
            }

            (info.LikeCount, info.CommentCount, info.ShareCount) = await ExtractPostInteractionsAsync(post);
        }
        public async Task FillFullContentPostNormalAsync(PostInfoRawVM info,IPage page,IElementHandle post, IReadOnlyList<IElementHandle> postinfor)
        {
            int c = postinfor?.Count ?? 0;
            Libary.Instance.LogTech($"[FillFullContentPostNormal] postinfor.Count={c}");
            string content = "N/A";
            PostType postType = PostType.Page_Unknow;

            // ========================
            // CASE c == 3
            // ========================
            if (c == 3)
            {             
                content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);
                if (!ProcessingHelper.IsValidContent(content))
                {                  
                    content = await CrawlBaseDAO.Instance.BackgroundTextAllAsync(page, post);               
                    if (ProcessingHelper.IsValidContent(content))
                    {                    
                        postType = PostType.Page_BackGround;                    
                    }
                    else Libary.Instance.LogTech($"{Libary.IconInfo} Content BG cũng k được, FAIL ");
                }
                else
                {
                    postType = PostType.Page_Normal;                 
                }
            }
            // ========================
            // CASE c == 2
            // ========================
            else if (c == 2)
            {
                content = await CrawlBaseDAO.Instance
                    .BackgroundTextAllAsync(page, post);

                postType = ProcessingHelper.IsValidContent(content)
                    ? PostType.Page_BackGround
                    : PostType.Page_NoConent;
            }
            // ========================
            // CASE c == 4
            // ========================
            else if (c == 4)
            {
                content = await CrawlBaseDAO.Instance.GetContentTextAsync(page, postinfor[2]);

                if (!ProcessingHelper.IsValidContent(content))
                {
                    content = await CrawlBaseDAO.Instance
                        .BackgroundTextAllAsync(page, post);

                    postType = ProcessingHelper.IsValidContent(content)
                        ? PostType.Page_BackGround
                        : PostType.Page_BackGround_Nocap;
                }
                else
                {
                    postType = PostType.Page_LinkWeb;
                }
            }
            // ========================
            // CASE OTHER
            // ========================
            else
            {
                content = await CrawlBaseDAO.Instance.BackgroundTextAllAsync(page, post);

                if (ProcessingHelper.IsValidContent(content))
                    postType = PostType.Page_BackGround;
            }

            info.Content = content;
            info.PostType = postType;

            Libary.Instance.LogTech($"[FillFullContentPostNormal] ContentLen={(content?.Length ?? 0)} | PostType={postType}");
        }

    }
}
