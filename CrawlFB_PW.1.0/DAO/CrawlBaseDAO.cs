using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CrawlFB_PW._1._0.DAO.Page;
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
        public static class ScrollService
        {
            public static ScrollManager Instance = new ScrollManager(3);
        }

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
                    //Libary.Instance.LogDebug($"iii {textContent}");
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
        public async Task<(string postTime, string originalPostTime, string postLink, string originalPostLink)> PostTypeDetectorAsync(
        List<string> timeList,
        List<string> linkList,
        IEnumerable<IElementHandle> postinfor = null)
        {
            string postTime = null;
            string originalPostTime = null;
            string postLink = null;
            string originalPostLink = null;

            try
            {
                int timeCount = timeList?.Count ?? 0;
                int linkCount = linkList?.Count ?? 0;

                // ===== CASE 1: BÀI TỰ ĐĂNG =====
                if (timeCount == 1 && linkCount >= 1)
                {
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);

                    Libary.Instance.LogDebug($"{Libary.IconInfo} Bài tự đăng");
                }
                // ===== CASE 2: BÀI SHARE =====
                else if (timeCount == 2 && linkCount >= 2)
                {
                    postTime = TimeHelper.CleanTimeString(timeList[0]);
                    originalPostTime = TimeHelper.CleanTimeString(timeList[1]);

                    postLink = ProcessingHelper.ShortenFacebookPostLink(linkList[0]);
                    originalPostLink = ProcessingHelper.ShortenFacebookPostLink(linkList[1]);

                    Libary.Instance.LogDebug($"{Libary.IconInfo} Bài Share");
                }
                // ===== CASE 0: VIDEO / REEL =====
                else if (timeCount == 0 && linkCount == 0)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Không có time/link → nghi video");

                    if (postinfor != null)
                    {
                        var (vtime, vlink) = await HandleVideoPostAsync(postinfor);

                        postTime = vtime;
                        postLink = vlink;

                        Libary.Instance.LogDebug(
                            !string.IsNullOrWhiteSpace(vlink)
                                ? $"{Libary.IconOK} Lấy được Link Video"
                                : $"{Libary.IconFail} Không lấy được Link Video"
                        );
                    }
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Không khớp pattern");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} PostTypeDetector ERROR: {ex.Message}"
                );
            }

            // ===== LOG KẾT QUẢ =====
            Libary.Instance.LogDebug(
                $"{Libary.IconInfo} [PostTypeDetector] " +
                $"PostTime={Libary.BoolIcon(!string.IsNullOrWhiteSpace(postTime))}, " +
                $"PostLink={Libary.BoolIcon(!string.IsNullOrWhiteSpace(postLink))}, " +
                $"OriginalLink={Libary.BoolIcon(!string.IsNullOrWhiteSpace(originalPostLink))}"
            );

            return (postTime, originalPostTime, postLink, originalPostLink);
        }

        //2*. NGƯỜI ĐĂNG 
        //2.1 LẤY THEO PROFILENAME - ĐẦU VÀO POSTNODE
        public async Task<(string name, string link)> GetPosterFromProfileNameAsync(IElementHandle post)
        {
            try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo} Thử lấy poster bằng profilename");

                var profileDiv = await post.QuerySelectorAsync("div[data-ad-rendering-role='profile_name']");
                if (profileDiv == null)
                    return (null, null);

                // 🔥 ƯU TIÊN lấy text (KHÔNG cần link)
                var textEl = await profileDiv.QuerySelectorAsync("div[role='button'], a, span");

                string name = (await textEl?.InnerTextAsync())?.Trim();

                // ===============================
                // 🔥 CHECK ẨN DANH
                // ===============================
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var lower = name.ToLower();

                    if (lower.Contains("ẩn danh") || lower.Contains("anonymous"))
                    {
                        return (SystemIds.PERSON_ANONYMOUS_NAME, SystemIds.PERSON_ANONYMOUS_ID);
                    }
                }

                // ===============================
                // 🔥 nếu có link thì lấy
                // ===============================
                var linkEl = await profileDiv.QuerySelectorAsync("a[href]");
                string link = await linkEl?.GetAttributeAsync("href");

                return (name, link);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [GetPosterFromProfileNameAsync] Lỗi: {ex.Message}");
                return (null, null);
            }
        }
        //2.2 --------------hàm lấy thông tin người đăng theo selector------------------đầu vào posinfor
        public async Task<(string name, string link)> GetPosterInfoBySelectorsAsync(IElementHandle container)
        {
            string name = null;
            string link = null;

            try
            {
                // ===== Selector chính =====
                Libary.Instance.LogDebug($"{Libary.IconInfo} Thử selector chính lấy người đăng");
                var el = await container.QuerySelectorAsync("span[class='xjp7ctv'] > a");
                if (el != null)
                {
                    name = (await el.InnerTextAsync())?.Trim();
                    link = await el.GetAttributeAsync("href");

                    if (!string.IsNullOrWhiteSpace(link))
                        link = ProcessingDAO.Instance.ShortenPosterLink(link);

                    Libary.Instance.LogDebug($"{Libary.IconOK} Selector chính OK | Name: {name}");
                    return (name, link);
                }

                // ===== Selector dự phòng =====
                Libary.Instance.LogDebug($"{Libary.IconInfo} Selector chính fail → thử selector dự phòng");
                var el2 = await container.QuerySelectorAsync("span[class='xjp7ctv'] > span > span > a");
                if (el2 != null)
                {
                    name = (await el2.InnerTextAsync())?.Trim();
                    link = await el2.GetAttributeAsync("href");

                    if (!string.IsNullOrWhiteSpace(link))
                        link = ProcessingDAO.Instance.ShortenPosterLink(link);

                    Libary.Instance.LogDebug($"{Libary.IconOK} Selector dự phòng OK | Name: {name}");
                    return (name, link);
                }

                // ===== Không tìm được =====
                Libary.Instance.LogDebug($"{Libary.IconFail} Không tìm thấy poster bằng selector");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} Lỗi selector poster: {ex.Message}");
            }

            // 🔥 FAIL THẬT → return null
            return (null, null);
        }

        //3*** LẤY CONTENT
        //3.1 NORLMAL nếu k lấy được nội dung---thay thế div hàm dưới => HÀM HIỆN TẠI ĐANG DÙNG
        public async Task<string> GetContentTextAsync(IPage page, IElementHandle container)
        {
            // ===============================
            // 1. Thử selector chính (div)
            // ===============================
            var contentEls = await container.QuerySelectorAllAsync(
                "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
            );

            if (contentEls.Count > 0)
            {
                Libary.Instance.LogDebug("✅ Tìm thấy nội dung (div chuẩn)");

                var content = await GetFullContentAsync(page, container);
                return string.IsNullOrWhiteSpace(content) ? null : content;
            }

            // ===============================
            // 2. Fallback: thẻ strong
            // ===============================
            var strongEls = await container.QuerySelectorAllAsync(
                "strong[class='html-strong xdj266r x14z9mp xat24cr x1lziwak xexx8yu xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs x1s688f']"
            );

            if (strongEls.Count > 0)
            {
                Libary.Instance.LogDebug("⚠️ Không có div → fallback strong");

                var texts = new List<string>();

                foreach (var el in strongEls)
                {
                    var text = (await el.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        texts.Add(text);
                }

                return texts.Count > 0
                    ? string.Join(" ", texts)
                    : null;
            }

            // ===============================
            // 3. Không có gì
            // ===============================
            Libary.Instance.LogDebug("❌ Không tìm thấy nội dung (div + strong)");
            return null;
        }

        // 🧩 Hàm mở “Xem thêm” và lấy nội dung bài đăng chính => HÀM HIỆN TẠI ĐANG DÙNG
        private async Task<string> GetFullContentAsync(IPage page, IElementHandle container)
        {
            if (container == null)
                return null;

            var sb = new StringBuilder();

            try
            {
                Libary.Instance.LogDebug($"{Libary.IconInfo}[FULL] START GetFullContentAsync");

                // ===== 1. Tìm & click "Xem thêm" trong container =====
                var seeMoreBtn = await container.QuerySelectorAsync(
                    "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                );

                if (seeMoreBtn != null)
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Tìm thấy 'Xem thêm' trong bài");

                    var bbox = await seeMoreBtn.BoundingBoxAsync();
                    if (bbox != null && bbox.Width > 5 && bbox.Height > 5)
                    {
                        await page.EvaluateAsync(
                            "el => el.scrollIntoView({behavior:'instant', block:'center'})",
                            seeMoreBtn
                        );

                        await page.WaitForTimeoutAsync(120);

                        try
                        {
                            await seeMoreBtn.ClickAsync(new ElementHandleClickOptions
                            {
                                Timeout = 2000
                            });

                            await page.WaitForTimeoutAsync(200);
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.LogDebug($"{Libary.IconInfo} JS fallback click: {ex.Message}");

                            await page.EvaluateAsync(
                                @"el => { try { el.click(); } catch {} }",
                                seeMoreBtn
                            );

                            await page.WaitForTimeoutAsync(150);
                        }
                    }
                    else
                    {
                        Libary.Instance.LogDebug($"{Libary.IconWarn} 'Xem thêm' bị che → bỏ qua");
                    }
                }
                else
                {
                    Libary.Instance.LogDebug($"{Libary.IconInfo} Không có 'Xem thêm'");
                }

                // ===== 2. Lấy nội dung =====
                var spans = await container.QuerySelectorAllAsync(
                    "div[class='xdj266r x14z9mp xat24cr x1lziwak x1vvkbs x126k92a']"
                );

                foreach (var span in spans)
                {
                    var text = (await span.InnerTextAsync())?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} ERROR GetFullContentAsync: {ex.Message}");
            }

            var result = sb.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(result))
            {
                Libary.Instance.LogDebug($"{Libary.IconOK} Đã lấy full content");
                return result;
            }

            Libary.Instance.LogDebug($"{Libary.IconWarn} Nội dung rỗng sau xử lý");
            return null;
        }

        //3.2  HÀM LẤY BACKGROUND => HIỆN TẠI ĐANG DÙNG
        public async Task<string> BackgroundTextAllAsync(IPage page, IElementHandle post)
        {
            if (post == null) return null;
        
            try
            {
                // ===============================
                // 1️⃣ story_message (ưu tiên)
                // ===============================
                var storyDiv = await post.QuerySelectorAsync("div[data-ad-rendering-role='story_message']");
                if (storyDiv != null)
                {
                    // ===== click "Xem thêm" =====
                    var seeMore = await storyDiv.QuerySelectorAsync(
                        "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                    );

                    if (seeMore != null)
                    {
                        Libary.Instance.CreateLog($"{Libary.IconInfo} TÌM THẤY 'Xem thêm' → click");

                        await page.EvaluateAsync("el => el.scrollIntoView({block:'center'})", seeMore);
                        await page.WaitForTimeoutAsync(120);

                        try
                        {
                            await seeMore.ClickAsync(new ElementHandleClickOptions { Timeout = 1500 });
                        }
                        catch (Exception ex)
                        {
                            Libary.Instance.CreateLog($"{Libary.IconWarn} fallback JS click: " + ex.Message);
                            await page.EvaluateAsync("(el)=>{ try{ el.click(); } catch{} }", seeMore);
                        }

                        await page.WaitForTimeoutAsync(200);
                    }

                    // ===== lấy text =====
                    var textNodes = await storyDiv.QuerySelectorAllAsync("div.xdj266r");

                    var sb = new StringBuilder();

                    foreach (var node in textNodes)
                    {
                        var t = (await node.InnerTextAsync())?.Trim();
                        if (!string.IsNullOrWhiteSpace(t))
                            sb.AppendLine(t);
                    }

                    var result = sb.ToString().Trim();

                    if (!string.IsNullOrWhiteSpace(result))
                        return result;

                    return null;
                }

                // ===============================
                // 2️⃣ Background text block
                // ===============================
                var bgBlocks = await post.QuerySelectorAllAsync(
                    "div[class='x1yx25j4 x13crsa5 x1rxj1xn x162tt16 x5zjp28'] > div"
                );

                if (bgBlocks != null && bgBlocks.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var e in bgBlocks)
                    {
                        var t = (await e.InnerTextAsync())?.Trim();
                        if (!string.IsNullOrWhiteSpace(t))
                            sb.AppendLine(t);
                    }

                    var result = sb.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(result))
                        return result;
                }

                // ===============================
                // 3️⃣ fallback ảnh (alt)
                // ===============================
                var bgImages = await post.QuerySelectorAllAsync("img[alt][class*='x15mokao']");

                if (bgImages != null && bgImages.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var img in bgImages)
                    {
                        var alt = (await img.GetAttributeAsync("alt"))?.Trim();
                        if (!string.IsNullOrWhiteSpace(alt))
                            sb.AppendLine(alt);
                    }

                    var result = sb.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(result))
                        return result;
                }

                Libary.Instance.LogDebug($"{Libary.IconFail} Không lấy được Background text");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} BackgroundText ERROR: {ex.Message}");
            }

            return null;
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
                await ScrollService.Instance.ScrollAsync(newPage, "OpenCheckType");

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
        public async Task<(FBType Type, string IdFB)> CheckTypeCachedAsync(IPage mainPage,string url,string knownIdFB = null)
        {
            if (string.IsNullOrWhiteSpace(url) || url == "N/A")
                return (FBType.Unknown, null);
            Libary.Instance.LogDebug($" [CheckTypeCachedAsync] bắt đầu CheckTypte {url} ");
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
            if (!ProcessingHelper.IsValidContent(url)) return (FBType.Unknown, null);
            //url k hợp lệ không mở tab
            if (mainPage == null || mainPage.IsClosed)
                return (FBType.Unknown, "");

            IPage newPage = null;
            try
            {
                newPage = await ads.Instance.OpenNewTabSimpleAsync(mainPage, url);
                if (newPage == null) return (FBType.Unknown, "");

                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await newPage.WaitForTimeoutAsync(300);

                FBType type = await CheckFBTypeAsync(newPage);

                string html = await newPage.ContentAsync();
                string idfb = ProcessingHelper.ExtractFacebookObjectIdFromHtml(html, url);
                Libary.Instance.LogDebug($" [CheckTypeCachedAsync] Kết quả CheckType: IDFB: {idfb} , Type {type} ");
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
            // 🔥 Chuẩn hóa link trước
            link = UrlHelper.ShortenPagePersonLink(link);
            link = ProcessingHelper.NormalizeInputUrl(link);
            string idfb = ProcessingHelper.ExtractFacebookId(link);
            var (type, _) = await CheckTypeCachedAsync(page, link, idfb);
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
        public async Task<PostInfoRawVM> GetPostOriginalInfoAsyncOld(IPage mainPage,string url,int timeoutSec = 3)
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
                if (newPage != null)
                {
                    await ScrollService.Instance.ScrollAsync(newPage, "OriginalPost");
                }
                // await ProcessingDAO.Instance.HumanScrollAsync(newPage);
                await newPage.WaitForTimeoutAsync(200);
                if(newPage != null) Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Mở tab mới OK");
                // ===============================
                // 2️⃣ TÌM POST DIV
                // ===============================
                // await newPage.WaitForSelectorAsync("div[data-ad-rendering-role='story_message']", new PageWaitForSelectorOptions { Timeout = 3000 });
                // var postDiv = await PopupDAO.Instance.GetFeedPopupAsync(newPage);
                var postDiv = await newPage.QuerySelectorAsync("div[aria-labelledby='_R_imjbsmj5ilipam_']");
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
                info.PageName = pageName;
                info.PageLink = pageLink;
                // 🔥 Detect bằng link
                bool isGroup = !string.IsNullOrWhiteSpace(pageLink) &&
               pageLink.Contains("/groups/");

                if (isGroup)
                {
                    var (posterName, posterLink) = await GetGroupPosterFromFeedAsync(postDiv);

                    if (!string.IsNullOrWhiteSpace(posterLink) && posterLink != "N/A")
                    {
                        info.PosterName = posterName;
                        info.PosterLink = posterLink;
                        Libary.Instance.LogTech("[ORIGINAL POST] Detect sơ bộ = Group → lấy poster feed");
                    }
                }
                else
                {
                    Libary.Instance.LogTech("[ORIGINAL POST] Detect sơ bộ = Fanpage/Profile → chưa gán poster, chờ check type");
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
                var (hasVideo, rawVideoLink, videoTime) = await CrawlBaseDAO.Instance.DetectVideoFromPostAsync(postDiv);

                List<(string Src, string Alt)> photos = await CrawlBaseDAO.Instance.DetectPhotosFromPostAsync(postDiv);
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
                info.AttachmentJson = AttachmentHelper.BuildAttachmentJson(hasVideo,
                ProcessingHelper.NormalizeReelLink(rawVideoLink),videoTime,photos);
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
                var (containerType, idfbContainer) = await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(mainPage, info.PageLink);

                if (containerType != FBType.Unknown) info.ContainerType = containerType;

                // Gán IDFB container
                if (!string.IsNullOrWhiteSpace(idfbContainer) && idfbContainer != "N/A") info.ContainerIdFB = idfbContainer;
                if (!string.IsNullOrWhiteSpace(info.ContainerIdFB) && info.ContainerIdFB != "N/A")
                {
                    var dbPage = SQLDAO.Instance.GetPageByIDFB(info.ContainerIdFB);
                    if (dbPage != null)
                    {
                        info.PageID = dbPage.PageID;
                        info.PageName = dbPage.PageName;
                        info.PageLink = dbPage.PageLink;
                        info.ContainerIdFB = dbPage.IDFBPage;

                        Libary.Instance.LogTech("[ORIGINAL] ✅ Resolve from DB by IDFB");
                    }
                }
                if (info.ContainerType == FBType.Fanpage)
                {
                    info.PosterName = info.PageName;
                    info.PosterLink = info.PageLink;
                    info.PosterIdFB = info.ContainerIdFB;
                    info.PosterNote = FBType.Fanpage;

                    Libary.Instance.LogTech("[ORIGINAL] Poster = Container Fanpage");
                }
                else if (info.ContainerType == FBType.GroupOn)
                {
                    var (posterType, idfbPoster) = await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(mainPage, info.PosterLink);

                    if (posterType != FBType.Unknown) info.PosterNote = posterType;

                    if (!string.IsNullOrWhiteSpace(idfbPoster) && idfbPoster != "N/A") info.PosterIdFB = idfbPoster;

                    Libary.Instance.LogTech("[ORIGINAL] Poster = entity trong Group");
                }
          
            // 🔥 PERSON (THIẾU – BỔ SUNG)
            // ========================
        else if (info.ContainerType == FBType.Person ||
             info.ContainerType == FBType.PersonKOL)
                    {
                    // ❗ container không tồn tại
                    info.PageName = "N/A";
                    info.PageLink = "N/A";
                    info.ContainerIdFB = null;

                    // poster chính là user
                    if (ProcessingHelper.IsValidContent(info.PosterLink))
                    {
                        var (posterType, idfbPoster) =
                            await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(mainPage, info.PosterLink);

                        if (posterType != FBType.Unknown)
                            info.PosterNote = posterType;

                        if (ProcessingHelper.IsValidContent(idfbPoster))
                            info.PosterIdFB = idfbPoster;
                    }

                    Libary.Instance.LogTech("[ORIGINAL] PERSON → no container, poster = user");
                }
                Libary.Instance.LogDebug(
                    $"Kết quả Original Page: PageName {info.PageName} , " +
                    $"PageID {info.PageID}, Pagelink: {info.PageLink}, Pageidfb: {info.ContainerIdFB}"
                );

                Libary.Instance.LogDebug(
                    $"Kết quả Original POSTER: PosterName {info.PosterName}, " +
                    $"Posterlink: {info.PosterLink}, Posteridfb: {info.PosterIdFB}");
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
        public async Task<PostInfoRawVM> GetPostOriginalInfoAsync(IPage mainPage,string url,int timeoutSec = 3)
        {
            var info = new PostInfoRawVM
            {
                PostLink = url
            };

            IPage newPage = null;

            try
            {
                // ===============================
                // 1️⃣ OPEN TAB
                // ===============================
                newPage = await AdsPowerPlaywrightManager.Instance.OpenTabAndPrepareDomAsync(mainPage, url);
                if (newPage == null)
                {
                    return null;
                }
                // await ProcessingDAO.Instance.HumanScrollAsync(newPage);
                await newPage.WaitForTimeoutAsync(200);
                if (newPage != null) Libary.Instance.LogTech($"{Libary.IconOK} [ORIGINAL POST] Mở tab mới OK");
                // ===============================
                // 2️⃣ GET POST DIV
                // ===============================
                var postDiv = await CrawlBaseDAO.Instance.GetFeedPostOriginalNormalAsync(newPage);

                if (postDiv == null) return null;

                // ===============================
                // 3️⃣ TIME
                // ===============================
                var postinfor = await postDiv.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

                foreach (var el in postinfor)
                {
                    var txt = (await el.InnerTextAsync())?.Trim().ToLower();

                    if (string.IsNullOrEmpty(txt))
                        continue;

                    if (ProcessingDAO.Instance.IsTime(txt))
                    {
                        txt = TimeHelper.CleanTimeString(txt);
                        info.PostTime = txt;
                        info.RealPostTime = TimeHelper.ParseFacebookTime(txt);
                        break;
                    }
                }

                // ===============================
                // 4️⃣ INTERACTION
                // ===============================
                (info.LikeCount, info.CommentCount, info.ShareCount) =await CrawlBaseDAO.Instance.ExtractPostInteractionsAsync(postDiv);

                // ===============================
                // 5️⃣ CONTENT
                // ===============================
                var content = await PopupDAO.Instance.GetContentPopup(postDiv);
                // ===============================
                // 6️⃣ MEDIA
                // ===============================
                var (hasVideo, rawVideoLink, videoTime) =
                    await CrawlBaseDAO.Instance.DetectVideoFromPostAsync(postDiv);

                var photos = await CrawlBaseDAO.Instance
                    .DetectPhotosFromPostAsync(postDiv);

                // fallback ALT nếu không có content
                if (string.IsNullOrWhiteSpace(content))
                {
                    content = "";

                    foreach (var p in photos)
                    {
                        if (!string.IsNullOrWhiteSpace(p.Alt))
                        {
                            content += (content.Length == 0 ? "" : "\n") + p.Alt;
                        }
                    }
                }

                info.Content = content;

                // attachment
                info.AttachmentJson = AttachmentHelper.BuildAttachmentJson(
                    hasVideo,
                    ProcessingHelper.NormalizeReelLink(rawVideoLink),
                    videoTime,
                    photos);

                // ===============================
                // 7️⃣ POSTER + CONTAINER
                // ===============================
                await ResolvePosterAndContainer(info, newPage, postDiv);

                // ===============================
                // 8️⃣ POST TYPE
                // ===============================
                ResolvePostType(info, photos, hasVideo, rawVideoLink);

                return info;
            }
            catch
            {
                return null;
            }
            finally
            {
                await SafeClosePageAsync(newPage);
            }
        }
        // hàm phụ getpostOriginal
        private void ResolvePostType(
    PostInfoRawVM info,
    List<(string Src, string Alt)> photos,
    bool hasVideo,
    string rawVideoLink)
        {
            bool hasContent = ProcessingHelper.IsValidContent(info.Content);
            bool hasPhoto = photos != null && photos.Count > 0;

            bool isReel = !string.IsNullOrWhiteSpace(rawVideoLink)
                          && rawVideoLink.Contains("/reel/");

            // ===============================
            // PAGE
            // ===============================
            if (info.ContainerType == FBType.Fanpage || info.ContainerType == FBType.GroupOn)
            {
                if (isReel)
                {
                    info.PostType = hasContent
                        ? PostType.page_Real_Cap
                        : PostType.Page_Reel_NoCap;
                }
                else if (hasVideo)
                {
                    info.PostType = hasContent
                        ? PostType.Page_Video_Cap
                        : PostType.Page_Video_Nocap;
                }
                else if (hasPhoto)
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
            }

            // ===============================
            // PERSON
            // ===============================
            else if (info.ContainerType == FBType.Person ||
                     info.ContainerType == FBType.PersonKOL)
            {
                if (isReel)
                {
                    info.PostType = hasContent
                        ? PostType.Person_Reel_ConTent
                        : PostType.Person_Reel_NoConent;
                }
                else if (hasVideo)
                {
                    info.PostType = hasContent
                        ? PostType.Person_video_cap
                        : PostType.Person_video_Nocap;
                }
                else if (hasPhoto)
                {
                    info.PostType = hasContent
                        ? PostType.Person_Photo_cap
                        : PostType.Person_Photo_Nocap;
                }
                else
                {
                    info.PostType = hasContent
                        ? PostType.Person_Normal
                        : PostType.Person_Unknow;
                }
            }

            // ===============================
            // FALLBACK
            // ===============================
            else
            {
                info.PostType = PostType.Page_Unknow;
            }
        }
        private async Task ResolvePosterAndContainer(
    PostInfoRawVM info,
    IPage page,
    IElementHandle postDiv)
        {
            var (nametemp, linktemp) = await PopupDAO.Instance.GetPosterPopup(postDiv);          

            var (containerType, idfbContainer) = await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(page, linktemp);

            info.ContainerType = containerType;

            if (!string.IsNullOrWhiteSpace(idfbContainer)) info.ContainerIdFB = idfbContainer;

            // resolve DB
            if (!string.IsNullOrWhiteSpace(info.ContainerIdFB))
            {
                var dbPage = SQLDAO.Instance.GetPageByIDFB(info.ContainerIdFB);
                if (dbPage != null)
                {
                    info.PageID = dbPage.PageID;
                    info.PageName = dbPage.PageName;
                    info.PageLink = dbPage.PageLink;
                    info.ContainerIdFB = dbPage.IDFBPage;
                }
            }

            // ===============================
            // FANPAGE
            // ===============================
            if (containerType == FBType.Fanpage)
            {
                info.PosterName = nametemp;
                info.PosterLink = linktemp;
                info.PosterIdFB = info.ContainerIdFB;
                info.PosterNote = FBType.Fanpage;

                info.PageName = nametemp;
                info.PageLink = linktemp;
            }

            // ===============================
            // GROUP
            // ===============================
            else if (containerType == FBType.GroupOn)
            {
                info.PageName = nametemp;
                info.PageLink = linktemp;
                (info.PosterName, info.PosterLink) = await PopupDAO.Instance.GetPosterGroupsPopupPost(postDiv);

                var (posterType, idfbPoster) =
                    await CrawlBaseDAO.Instance.TryCheckTypeFullAsync(page, info.PosterLink);

                if (posterType != FBType.Unknown)
                    info.PosterNote = posterType;

                if (!string.IsNullOrWhiteSpace(idfbPoster))
                    info.PosterIdFB = idfbPoster;
            }

            // ===============================
            // PERSON
            // ===============================
            else if (containerType == FBType.Person ||
                     containerType == FBType.PersonKOL)
            {
                info.PageName = null;
                info.PageLink = null;
                info.PosterName = nametemp;
                info.PosterLink = linktemp;
                info.ContainerIdFB = idfbContainer;
                info.PosterIdFB = idfbContainer;
                info.PosterNote = containerType;
            }
        }
        public async Task<IElementHandle> GetFeedPostOriginalNormalAsync(IPage page)
        {
            if (page == null || page.IsClosed)
                return null;

            try
            {
                // ===============================
                // 🔥 1️⃣ ƯU TIÊN SELECTOR CỨNG (NHANH NHẤT)
                // ===============================
                var postDiv = await page.QuerySelectorAsync(
                    "div[aria-labelledby='_R_imjbsmj5ilipam_']"
                );

                if (postDiv != null)
                {
                    Libary.Instance.LogDebug("🔥 [PopupDAO] Found by aria-labelledby cụ thể");
                    return postDiv;
                }

                // ===============================
                // ⏳ 2️⃣ WAIT POPUP (CHỈ KHI CHƯA THẤY)
                // ===============================
                try
                {
                    await page.WaitForSelectorAsync(
                        "div[role='dialog']",
                        new PageWaitForSelectorOptions
                        {
                            Timeout = 5000
                        }
                    );
                }
                catch
                {
                    Libary.Instance.LogDebug("⚠️ Popup dialog không xuất hiện");
                }

                // ===============================
                // 🔥 3️⃣ ƯU TIÊN dialog có aria-labelledby
                // ===============================
                var dialog = await page.QuerySelectorAsync(
                    "div[role='dialog'][aria-labelledby]"
                );

                if (dialog != null)
                {
                    Libary.Instance.LogDebug("✅ [PopupDAO] Found dialog (aria-labelledby)");
                    return dialog;
                }

                // ===============================
                // 🔥 4️⃣ FALLBACK dialog chung
                // ===============================
                dialog = await page.QuerySelectorAsync("div[role='dialog']");

                if (dialog != null)
                {
                    Libary.Instance.LogDebug("⚠️ [PopupDAO] Found dialog fallback");
                    return dialog;
                }

                Libary.Instance.LogDebug("❌ [PopupDAO] Không tìm thấy popup dialog");
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"❌ [PopupDAO] Exception: {ex.Message}");
            }

            return null;
        }
        // lấy pagecontainer hàm con của GetPostOriginalInfoAsync
        public async Task<(string PageName, string PageLink)> GetPageContainerFromFeedAsync(IElementHandle feed)
        {
            if (feed == null)
                return (null, null);

            try
            {
                var spanPrimary = await feed.QuerySelectorAsync(
                    "span[class='html-span xdj266r x14z9mp xat24cr x1lziwak xexx8yu " +
                    "xyri2b x18d9i69 x1c1uobl x1hl2dhg x16tdsg8 x1vvkbs']");

                if (spanPrimary != null)
                {
                    var a = await spanPrimary.QuerySelectorAsync("a[href]");
                    if (a != null)
                    {
                        var name = Clean((await a.InnerTextAsync()));
                        var link = Clean(await a.GetAttributeAsync("href"));

                        if (link != null)
                            link = ProcessingHelper.ShortLinkPage(link);

                        return (name, link);
                    }
                }

                var aFallback = await feed.QuerySelectorAsync("a[href]");

                if (aFallback != null)
                {
                    var name = Clean((await aFallback.InnerTextAsync()));
                    var link = Clean(await aFallback.GetAttributeAsync("href"));

                    if (link != null)
                        link = ProcessingHelper.ShortLinkPage(link);

                    return (name, link);
                }

                var divWide = await feed.QuerySelectorAsync("div[data-ad-rendering-role='profile_name'] a[href]");

                if (divWide != null)
                {
                    var name = Clean((await divWide.InnerTextAsync()));
                    var link = Clean(await divWide.GetAttributeAsync("href"));

                    if (link != null)
                        link = ProcessingHelper.ShortLinkPage(link);

                    return (name, link);
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug($"{Libary.IconFail} [PageContainer-Feed] Exception: {ex.Message}");
            }

            return (null, null);
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
        public async Task<string> ExtractPopupContentAsync(IPage page, IElementHandle postDiv)
        {
            if (postDiv == null)
                return null;

            try
            {
                string content = null;

                // ===============================
                // 1) Lấy children
                // ===============================
                var children = await postDiv.QuerySelectorAllAsync(":scope > div");
                IElementHandle contentContainer = null;

                // 2) Nếu có ≥ 3 phần tử → thẻ cuối là caption
                if (children != null && children.Count == 3)
                {
                    contentContainer = children[children.Count - 1];
                    Libary.Instance.LogDebug("[CONTENT] Dùng children[last] làm content.");
                }

                // ===============================
                // 3) Lấy từ contentContainer
                // ===============================
                if (contentContainer != null)
                {
                    var seeMore = await contentContainer.QuerySelectorAsync(
                        "div[role='button']:has-text(\"Xem thêm\"), div[role='button']:has-text(\"See more\")"
                    );

                    if (seeMore != null)
                    {
                        Libary.Instance.CreateLog("[CONTENT] Click 'Xem thêm'");
                        try { await seeMore.ClickAsync(); } catch { }
                        await page.WaitForTimeoutAsync(200);
                    }

                    var raw = await contentContainer.InnerTextAsync();
                    if (!string.IsNullOrWhiteSpace(raw))
                        content = raw.Trim();
                }

                // ===============================
                // 4) FALLBACK — div caption chuẩn
                // ===============================
                if (string.IsNullOrWhiteSpace(content))
                {
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

                        var result = sb.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(result))
                            content = result;
                    }
                }

                // ===============================
                // 5) RETURN
                // ===============================
                if (!string.IsNullOrWhiteSpace(content))
                {
                    Libary.Instance.LogDebug(
                        $"{Libary.IconOK} [ExtractPopupContentAsync] OK (len={content.Length})"
                    );
                    return content;
                }

                return null;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogDebug(
                    $"{Libary.IconFail} [ExtractPopupContentAsync] ERROR: {ex.Message}"
                );
                return null;
            }
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
            string postTime = null;
            string postLink = null;

            try
            {
                foreach (var post in postinfor)
                {
                    // ===============================
                    // 1️⃣ Lấy link video
                    // ===============================
                    if (string.IsNullOrWhiteSpace(postLink))
                    {
                        var videoAnchors = await post.QuerySelectorAllAsync("a[href*='video'], a[href*='reel']");
                        if (videoAnchors != null && videoAnchors.Count > 0)
                        {
                            var href = await videoAnchors.First().GetAttributeAsync("href");
                            if (!string.IsNullOrWhiteSpace(href))
                                postLink = href;
                        }
                    }

                    // ===============================
                    // 2️⃣ Lấy thời gian
                    // ===============================
                    if (string.IsNullOrWhiteSpace(postTime))
                    {
                        var txt = (await post.InnerTextAsync())?.Trim();

                        if (!string.IsNullOrWhiteSpace(txt) &&
                            Regex.IsMatch(txt, @"(\d+\s*(giờ|phút|ngày|tuần|hôm qua|tháng))", RegexOptions.IgnoreCase))
                        {
                            postTime = TimeHelper.CleanTimeString(txt);
                        }
                    }

                    // ===============================
                    // 3️⃣ Nếu đã đủ → break
                    // ===============================
                    if (!string.IsNullOrWhiteSpace(postTime) &&
                        !string.IsNullOrWhiteSpace(postLink))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ HandleVideoPostAsync ERROR: " + ex.Message);
            }

            return (postTime, postLink);
        }
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
                var imgs = await post.QuerySelectorAllAsync("img[data-imgperflogname='feedPostPhoto'], img[data-imgperflogname='feedImage']");

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
            // Không có content
            if (string.IsNullOrWhiteSpace(info.Content))
                return true;

            // Không có tương tác
            if (info.LikeCount == 0 && info.CommentCount == 0 && info.ShareCount == 0)
                return true;

            // Không có thời gian
            if (string.IsNullOrWhiteSpace(info.PostTime))
                return true;

            // Không parse được thời gian thật
            if (!info.RealPostTime.HasValue)
                return true;

            return false;
        }
        //== hàm bổ trợ merger dữ liệu k đè
        public void MergeReelInfoIfEmpty(PostInfoRawVM info, PostPage reel)
        {
            if (string.IsNullOrWhiteSpace(info.Content) &&!string.IsNullOrWhiteSpace(reel.Content))
            {
                info.Content = reel.Content;
            }

            if (info.LikeCount == 0 && reel.LikeCount > 0)
                info.LikeCount = reel.LikeCount??0;

            if (info.CommentCount == 0 && reel.CommentCount > 0)
                info.CommentCount = reel.CommentCount??0;

            if (info.ShareCount == 0 && reel.ShareCount > 0)
                info.ShareCount = reel.ShareCount??0;
            if (string.IsNullOrWhiteSpace(info.PostTime) &&
                   !string.IsNullOrWhiteSpace(reel.PostTime))
            {
                info.PostTime = reel.PostTime;

                if (!info.RealPostTime.HasValue && reel.RealPostTime.HasValue)
                    info.RealPostTime = reel.RealPostTime;
            }
        }
        // merg norlmal đều raw cả
        public void MergeRawInfoIfEmpty(PostInfoRawVM target, PostInfoRawVM source)
        {
            if (target == null || source == null)
                return;

            // ===============================
            // TEXT
            // ===============================
            if (string.IsNullOrWhiteSpace(target.Content))
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

            // ===============================
            // ENUM
            // ===============================
            if (target.PosterNote == FBType.Unknown)
                target.PosterNote = source.PosterNote;

            if (target.ContainerType == FBType.Unknown)
                target.ContainerType = source.ContainerType;

            // ===============================
            // TIME
            // ===============================
            if (!target.RealPostTime.HasValue && source.RealPostTime.HasValue)
            {
                target.PostTime = source.PostTime;
                target.RealPostTime = source.RealPostTime;
            }

            // ===============================
            // INTERACTION (nullable safe)
            // ===============================
            if ((target.LikeCount ?? 0) == 0 && (source.LikeCount ?? 0) > 0)
                target.LikeCount = source.LikeCount;

            if ((target.CommentCount ?? 0) == 0 && (source.CommentCount ?? 0) > 0)
                target.CommentCount = source.CommentCount;

            if ((target.ShareCount ?? 0) == 0 && (source.ShareCount ?? 0) > 0)
                target.ShareCount = source.ShareCount;

            // ===============================
            // ID (QUAN TRỌNG)
            // ===============================
            if (string.IsNullOrWhiteSpace(target.ContainerIdFB) &&
                !string.IsNullOrWhiteSpace(source.ContainerIdFB))
            {
                target.ContainerIdFB = source.ContainerIdFB;
            }

            if (string.IsNullOrWhiteSpace(target.PosterIdFB) &&
                !string.IsNullOrWhiteSpace(source.PosterIdFB))
            {
                target.PosterIdFB = source.PosterIdFB;
            }

            if (string.IsNullOrWhiteSpace(target.PageID) &&
                !string.IsNullOrWhiteSpace(source.PageID))
            {
                target.PageID = source.PageID;
            }
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

            // ===== 1. Try selector =====
            if (posterContainer != null)
            {
                try
                {
                    (info.PosterName, info.PosterLink) =
                        await CrawlBaseDAO.Instance.GetPosterInfoBySelectorsAsync(posterContainer);
                }
                catch
                {
                    // ignore
                }
            }

            // ===== 2. Fallback nếu fail =====
            if (!ProcessingHelper.IsValidContent(info.PosterLink))
            {
                Libary.Instance.LogDebug("[PosterFallback] dùng ProfileName");

                (info.PosterName, info.PosterLink) =
                    await CrawlBaseDAO.Instance.GetPosterFromProfileNameAsync(post);
            }

            // ===== 3. Anonymous check =====
            if (info.PosterName == SystemIds.PERSON_ANONYMOUS_NAME)
            {
                info.PosterNote = FBType.PersonHidden;
                info.PosterIdFB = null;
                return;
            }

            // ===== 4. Nếu vẫn không có link → stop =====
            if (!ProcessingHelper.IsValidContent(info.PosterLink))
                return;

            // ===== 5. Resolve type =====
            (info.PosterNote, info.PosterIdFB) =
                await CrawlBaseDAO.Instance.CheckTypeCachedAsync(page, info.PosterLink);
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

        // chektype mới
        public async Task<(FBType Type, string IdFB)> TryCheckTypeFullAsync(IPage page, string link)
        {
            // ❌ chặn từ đầu (QUAN TRỌNG)
            if (page == null || page.IsClosed ||
                !ProcessingHelper.IsValidContent(link))
                return (FBType.Unknown, null);

            link = UrlHelper.ShortenPagePersonLink(link);
            link = ProcessingHelper.NormalizeInputUrl(link);

            // ❌ chặn lần 2 sau normalize
            if (!ProcessingHelper.IsValidContent(link))
                return (FBType.Unknown, null);

            string idfb = ProcessingHelper.ExtractFacebookId(link);

            return await CheckTypeCachedAsync(page, link, idfb);
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
