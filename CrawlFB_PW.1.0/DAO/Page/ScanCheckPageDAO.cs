using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using IPage = Microsoft.Playwright.IPage;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Security.Policy;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using CrawlFB_PW._1._0.Helper;
using FBType = CrawlFB_PW._1._0.Enums.FBType;
namespace CrawlFB_PW._1._0.DAO
{
    public class ScanCheckPageDAO
    {
        private static ScanCheckPageDAO instance;
        public static ScanCheckPageDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new ScanCheckPageDAO();
                return instance;
            }
        }
        private ScanCheckPageDAO()
        {
            // private constructor
        }
        public class GroupAboutInfo
        {
            public string CreatedDateExact { get; set; } = "N/A";
            public string MemberShort { get; set; } = "N/A";
            public string MemberTotal { get; set; } = "N/A";
            public string PageName { get; set; } = "N/A";
            public string IDFB { get; set; } = "N/A";
        }
        public class FanpageAboutInfo
        {
            public string CreatedDateExact { get; set; } = "N/A";
            public string MemberTotal { get; set; } = "N/A";
            public string PageName { get; set; } = "N/A";
            public string PageInfo { get; set; } = "N/A";
            public string IDFB { get; set; } = "N/A";
        }
        public string ExtractRelativeCreated(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length >= 2)
                return lines[1].Trim();

            return "N/A";
        }
        public string ExtractShortMembers(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"\d+[.,]?\d*[KkMm]?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return match.Success ? match.Value : "N/A";
        }
        public string ExtractTotalMembers(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"\d{1,3}(?:[.,]\d{3})*",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return match.Success ? match.Value : "N/A";
        }
        public string ExtractCreatedDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "N/A";

            // REGEX BẮT MỌI DẠNG NGÀY THÁNG FACEBOOK
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"(\d{1,2}\s+tháng\s+\d{1,2}(?:,|\s+năm)?\s+\d{4})",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return "N/A";
        }
        public async Task<DateTime?> GetPostTimeAsync(IPage page)
        {
            List<DateTime> timeList = new List<DateTime>();

            await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 5);
            await page.WaitForTimeoutAsync(2000);

            var nodes = await page.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");
            if (nodes == null || nodes.Count == 0)
                return null;

            foreach (var node in nodes)
            {
                var postinfor = await node.QuerySelectorAllAsync("div[class='xu06os2 x1ok221b']");
                foreach (var info in postinfor)
                {
                    var links = await info.QuerySelectorAllAsync("a[href]");
                    if (links == null) continue;

                    foreach (var link in links)
                    {
                        string raw = (await link.InnerTextAsync())?.Trim();
                        if (string.IsNullOrWhiteSpace(raw))
                            continue;

                        // ✔ chỉ nhận diện text thời gian
                        if (!ProcessingDAO.Instance.IsTime(raw))
                            continue;

                        // ✔ parse time → DateTime?
                        DateTime? dt = TimeHelper.ParseFacebookTime(raw);

                        if (dt.HasValue)
                        {
                            timeList.Add(dt.Value);
                        }
                    }
                }
            }

            if (timeList.Count == 0)
                return null;

            // ✔ lấy mốc MỚI NHẤT (LastPostTime)
            return timeList.Max();
        }

        //============HÀM CHÍNH
        public async Task<PageInfo> ScanPageInfoAsync(IPage page, string pageUrl)
        {
            Libary.Instance.LogDebug($"▶ Bắt đầu ScanPageInfoAsync | URL={pageUrl}");

            var info = new PageInfo();

            await page.GotoAsync(pageUrl, new PageGotoOptions
            {
                Timeout = AppConfig.DEFAULT_TIMEOUT,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            Libary.Instance.LogDebug("GotoAsync OK");

            info.PageLink = page.Url;
            int idxHash = info.PageLink.IndexOf("#");
            if (idxHash > 0)
                info.PageLink = info.PageLink.Substring(0, idxHash);

            Libary.Instance.LogDebug($"PageLink clean = {info.PageLink}");

            await page.WaitForTimeoutAsync(2000);

            string HTML = await page.ContentAsync();
            Libary.Instance.LogDebug($"HTML length = {HTML?.Length ?? 0}");

            DateTime? time = await GetPostTimeAsync(page);
            Libary.Instance.LogDebug($"TimeLastPost = {(time.HasValue ? time.Value.ToString() : "null")}");

            // 2️⃣ Check Type
            var type = await PageDAO.Instance.CheckFBTypeAsync(page);
            info.PageType = type; // enum

            Libary.Instance.LogDebug($"FBType = {type}");

            info.TimeLastPost = time;

            // 3️⃣ Nếu là Group → đọc ABOUT
            if (type == FBType.GroupOn || type == FBType.GroupOff)
            {
                Libary.Instance.LogDebug("Đang xử lý GROUP");

                var about = await GetGroupAboutAsync(page, info.PageLink);

                info.PageMembers = about.MemberShort != "N/A"
                    ? about.MemberShort
                    : about.MemberTotal;

                info.PageInfoText = "/Ngày tạo: " + about.CreatedDateExact;
                info.PageName = about.PageName;
                info.IDFBPage = about.IDFB;

                Libary.Instance.LogDebug(
                    $"GROUP | Name={info.PageName} | Members={info.PageMembers} | IDFB={info.IDFBPage}"
                );
            }
            // 4️⃣ Nếu là Fanpage → lấy followers
            else if (type == FBType.Fanpage)
            {
                Libary.Instance.LogDebug("Đang xử lý FANPAGE");

                var about = await GetFanpageAboutAsync(page, info.PageLink, HTML);

                info.PageMembers = about.MemberTotal;
                info.PageInfoText = about.PageInfo;
                info.PageName = about.PageName;
                info.IDFBPage = about.IDFB;

                Libary.Instance.LogDebug(
                    $"FANPAGE | Name={info.PageName} | Followers={info.PageMembers} | IDFB={info.IDFBPage}"
                );
            }
            // 5️⃣ Nếu là Person → lấy followers + friends
            else if (type == FBType.Person || type == FBType.PersonKOL)
            {
                Libary.Instance.LogDebug($"PERSON | Type={type} | Không đọc members");
                // info.PageMembers = ...
                // info.PageInteraction = ...
            }

            Libary.Instance.LogDebug("✔ Kết thúc ScanPageInfoAsync");

            return info;
        }
        //================HÀM PHỤ TO LẤY GROUPS
        public async Task<GroupAboutInfo> GetGroupAboutAsync(IPage page, string groupUrl)
        {
            GroupAboutInfo info = new GroupAboutInfo();
            string idfbbasic = ProcessingHelper.ExtractIdFromUrl(groupUrl);
            string clean = ProcessingHelper.NormalizeFacebookUrl(groupUrl);
            string aboutUrl = clean + "/about/";
            Libary.Instance.LogTech("Link trong about: "+aboutUrl);
            await page.GotoAsync(aboutUrl);
            await page.WaitForTimeoutAsync(1500);
            if (!string.IsNullOrEmpty(idfbbasic))
            {
                info.IDFB = idfbbasic;
            }
            else
            {
                // Không có id trong URL -> parse HTML để lấy groupID
                string html = await page.ContentAsync();
                var g = Regex.Match(html, "\"groupID\"\\s*:\\s*\"(\\d+)\"");
                if (g.Success) info.IDFB = g.Groups[1].Value;
            }
            var anchors = await page.QuerySelectorAllAsync("a[href]");
            if (anchors == null || anchors.Count == 0)
            {
                Libary.Instance.LogTech("❌ Không tìm thấy thẻ <a> nào.");
            }
            else
            {
                foreach (var a in anchors)
                {
                    string href = (await a.GetAttributeAsync("href") ?? "").Trim();
                    string text = (await a.InnerTextAsync() ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(text))
                        continue;

                    string hrefLower = href.ToLower();
                    string cleanGroupUrl = groupUrl.TrimEnd('/').ToLower();

                    // --- MATCH CẢ 3 DẠNG ---
                    bool match =
                        hrefLower.Contains(cleanGroupUrl) ||                                    // full URL
                        (hrefLower.Contains("/groups/") && hrefLower.EndsWith("/")) &&         // relative /groups/xxx/
                        !hrefLower.Contains("/members") && !hrefLower.Contains("/admin") &&
                        !hrefLower.Contains("/rules") && !hrefLower.Contains("/files");
                    if (match)
                    {
                        info.PageName = text;
                    }
                    else
                    {
                        var ElName = await page.QuerySelectorAllAsync("div [class = 'x1e56ztr x1xmf6yo']");
                        if (anchors == null || anchors.Count == 0)
                        {
                            Libary.Instance.LogTech("❌ Không lấy được Name");
                        }
                        else
                        {
                            try
                            {
                                // phần tử đầu tiên
                                var first = ElName[0];

                                // lấy text
                                string name = (await first.InnerTextAsync() ?? "").Trim();

                                Libary.Instance.LogTech($"📌 NAME FOUND: {name}");

                                info.PageName = name;   // gán vào DTO nếu cần
                            }
                            catch (Exception ex)
                            {
                                Libary.Instance.LogTech("❌ Lỗi lấy Name: " + ex.Message);
                            }
                        }
                    }
                    // 👉 Gán vào info.PageName
                    // Thoát ngay vì đã lấy được tên group đúng
                    break;
                }
            }
            var blocks = await page.QuerySelectorAllAsync("div.xu06os2.x1ok221b");
            foreach (var block in blocks)
            {
                string txt = (await block.InnerTextAsync() ?? "").Trim();
                // 1️⃣ Lấy ngày tạo (mọi dạng)
                if (txt.Contains("tạo"))   // thay vì StartsWith
                {
                    string date = ExtractCreatedDate(txt);
                    if (date != "N/A")
                    {
                        info.CreatedDateExact = date;
                        Libary.Instance.LogTech($"📅 CREATED DATE = {date}");
                    }
                }
                // Block 10 → Thành viên dạng ngắn
                if (txt.Contains("Thành viên") && txt.Contains("·"))
                    info.MemberShort = ExtractShortMembers(txt);
                // Block 15 → Tổng thành viên dạng dài
                if (txt.Contains("Tổng cộng"))
                    info.MemberTotal = ExtractTotalMembers(txt);
            }
            return info;
        }
        //==================HÀM PHỤ TO LẤY FANPAGE
        public async Task<FanpageAboutInfo> GetFanpageAboutAsync(IPage page, string Url, string HTML)
        {
            FanpageAboutInfo info = new FanpageAboutInfo();
            string idfbbasic = ProcessingHelper.ExtractIdFromUrl(Url);
            string clean = ProcessingHelper.NormalizeFacebookUrl(Url);
            string aboutUrl = "";
            if (clean.IndexOf("profile.php", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (clean.IndexOf("sk=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    if (clean.Contains("?"))
                        aboutUrl = clean + "&sk=about";
                    else
                        aboutUrl = clean + "?sk=about";
                }
                
            }
            else aboutUrl = clean + "/about/";
            Libary.Instance.LogTech($"[TEST] AboutUrl = {aboutUrl}");

            await page.GotoAsync(aboutUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = AppConfig.DEFAULT_TIMEOUT
            });
            if (!string.IsNullOrEmpty(idfbbasic))
            {
                // Nếu URL đã chứa ID thì lấy luôn
                info.IDFB = idfbbasic;
            }
            else
            {
                // Nếu không, parse HTML của /about để tìm pageID
                string html = await page.ContentAsync();
                var idfb = await DumpPageIdOnlyAsync(html);
                if (idfb != null && idfb.Count > 0)
                    info.IDFB = idfb.First();
            }
            try
            {
                string text = "";

                // 1️⃣ Ưu tiên selector UI mới (lấy rộng)
                var eleFollowers = await page.QuerySelectorAsync(
                    "a.x1i10hfl.xjbqb8w.x1ejq31n"
                );

                if (eleFollowers != null)
                {
                    text = (await eleFollowers.EvaluateAsync<string>(
                        "el => el.textContent"
                    ))?.Trim();
                }

                // ❗ VALIDATE TEXT
                if (!IsValidFollowerText(text))
                {
                    Libary.Instance.LogTech("⚠ Text followers không hợp lệ, fallback…");

                    // 2️⃣ fallback: lấy đúng strong + text node
                    text = await page.EvaluateAsync<string>(@"
            () => {
                const a = document.querySelector(
                    'a.x1i10hfl.xjbqb8w.x1ejq31n[href*=""followers""]'
                );
                if (!a) return '';

                const strong = a.querySelector('strong');
                if (!strong) return '';

                let result = strong.textContent || '';
                let node = strong.nextSibling;
                if (node && node.nodeType === Node.TEXT_NODE)
                    result += node.textContent;

                return result.trim();
            }
        ");
                }

                if (IsValidFollowerText(text))
                {
                    info.MemberTotal = text;
                    Libary.Instance.LogTech("Followers thu được: " + text);
                }
                else
                {
                    info.MemberTotal = "N/A";
                    Libary.Instance.LogTech("❌ Followers: không lấy được text hợp lệ");
                }
            }
            catch (Exception ex)
            {
                info.MemberTotal = "N/A";
                Libary.Instance.LogTech("[Followers] ERROR: " + ex.Message);
            }

            try
            {
                var eleName = await page.QuerySelectorAsync("div.x1e56ztr.x1xmf6yo>span");               
                info.PageName = eleName != null ? (await eleName.InnerTextAsync()).Trim(): "N/A";
                Libary.Instance.LogTech("Name thu được: " + info.PageName, AppConfig.ENABLE_LOG);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech($"[GetFanpageAbout: Lỗi lấy Tên Page]{ex.Message}");
            } 
            // Do phải lấy k about nên phải truyền HTML ngoài vào
            try
            {
                string fullText = ProcessingHelper.ExtractPageInfoFromHtml(HTML);
                info.PageInfo = string.IsNullOrEmpty(fullText) ? "N/A" : fullText.Trim();
            }
            catch (Exception ex)
            {
                info.PageInfo = "N/A";
                Libary.Instance.LogTech("[FanpageAbout] Lỗi parse PageInfo: " + ex.Message);
            }
            return info;
        }
        //========== lấy flo
        public async Task<string> GetFollowersTextAsync(IPage page)
        {
            try
            {
                var a = await page.QuerySelectorAsync(
                    "a.x1i10hfl.xjbqb8w.x1ejq31n[href*='followers']"
                );

                if (a == null)
                    return "N/A";

                return await a.EvaluateAsync<string>(@"
            el => {
                const strong = el.querySelector('strong');
                if (!strong) return '';

                let result = strong.textContent || '';

                let node = strong.nextSibling;
                if (node && node.nodeType === Node.TEXT_NODE) {
                    result += node.textContent;
                }

                return result.trim();
            }
        ");
            }
            catch
            {
                return "N/A";
            }
        }
        bool IsValidFollowerText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string t = text.ToLower();

            // phải có chữ followers
            if (!t.Contains("theo dõi") && !t.Contains("follower"))
                return false;

            // phải có số
            if (!System.Text.RegularExpressions.Regex.IsMatch(t, @"\d"))
                return false;

            return true;
        }

        // hàm bổ trợ mở about tránh thành profile
        public async Task<bool> ClickAboutFromProfileTabsAsync(IPage page, int timeoutMs = 10000)
        {
            try
            {
                Libary.Instance.LogTech("Try click About from ProfileTabs");

                // Đợi ProfileTabs xuất hiện
                var tabs = await page.WaitForSelectorAsync(
                    "div[aria-orientation='horizontal']",
                    new PageWaitForSelectorOptions
                    {
                        Timeout = timeoutMs
                    });

                if (tabs == null)
                {
                    Libary.Instance.LogTech("ProfileTabs not found");
                    return false;
                }

                // Tìm thẻ a có href chứa 'about'
                var aboutLink = await tabs.QuerySelectorAsync("a[href*='about']");

                if (aboutLink == null)
                {
                    Libary.Instance.LogTech("About link not found inside ProfileTabs");
                    return false;
                }

                // Click giống người thật
                await aboutLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                Libary.Instance.LogTech("Click About SUCCESS");
                return true;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("ClickAboutFromProfileTabs ERROR: " + ex.Message);
                return false;
            }
        }

      
        ///=========THU THẬP ID===================
        public Task<List<string>> DumpPageIdOnlyAsync(string html)
        {
           
            // pageID chính
            var matches1 = Regex.Matches(html, "\"pageID\":\"(\\d+)\"");

            // fallback: page_id
            var matches2 = Regex.Matches(html, "\"page_id\":\"(\\d+)\"");

            var list = new List<string>();

            foreach (Match m in matches1)
                if (m.Success) list.Add(m.Groups[1].Value);

            foreach (Match m in matches2)
                if (m.Success) list.Add(m.Groups[1].Value);

            return Task.FromResult(list.Distinct().ToList());
        }
        public async Task<string> ExtractProfileIdFromPhotoHrefAsync(IPage page)
        {
            Libary.Instance.LogDebug("▶ Bắt đầu ExtractProfileIdFromPhotoHrefAsync");
            // Lấy toàn bộ href từ trang
            var hrefs = await page.EvaluateAsync<string[]>(@"Array.from(document.querySelectorAll('a[href]')).map(a => a.getAttribute('href'))");
            Libary.Instance.LogDebug($"Tổng href lấy được: {hrefs?.Length ?? 0}");
            Dictionary<string, int> countMap = new Dictionary<string, int>();
            // Regex lấy UID sau pb.
            Regex pbRegex = new Regex(@"set=pb\.(\d+)\.", RegexOptions.Compiled);

            foreach (var href in hrefs)
            {
                if (string.IsNullOrEmpty(href)) continue;

                // Chỉ lấy link chứa facebook.com/photo/…set=pb.
                if (!href.Contains("/photo/") || !href.Contains("set=pb."))
                    continue;
                var m = pbRegex.Match(href);
                if (m.Success)
                {
                    string uid = m.Groups[1].Value;

                    if (!countMap.ContainsKey(uid))
                        countMap[uid] = 0;
                    countMap[uid]++;
                }
                Libary.Instance.LogDebug($"| UID tìm được: {countMap.Count}");
            }
            if (countMap.Count == 0)
            {
                Libary.Instance.LogDebug("❌ Không tìm được UID từ photo href");
                return null;
            }

            // UID xuất hiện nhiều nhất = chính xác UID Profile
            var best = countMap.OrderByDescending(x => x.Value).First();

            Libary.Instance.LogDebug(
                $"✅ UID chọn: {best.Key} | Số lần xuất hiện: {best.Value}"
            );
            return best.Key;
        }

      

    }
}
